using BibliothequeService.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException(
        "ConnectionStrings:DefaultConnection est absente. Définissez la variable d'environnement ConnectionStrings__DefaultConnection.");
}

builder.Services.AddDbContext<BibliothequeDbContext>(options =>
    BibliothequeDbContext.UtiliserPostgres(options, connectionString));

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
        Title = "UQAR Live — Bibliothèque",
        Version = "v1",
        Description = """
            Salles et réservations.
            Les lectures sont ouvertes. Créer une réservation exige un code permanent qui correspond à un compte actif.
            """
    });
    c.SchemaFilter<BibliothequeService.Swagger.ExemplesSwagger>();
    c.OperationFilter<BibliothequeService.Swagger.DescriptionsSwagger>();
});

var authService = builder.Configuration["Services:AuthService"];
if (string.IsNullOrWhiteSpace(authService))
{
    authService = "https://api-uqarlive.onrender.com";
}

builder.Services.AddHttpClient("AuthService", client =>
{
    client.BaseAddress = new Uri(authService.TrimEnd('/') + "/");
    client.Timeout = TimeSpan.FromSeconds(90);
});

var environnement = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
if (!string.Equals(environnement, "Testing", StringComparison.OrdinalIgnoreCase))
{
    var port = Environment.GetEnvironmentVariable("PORT") ?? "5004";
    builder.WebHost.UseUrls($"http://0.0.0.0:{port}");
}

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<BibliothequeDbContext>();
    db.Database.Migrate();
}

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "UQAR Live — Bibliothèque");
    c.DocumentTitle = "UQAR Live — Bibliothèque";
    c.DefaultModelsExpandDepth(2);
});

app.UseAuthorization();
app.MapControllers();

app.MapGet("/", () => Results.Ok("Bibliotheque service up"))
    .WithTags("Santé")
    .WithSummary("Le processus est démarré.");
app.MapGet("/health", () => Results.Ok("Healthy"))
    .WithTags("Santé")
    .WithSummary("Sonde utilisée par Render.");

app.Logger.LogInformation(
    "Service Bibliothèque démarré sur le port {Port}",
    Environment.GetEnvironmentVariable("PORT") ?? "5004");

app.Run();

public partial class Program
{
}
