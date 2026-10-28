using System.Globalization;
using System.Net;
using System.Text.Json;
using Pos.Stock.Client.Models;

namespace Pos.Stock.Client.Services;

public sealed class ApiClient(HttpClient httpClient)
{
    private const string NotReachable =
        "Cannot reach the API. Check that it is running and that CORS allows this origin.";

    // ---------- Health ----------

    public async Task<HttpStatusCode> GetHealthAsync(CancellationToken cancellationToken = default)
    {
        using var response = await httpClient.GetAsync("health", cancellationToken);
        return response.StatusCode;
    }

    // ---------- Categories ----------

    public Task<ApiResult<List<CategoryDto>>> GetCategoriesAsync() =>
        SendAsync<List<CategoryDto>>(() => httpClient.GetAsync("api/categories"));

    public Task<ApiResult> CreateCategoryAsync(CategoryForm form) =>
        SendAsync(() => httpClient.PostAsJsonAsync("api/categories", form));

    public Task<ApiResult> DeleteCategoryAsync(int id) =>
        SendAsync(() => httpClient.DeleteAsync($"api/categories/{id}"));

    // ---------- Products ----------

    public Task<ApiResult<List<ProductDto>>> GetProductsAsync(string? search = null)
    {
        var url = string.IsNullOrWhiteSpace(search)
            ? "api/products"
            : $"api/products?search={Uri.EscapeDataString(search.Trim())}";

        return SendAsync<List<ProductDto>>(() => httpClient.GetAsync(url));
    }

    public Task<ApiResult<List<ProductDto>>> GetLowStockAsync() =>
        SendAsync<List<ProductDto>>(() => httpClient.GetAsync("api/products/low-stock"));

    public Task<ApiResult> CreateProductAsync(ProductForm form) =>
        SendAsync(() => httpClient.PostAsJsonAsync("api/products", form));

    public Task<ApiResult> UpdateProductAsync(int id, ProductForm form) =>
        SendAsync(() => httpClient.PutAsJsonAsync($"api/products/{id}", form));

    public Task<ApiResult> DeactivateProductAsync(int id) =>
        SendAsync(() => httpClient.DeleteAsync($"api/products/{id}"));

    // ---------- Stock ----------

    public Task<ApiResult> AdjustStockAsync(StockAdjustmentForm form) =>
        SendAsync(() => httpClient.PostAsJsonAsync("api/stock/adjust", form));

    public Task<ApiResult<List<StockMovementDto>>> GetMovementsAsync(int? productId = null)
    {
        var url = productId is null
            ? "api/stock/movements"
            : $"api/stock/movements?productId={productId}";

        return SendAsync<List<StockMovementDto>>(() => httpClient.GetAsync(url));
    }

    // ---------- Sales ----------

    public Task<ApiResult<SaleDto>> CreateSaleAsync(SaleRequest request) =>
        SendAsync<SaleDto>(() => httpClient.PostAsJsonAsync("api/sales", request));

    public Task<ApiResult<List<SaleDto>>> GetSalesAsync() =>
        SendAsync<List<SaleDto>>(() => httpClient.GetAsync("api/sales"));

    public Task<ApiResult<DailyReportDto>> GetDailyReportAsync(DateOnly date)
    {
        var text = date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
        return SendAsync<DailyReportDto>(() => httpClient.GetAsync($"api/sales/report/daily?date={text}"));
    }

    // ---------- Helpers ----------

    private static async Task<ApiResult> SendAsync(Func<Task<HttpResponseMessage>> send)
    {
        try
        {
            using var response = await send();

            return response.IsSuccessStatusCode
                ? ApiResult.Success()
                : ApiResult.Fail(await ReadErrorAsync(response));
        }
        catch (HttpRequestException)
        {
            return ApiResult.Fail(NotReachable);
        }
    }

    private static async Task<ApiResult<T>> SendAsync<T>(Func<Task<HttpResponseMessage>> send)
    {
        try
        {
            using var response = await send();

            if (!response.IsSuccessStatusCode)
            {
                return ApiResult<T>.Fail(await ReadErrorAsync(response));
            }

            var value = await response.Content.ReadFromJsonAsync<T>();

            return value is null
                ? ApiResult<T>.Fail("The API returned an empty response.")
                : ApiResult<T>.Success(value);
        }
        catch (HttpRequestException)
        {
            return ApiResult<T>.Fail(NotReachable);
        }
        catch (JsonException)
        {
            return ApiResult<T>.Fail("The API returned data in an unexpected format.");
        }
    }

    private static async Task<string> ReadErrorAsync(HttpResponseMessage response)
    {
        try
        {
            var text = await response.Content.ReadAsStringAsync();

            if (!string.IsNullOrWhiteSpace(text))
            {
                using var document = JsonDocument.Parse(text);
                var root = document.RootElement;

                if (root.ValueKind == JsonValueKind.Object)
                {
                    // Our own errors: { "error": "..." }
                    if (root.TryGetProperty("error", out var error) && error.ValueKind == JsonValueKind.String)
                    {
                        return error.GetString() ?? "Request failed.";
                    }

                    // ASP.NET validation errors: { "errors": { "Field": ["message"] } }
                    if (root.TryGetProperty("errors", out var errors) && errors.ValueKind == JsonValueKind.Object)
                    {
                        foreach (var field in errors.EnumerateObject())
                        {
                            if (field.Value.ValueKind == JsonValueKind.Array && field.Value.GetArrayLength() > 0)
                            {
                                return $"{field.Name}: {field.Value[0].GetString()}";
                            }
                        }
                    }

                    if (root.TryGetProperty("title", out var title) && title.ValueKind == JsonValueKind.String)
                    {
                        return title.GetString() ?? "Request failed.";
                    }
                }
            }
        }
        catch (JsonException)
        {
            // Not JSON, fall through to the generic message.
        }

        return $"Request failed (HTTP {(int)response.StatusCode}).";
    }
}
