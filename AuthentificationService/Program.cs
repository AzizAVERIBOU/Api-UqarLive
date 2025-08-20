using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using AuthentificationService.Data;
using AuthentificationService.Models.Auth;
using Microsoft.OpenApi.Models;

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
// Récupérer le mot de passe de la variable d'environnement
var password = Environment.GetEnvironmentVariable("Password");

// Construire la chaîne de connexion complète
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// Configurer le DbContext avec la chaîne de connexion complète
builder.Services.AddDbContext<AuthentificationDbContext>(options =>
    options.UseSqlServer(connectionString));

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

    // Configuration de Swagger pour JWT
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

builder.WebHost.UseUrls("http://0.0.0.0:5001");

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

app.Logger.LogInformation("Service d'Authentification démarré sur le port 5001");

app.Run();
