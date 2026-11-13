namespace CorporateCarsharing.Client.Models;
public sealed record VehicleDto(int Id, string PlateNumber, string Model, string Status);
public sealed record BookingDto(int Id, int VehicleId, DateTime StartAt, DateTime EndAt, string Status);
public sealed record CreateBookingRequest(int VehicleId, DateTime StartAt, DateTime EndAt);
