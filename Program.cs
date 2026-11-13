using CorporateCarsharing.Repositories;
using CorporateCarsharing.Services;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Хранилище единое для запросов приложения, сервис содержит бизнес-логику.
builder.Services.AddSingleton<CarsharingRepository>();
builder.Services.AddSingleton<BookingService>();
builder.Services.AddSingleton<TelemetryMappingService>();

var app = builder.Build();
app.UseSwagger();
app.UseSwaggerUI();

app.MapGet("/", () => Results.Ok(new { service = "Corporate Carsharing API", version = "1.0" }));
app.MapGet("/api/v1/vehicles", (CarsharingRepository repository) => Results.Ok(repository.GetVehicles()));
app.MapGet("/api/v1/bookings", (CarsharingRepository repository) => Results.Ok(repository.GetBookings()))
    .Produces<IReadOnlyList<Booking>>(StatusCodes.Status200OK);

// Адрес Location из 201 Created теперь реально доступен.
app.MapGet("/api/v1/bookings/{id:int}", (int id, CarsharingRepository repository) =>
{
    var booking = repository.FindBooking(id);
    return booking is null ? Results.NotFound(new { error = "Бронирование не найдено." }) : Results.Ok(booking);
})
    .Produces<Booking>(StatusCodes.Status200OK)
    .Produces(StatusCodes.Status404NotFound);
app.MapPost("/api/v1/bookings", (CreateBookingRequest request, BookingService service) =>
{
    var result = service.Create(request);
    return result.Kind switch
    {
        BookingResultKind.Created => Results.Created($"/api/v1/bookings/{result.Booking!.Id}", result.Booking),
        BookingResultKind.BadRequest => Results.BadRequest(new { error = result.Error }),
        BookingResultKind.NotFound => Results.NotFound(new { error = result.Error }),
        BookingResultKind.Conflict => Results.Conflict(new { error = result.Error }),
        _ => Results.StatusCode(500)
    };
})
    .Accepts<CreateBookingRequest>("application/json")
    .Produces<Booking>(StatusCodes.Status201Created)
    .Produces(StatusCodes.Status400BadRequest)
    .Produces(StatusCodes.Status404NotFound)
    .Produces(StatusCodes.Status409Conflict);
// Интеграционный контракт: приём телеметрии от внешней системы.
app.MapPost("/api/v1/telemetry", (TelemetryInput input, TelemetryMappingService mapper) =>
{
    var result = mapper.Ingest(input);
    return result.Kind switch
    {
        TelemetryResultKind.Created => Results.Created($"/api/v1/telemetry/{result.Telemetry!.MessageId}", result.Telemetry),
        TelemetryResultKind.Duplicate => Results.Ok(new { status = "DuplicateIgnored", messageId = result.Telemetry!.MessageId }),
        TelemetryResultKind.BadRequest => Results.BadRequest(new { error = result.Error }),
        TelemetryResultKind.NotFound => Results.NotFound(new { error = result.Error }),
        _ => Results.StatusCode(500)
    };
});
app.MapGet("/api/v1/telemetry", (TelemetryMappingService mapper) => Results.Ok(mapper.GetAll()));

app.MapGet("/health", () => Results.Ok(new { status = "Healthy" }));
app.MapGet("/api/v1/version", () =>
    Results.Ok(new
    {
        service = "CorporateCarsharing",
        apiVersion = "1.0",
        validationEnabled = true
    }))
    .WithName("GetApiVersion")
    .WithTags("System")
    .Produces(200);
app.Run();

// Требуется для WebApplicationFactory<Program> в интеграционных тестах.
public partial class Program { }
