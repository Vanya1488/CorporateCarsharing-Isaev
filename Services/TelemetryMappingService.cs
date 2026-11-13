using System.Globalization;
using CorporateCarsharing.Repositories;

namespace CorporateCarsharing.Services;

public enum TelemetryResultKind { Created, Duplicate, BadRequest, NotFound }
public sealed record TelemetryResult(TelemetryResultKind Kind, VehicleTelemetry? Telemetry = null, string? Error = null);

/// <summary>Изолированное сопоставление внешней телеметрии с внутренней моделью.</summary>
public sealed class TelemetryMappingService(CarsharingRepository repository)
{
    private readonly object gate = new();
    private readonly Dictionary<string, VehicleTelemetry> received = new(StringComparer.Ordinal);

    public TelemetryResult Ingest(TelemetryInput input)
    {
        var eventId = input.EventId?.Trim();
        var vehicleCode = input.VehicleCode?.Trim();
        if (string.IsNullOrWhiteSpace(eventId) || string.IsNullOrWhiteSpace(vehicleCode))
            return new(TelemetryResultKind.BadRequest, Error: "event_id и vehicle_code обязательны.");
        if (!DateTimeOffset.TryParse(input.EventTime, CultureInfo.InvariantCulture,
                DateTimeStyles.None, out var parsedTime) ||
            !(input.EventTime?.EndsWith("Z", StringComparison.OrdinalIgnoreCase) == true ||
              input.EventTime?.Contains('+') == true ||
              HasExplicitNegativeOffset(input.EventTime)))
            return new(TelemetryResultKind.BadRequest, Error: "event_time должен содержать часовую зону ISO 8601.");
        if (input.Latitude < -90 || input.Latitude > 90 ||
            input.Longitude < -180 || input.Longitude > 180 || input.OdometerMeters < 0)
            return new(TelemetryResultKind.BadRequest, Error: "Некорректные координаты или пробег.");

        // В действующей модели Vehicle нет ExternalId: связываем по PlateNumber.
        var vehicle = repository.GetVehicles().FirstOrDefault(v =>
            string.Equals(v.PlateNumber, vehicleCode, StringComparison.OrdinalIgnoreCase));
        if (vehicle is null)
            return new(TelemetryResultKind.NotFound, Error: "Автомобиль с vehicle_code не найден.");

        var telemetry = new VehicleTelemetry(eventId, vehicle.Id, parsedTime.UtcDateTime,
            input.Latitude, input.Longitude, input.OdometerMeters / 1000m, input.Ignition);
        lock (gate)
        {
            if (received.TryGetValue(eventId, out var existing))
                return new(TelemetryResultKind.Duplicate, existing);
            received.Add(eventId, telemetry);
        }
        return new(TelemetryResultKind.Created, telemetry);
    }

    private static bool HasExplicitNegativeOffset(string? value)
    {
        // Смещение -HH:mm относится к концу ISO 8601, не к дефисам даты.
        return value.Length >= 6 && value[^6] == '-' && value[^3] == ':';
    }

    public IReadOnlyList<VehicleTelemetry> GetAll()
    {
        lock (gate) return received.Values.ToArray();
    }
}
