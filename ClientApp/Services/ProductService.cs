using ClientApp.Models;
using System.Net.Http.Json;

namespace ClientApp.Services;

public class ProductService
{
    private readonly HttpClient _httpClient;
    private readonly int _requestTimeoutSeconds = 10;
    private Product[]? _cachedProducts;
    private DateTime? _cacheExpiration;
    private readonly int _cacheDurationMinutes = 5;

    public ProductService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<(Product[]? Products, string? ErrorMessage)> GetProductsAsync(bool forceRefresh = false)
    {
        // Return cached data if available and not expired
        if (!forceRefresh && _cachedProducts != null && _cacheExpiration.HasValue && DateTime.Now < _cacheExpiration.Value)
        {
            Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] Client-side cache HIT - Returning cached products");
            return (_cachedProducts, null);
        }

        try
        {
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(_requestTimeoutSeconds));
            
            var response = await _httpClient.GetAsync("api/productlist", cts.Token);

            if (response.IsSuccessStatusCode)
            {
                var products = await response.Content.ReadFromJsonAsync<Product[]>(cancellationToken: cts.Token);
                
                if (products == null)
                {
                    return (null, "Received invalid data from the server.");
                }

                // Cache the results
                _cachedProducts = products;
                _cacheExpiration = DateTime.Now.AddMinutes(_cacheDurationMinutes);
                Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] Client-side cache MISS - Products fetched and cached");

                return (products, null);
            }
            else
            {
                return (null, $"Server returned error: {response.StatusCode} - {response.ReasonPhrase}");
            }
        }
        catch (TaskCanceledException)
        {
            return (null, $"Request timed out after {_requestTimeoutSeconds} seconds. Please check your connection and try again.");
        }
        catch (HttpRequestException ex)
        {
            return (null, $"Network error: Unable to connect to the server. {ex.Message}");
        }
        catch (System.Text.Json.JsonException ex)
        {
            return (null, $"Invalid response format: {ex.Message}");
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error fetching products: {ex}");
            return (null, $"An unexpected error occurred: {ex.Message}");
        }
    }

    public void ClearCache()
    {
        _cachedProducts = null;
        _cacheExpiration = null;
        Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] Client-side cache cleared");
    }
}

