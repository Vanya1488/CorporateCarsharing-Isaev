using System.Text.Json.Serialization;

// Контракт внешней системы: snake_case; в C# используются типизированные поля.
public sealed record TelemetryInput(
    [property: JsonPropertyName("event_id")] string? EventId,
    [property: JsonPropertyName("vehicle_code")] string? VehicleCode,
    [property: JsonPropertyName("event_time")] string? EventTime,
    [property: JsonPropertyName("lat")] decimal Latitude,
    [property: JsonPropertyName("lon")] decimal Longitude,
    [property: JsonPropertyName("odometer_m")] long OdometerMeters,
    [property: JsonPropertyName("ignition")] bool Ignition);

public sealed record VehicleTelemetry(
    string MessageId, int VehicleId, DateTime ReceivedAtUtc,
    decimal Latitude, decimal Longitude, decimal OdometerKm, bool IgnitionOn);
