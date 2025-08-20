using CafetariaService.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Configuration du port d'écoute
builder.WebHost.UseUrls("http://+:5005");

// Forcer le mode développement
builder.Environment.EnvironmentName = "Development";

// Configuration de la base de données (Azure SQL Database)
builder.Services.AddDbContext<CafetariaDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    options.UseSqlServer(connectionString, sqlServerOptionsAction: sqlOptions =>
    {
        sqlOptions.EnableRetryOnFailure(
            maxRetryCount: 5,
            maxRetryDelay: TimeSpan.FromSeconds(30),
            errorNumbersToAdd: null);
    });
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
        Title = "Cafetaria Service API",
        Version = "v1",
        Description = "API pour la gestion de la cafétéria (menus et horaires)"
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

app.Run();
