using Microsoft.EntityFrameworkCore;
using AssociationService.Data;
using System.Globalization;

var builder = WebApplication.CreateBuilder(args);

// Configuration des logs
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

// Configuration de la culture
var defaultCulture = new CultureInfo("fr-CA");
CultureInfo.DefaultThreadCurrentCulture = defaultCulture;
CultureInfo.DefaultThreadCurrentUICulture = defaultCulture;

// Configuration JSON
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
        options.JsonSerializerOptions.WriteIndented = true;
    });

// Ajout des services
builder.Services.AddMvc();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Service Association API",
        Version = "v1",
        Description = "API pour la gestion des associations"
    });
});

// Configuration de la base de données
builder.Services.AddDbContext<AssociationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"), 
        sqlServerOptionsAction: sqlOptions =>
        {
            sqlOptions.EnableRetryOnFailure(
                maxRetryCount: 3,
                maxRetryDelay: TimeSpan.FromSeconds(5),
                errorNumbersToAdd: null);
        }));

// Ajouter les services HTTP
builder.Services.AddHttpClient("AuthService", client =>
{
    // Utiliser le Gateway Azure pour la communication inter-services
    client.BaseAddress = new Uri("https://uqarlivegateway-hbayescme8ckf7e8.canadacentral-01.azurewebsites.net/");
});

// Configuration du port d'écoute
builder.WebHost.UseUrls("http://0.0.0.0:5002");

var app = builder.Build();

// Configuration du pipeline HTTP
// Swagger toujours activé (utile en conteneur)
app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

// Endpoints de santé et de statut
app.MapGet("/", () => Results.Ok("Association service up"));
app.MapGet("/health", () => Results.Ok("Healthy"));

app.Logger.LogInformation("Service Association démarré sur le port 5002");

app.Run();
