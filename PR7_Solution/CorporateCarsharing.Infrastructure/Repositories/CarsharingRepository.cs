using System.Collections.Generic;

using CorporateCarsharing.Core.Models;
using CorporateCarsharing.Core.Interfaces;
namespace CorporateCarsharing.Infrastructure.Repositories;

/// <summary>Учебное потокобезопасное хранилище в памяти (не база данных).</summary>
public sealed class CarsharingRepository : ICarsharingRepository
{
    private readonly object gate = new();
    private readonly List<Vehicle> vehicles = new()
    {
        new(1, "A123BC", "Skoda Octavia", "Available"),
        new(2, "B456DE", "Kia Ceed", "Available"),
        new(3, "C789FG", "Volkswagen Polo", "InUse")
    };
    private readonly List<Booking> bookings = new();
    private int lastBookingId;

    public IReadOnlyList<Vehicle> GetVehicles()
    {
        lock (gate) return vehicles.ToArray();
    }

    public IReadOnlyList<Booking> GetBookings()
    {
        lock (gate) return bookings.ToArray();
    }

    public Vehicle? FindVehicle(int vehicleId)
    {
        lock (gate) return vehicles.Find(v => v.Id == vehicleId);
    }

    public Booking? FindBooking(int bookingId)
    {
        lock (gate) return bookings.Find(b => b.Id == bookingId);
    }

    // Проверка конфликта и сохранение выполняются атомарно в одной блокировке.
    public Booking? AddIfNoOverlap(CreateBookingRequest request)
    {
        lock (gate)
        {
            if (bookings.Exists(b => b.VehicleId == request.VehicleId
                && request.StartAt < b.EndAt && request.EndAt > b.StartAt))
                return null;

            var booking = new Booking(++lastBookingId, request.VehicleId,
                request.StartAt, request.EndAt, "Confirmed");
            bookings.Add(booking);
            return booking;
        }
    }
}
