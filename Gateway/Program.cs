using Ocelot.DependencyInjection;
using Ocelot.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Ajout de la configuration Ocelot
builder.Configuration.AddJsonFile("ocelot.json", optional: false, reloadOnChange: true);

// Ajout des services Ocelot
builder.Services.AddOcelot(builder.Configuration);

// Ajout de l'enregistrement du service SwaggerAggregatorService
builder.Services.AddScoped<Gateway.Services.SwaggerAggregatorService>();

// Enregistrement de IHttpClientFactory
builder.Services.AddHttpClient();

// Ajout des services Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Gateway API",
        Version = "v1",
        Description = "API Gateway pour notre app console uqar live"
    });
});

var app = builder.Build();


    app.UseSwagger();
    app.UseSwaggerUI();


app.UseHttpsRedirection();

// Utilisation d'Ocelot
await app.UseOcelot();

app.Run();
