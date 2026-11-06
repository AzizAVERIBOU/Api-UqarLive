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
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var key = Encoding.ASCII.GetBytes(jwtSettings["SecretKey"]);

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
        Title = "Service d'Authentification API",
        Version = "v1",
        Description = "API pour la gestion de l'authentification des utilisateurs"
    });

    // Configuration de Swagger pour JWT (Microsoft.OpenApi 2.x / Swashbuckle 10)
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
    });

    c.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        [new OpenApiSecuritySchemeReference("Bearer", document, externalResource: null)] = []
    });
});

var port = Environment.GetEnvironmentVariable("PORT") ?? "5001";
builder.WebHost.UseUrls($"http://0.0.0.0:{port}");

var app = builder.Build();

// Swagger toujours activé (utile en conteneur)
app.UseSwagger();
app.UseSwaggerUI();

// app.UseHttpsRedirection(); // désactivé en conteneur si pas de TLS frontal

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// Endpoints de santé et de statut
app.MapGet("/", () => Results.Ok("Auth service up"));
app.MapGet("/health", () => Results.Ok("Healthy"));

app.Logger.LogInformation("Service d'Authentification démarré sur le port {Port}", port);

app.Run();
