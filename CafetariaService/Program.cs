using CafetariaService.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException(
        "ConnectionStrings:DefaultConnection est absente. Définissez la variable d'environnement ConnectionStrings__DefaultConnection.");
}

builder.Services.AddDbContext<CafetariaDbContext>(options =>
    CafetariaDbContext.UtiliserPostgres(options, connectionString));

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "UQAR Live — Cafétéria",
        Version = "v1",
        Description = """
            Catalogue, menu du jour et horaires de la semaine.
            Les lectures et les écritures sont ouvertes. Le menu du jour suit le calendrier du Québec.
            """
    });
    c.SchemaFilter<CafetariaService.Swagger.ExemplesSwagger>();
    c.OperationFilter<CafetariaService.Swagger.DescriptionsSwagger>();
});

var environnement = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
if (!string.Equals(environnement, "Testing", StringComparison.OrdinalIgnoreCase))
{
    var port = Environment.GetEnvironmentVariable("PORT") ?? "5005";
    builder.WebHost.UseUrls($"http://0.0.0.0:{port}");
}

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<CafetariaDbContext>();
    db.Database.Migrate();
}

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "UQAR Live — Cafétéria");
    c.DocumentTitle = "UQAR Live — Cafétéria";
    c.DefaultModelsExpandDepth(2);
});

app.UseAuthorization();
app.MapControllers();

app.MapGet("/", () => Results.Ok("Cafetaria service up"))
    .WithTags("Santé")
    .WithSummary("Le processus est démarré.");
app.MapGet("/health", () => Results.Ok("Healthy"))
    .WithTags("Santé")
    .WithSummary("Sonde utilisée par Render.");

app.Logger.LogInformation(
    "Service Cafétéria démarré sur le port {Port}",
    Environment.GetEnvironmentVariable("PORT") ?? "5005");

app.Run();

public partial class Program
{
}
