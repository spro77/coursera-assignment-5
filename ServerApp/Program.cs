var builder = WebApplication.CreateBuilder(args);

// Add CORS services
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowBlazorClient",
        policy =>
        {
            policy.WithOrigins("http://localhost:5202", "https://localhost:7257")
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        });
});

var app = builder.Build();

// Enable CORS
app.UseCors("AllowBlazorClient");

app.MapGet("/api/products", () =>
{
    return new[]
    {
        new { Id = 1, Name = "Laptop", Price = 1200.50, Stock = 25 },
        new { Id = 2, Name = "Headphones", Price = 50.00, Stock = 100 }
    };
});

app.Run();
