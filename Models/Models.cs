// Имена и JSON-поля сохранены для совместимости с существующими API и тестами.
public record Vehicle(int Id, string PlateNumber, string Model, string Status);
public record Booking(int Id, int VehicleId, DateTime StartAt, DateTime EndAt, string Status);
public record CreateBookingRequest(int VehicleId, DateTime StartAt, DateTime EndAt);
