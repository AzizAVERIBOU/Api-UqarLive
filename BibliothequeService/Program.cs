using Microsoft.EntityFrameworkCore;
using BibliothequeService.Data;

var builder = WebApplication.CreateBuilder(args);

// Configuration du port d'écoute
builder.WebHost.UseUrls("http://+:5004");

// Forcer le mode développement
builder.Environment.EnvironmentName = "Development";

// Add services to the container.
builder.Services.AddDbContext<BibliothequeDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = null;
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "BibliothequeService API", Version = "v1" });
});

// Configure HTTP client for inter-service communication
builder.Services.AddHttpClient("AuthService", client =>
{
    client.BaseAddress = new Uri("http://localhost:5001/");
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "BibliothequeService API V1");
    });
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
