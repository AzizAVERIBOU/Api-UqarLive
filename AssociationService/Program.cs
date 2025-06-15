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
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Ajouter les services HTTP
builder.Services.AddHttpClient("AuthService", client =>
{
    client.BaseAddress = new Uri("http://localhost:5000/");
});

var app = builder.Build();

// Configuration du pipeline HTTP
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Logger.LogInformation("Service Association démarré sur le port 5006");

app.Run();
