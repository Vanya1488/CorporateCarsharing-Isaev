using CorporateCarsharing.Core.Models;
using CorporateCarsharing.Core.Services;
using CorporateCarsharing.Infrastructure.Repositories;
using Xunit;
namespace CorporateCarsharing.Tests;
public class BookingServiceTests
{
    [Fact]
    public void CreatesBookingAndRejectsOverlap()
    {
        var service = new BookingService(new CarsharingRepository());
        var start = new DateTime(2026, 11, 20, 10, 0, 0, DateTimeKind.Utc);
        Assert.Equal(BookingResultKind.Created, service.Create(new(1,start,start.AddHours(2))).Kind);
        Assert.Equal(BookingResultKind.Conflict, service.Create(new(1,start.AddHours(1),start.AddHours(3))).Kind);
    }
    [Fact]
    public void RejectsInvalidInterval()
    {
        var service = new BookingService(new CarsharingRepository());
        var start = new DateTime(2026, 11, 20, 10, 0, 0, DateTimeKind.Utc);
        Assert.Equal(BookingResultKind.BadRequest, service.Create(new(1,start,start)).Kind);
    }
}
