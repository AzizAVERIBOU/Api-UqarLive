using MarketService.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Configuration du port d'écoute
builder.WebHost.UseUrls("http://+:5003");

// Forcer le mode développement
builder.Environment.EnvironmentName = "Development";

// Configuration de la base de données (Azure SQL Database)
builder.Services.AddDbContext<MarketDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Ajouter les services HTTP pour communiquer avec le service d'authentification
builder.Services.AddHttpClient("AuthService", client =>
{
    client.BaseAddress = new Uri("http://localhost:5001/");
});

// Ajout des services MVC
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
        options.JsonSerializerOptions.WriteIndented = true;
    });

// Ajout de Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Market Service API",
        Version = "v1",
        Description = "API pour la gestion du marketplace universitaire"
    });
    
    // Filtrer les endpoints système
    c.DocInclusionPredicate((docName, apiDesc) =>
    {
        // Exclure les endpoints de configuration système
        if (apiDesc.RelativePath?.StartsWith("configuration") == true ||
            apiDesc.RelativePath?.StartsWith("outputcache") == true)
        {
            return false;
        }
        return true;
    });
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

app.Logger.LogInformation("Service Market démarré sur le port 5003");

app.Run();
