using CorporateCarsharing.Core.Interfaces;
using CorporateCarsharing.Core.Models;

namespace CorporateCarsharing.Core.Services;

public enum BookingResultKind { Created, BadRequest, NotFound, Conflict }
public sealed record BookingResult(BookingResultKind Kind, Booking? Booking = null, string? Error = null);

/// <summary>Правила создания бронирования, не зависящие от HTTP или Swagger.</summary>
public sealed class BookingService(ICarsharingRepository repository)
{
    public BookingResult Create(CreateBookingRequest request)
    {
        if (request.VehicleId <= 0 ||
            request.StartAt == default || request.EndAt == default ||
            request.StartAt.Kind != DateTimeKind.Utc || request.EndAt.Kind != DateTimeKind.Utc ||
            request.EndAt <= request.StartAt)
            return new(BookingResultKind.BadRequest, Error: "Требуются vehicleId > 0 и интервал startAt/endAt в UTC (суффикс Z), endAt позже startAt.");

        var vehicle = repository.FindVehicle(request.VehicleId);
        if (vehicle is null)
            return new(BookingResultKind.NotFound, Error: "Автомобиль не найден.");
        if (vehicle.Status != "Available")
            return new(BookingResultKind.Conflict, Error: "Автомобиль недоступен.");

        var booking = repository.AddIfNoOverlap(request);
        if (booking is null)
            return new(BookingResultKind.Conflict, Error: "Интервал уже занят.");
        return new(BookingResultKind.Created, booking);
    }
}
