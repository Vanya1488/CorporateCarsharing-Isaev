using CorporateCarsharing.Core.Models;
namespace CorporateCarsharing.Core.Interfaces;
public interface ICarsharingRepository
{
    IReadOnlyList<Vehicle> GetVehicles();
    IReadOnlyList<Booking> GetBookings();
    Vehicle? FindVehicle(int vehicleId);
    Booking? FindBooking(int bookingId);
    Booking? AddIfNoOverlap(CreateBookingRequest request);
}
