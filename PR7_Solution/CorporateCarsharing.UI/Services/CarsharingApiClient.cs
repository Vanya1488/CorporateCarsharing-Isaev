using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using CorporateCarsharing.Client.Models;

namespace CorporateCarsharing.Client.Services;
public sealed class CarsharingApiClient : IDisposable
{
    private readonly HttpClient client = new() { Timeout = TimeSpan.FromSeconds(15) };
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task<IReadOnlyList<VehicleDto>> GetVehiclesAsync(string url)
    {
        var baseUri = ValidateUrl(url);
        using var response = await client.GetAsync(new Uri(baseUri, "api/v1/vehicles"));
        await EnsureSuccess(response);
        return await response.Content.ReadFromJsonAsync<List<VehicleDto>>(JsonOptions)
               ?? new List<VehicleDto>();
    }
    public async Task<BookingDto> CreateBookingAsync(string url, CreateBookingRequest request)
    {
        var baseUri = ValidateUrl(url);
        using var response = await client.PostAsJsonAsync(new Uri(baseUri, "api/v1/bookings"), request, JsonOptions);
        await EnsureSuccess(response);
        return await response.Content.ReadFromJsonAsync<BookingDto>(JsonOptions)
               ?? throw new InvalidOperationException("Сервер вернул пустое бронирование.");
    }
    private static Uri ValidateUrl(string value)
    {
        if (!Uri.TryCreate(value.TrimEnd('/') + "/", UriKind.Absolute, out var uri) ||
            (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
            throw new ArgumentException("Укажите адрес API, например https://localhost:57000/");
        return uri;
    }
    private static async Task EnsureSuccess(HttpResponseMessage response)
    {
        if (response.IsSuccessStatusCode) return;
        var body = await response.Content.ReadAsStringAsync();
        string error = body;
        try { using var doc = JsonDocument.Parse(body); if (doc.RootElement.TryGetProperty("error", out var prop)) error = prop.GetString() ?? body; }
        catch (JsonException) { }
        throw new InvalidOperationException($"HTTP {(int)response.StatusCode}: {error}");
    }
    public void Dispose() => client.Dispose();
}
