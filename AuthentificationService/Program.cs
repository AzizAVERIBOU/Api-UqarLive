using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using AuthentificationService.Data;
using AuthentificationService.Models.Auth;
using Microsoft.OpenApi;

var builder = WebApplication.CreateBuilder(args);

// Configuration des logs
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

// Configuration JSON
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
        options.JsonSerializerOptions.WriteIndented = true;
    });

// Configuration de la base de données
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException(
        "ConnectionStrings:DefaultConnection est absente. Définissez la variable d'environnement ConnectionStrings__DefaultConnection.");
}

builder.Services.AddDbContext<AuthentificationDbContext>(options =>
    options.UseNpgsql(connectionString));

// Configuration JWT
const string cleJwtExemple = "votre_cle_secrete_tres_longue_et_complexe_ici";
var cleJwt = builder.Configuration["JwtSettings:SecretKey"];
if (string.IsNullOrWhiteSpace(cleJwt) || cleJwt == cleJwtExemple)
{
    throw new InvalidOperationException(
        "JwtSettings:SecretKey est absente ou encore égale à la valeur d'exemple. Définissez la variable d'environnement JwtSettings__SecretKey.");
}

var key = Encoding.ASCII.GetBytes(cleJwt);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = false,
        ValidateAudience = false,
        ClockSkew = TimeSpan.Zero
    };
});

// Enregistrement des services
builder.Services.AddScoped<JwtToken>();
builder.Services.AddScoped<ServiceAuthentification>();

// Ajout des services
builder.Services.AddMvc();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "UQAR Live — Authentification",
        Version = "v1",
        Description = """
            Comptes, jetons et vérification des codes permanents.
            L'inscription et la connexion sont ouvertes. Le profil, la mise à jour, la déconnexion et le changement de mot de passe exigent le bouton Authorize, avec le jeton reçu à la connexion.
            L'inscription enregistre toujours le rôle Etudiant (0). Le mot de passe compte au moins 8 caractères. Le jeton dure 60 minutes.
            """
    });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "Collez le jeton reçu à la connexion, sans le mot Bearer.",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
    });

    c.SchemaFilter<AuthentificationService.Swagger.ExemplesSwagger>();
    c.OperationFilter<AuthentificationService.Swagger.DescriptionsSwagger>();
});

var environnement = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
if (!string.Equals(environnement, "Testing", StringComparison.OrdinalIgnoreCase))
{
    var port = Environment.GetEnvironmentVariable("PORT") ?? "5001";
    builder.WebHost.UseUrls($"http://0.0.0.0:{port}");
}

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AuthentificationDbContext>();
    db.Database.Migrate();
}

// Swagger toujours activé (utile en conteneur)
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.DocumentTitle = "UQAR Live — Authentification";
    c.DefaultModelsExpandDepth(2);
    c.DisplayRequestDuration();
});

// app.UseHttpsRedirection(); // désactivé en conteneur si pas de TLS frontal

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// Endpoints de santé et de statut
app.MapGet("/", () => Results.Ok("Auth service up"))
    .WithTags("Santé")
    .WithSummary("Le processus est démarré.");
app.MapGet("/health", () => Results.Ok("Healthy"))
    .WithTags("Santé")
    .WithSummary("Sonde utilisée par Render.");

app.Logger.LogInformation(
    "Service d'Authentification démarré sur le port {Port}",
    Environment.GetEnvironmentVariable("PORT") ?? "5001");

app.Run();

public partial class Program
{
}
