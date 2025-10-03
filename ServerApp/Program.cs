using Microsoft.Extensions.Caching.Memory;

var builder = WebApplication.CreateBuilder(args);

// Add CORS services
builder.Services.AddCors();

// Add response caching services
builder.Services.AddResponseCaching();

// Add memory cache services
builder.Services.AddMemoryCache();

var app = builder.Build();

// Enable CORS
app.UseCors(policy =>
    policy.AllowAnyOrigin()
          .AllowAnyMethod()
          .AllowAnyHeader());

// Enable response caching middleware
app.UseResponseCaching();

app.MapGet("/api/productlist", (IMemoryCache cache) =>
{
    const string cacheKey = "productList";
    
    // Try to get cached data
    if (!cache.TryGetValue<object[]>(cacheKey, out var cachedProducts))
    {
        // Cache miss - generate the product list
        cachedProducts = GenerateProductList();
        
        // Set cache options
        var cacheOptions = new MemoryCacheEntryOptions()
            .SetSlidingExpiration(TimeSpan.FromMinutes(5))
            .SetAbsoluteExpiration(TimeSpan.FromMinutes(30))
            .SetPriority(CacheItemPriority.Normal);
        
        // Store in cache
        cache.Set(cacheKey, cachedProducts, cacheOptions);
        
        LogCacheActivity("MISS", "Product list generated and cached");
    }
    else
    {
        LogCacheActivity("HIT", "Product list served from cache");
    }
    
    return cachedProducts;
})
.CacheOutput(policy => policy.Expire(TimeSpan.FromMinutes(5))); // HTTP response caching for 5 minutes

app.Run();

// Helper methods for better code organization
static object[] GenerateProductList()
{
    return new[]
    {
        new
        {
            Id = 1,
            Name = "Laptop",
            Price = 1200.50,
            Stock = 25,
            Category = new { Id = 101, Name = "Electronics" }
        },
        new
        {
            Id = 2,
            Name = "Headphones",
            Price = 50.00,
            Stock = 100,
            Category = new { Id = 102, Name = "Accessories" }
        }
    };
}

static void LogCacheActivity(string status, string message)
{
    Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] Cache {status} - {message}");
}
