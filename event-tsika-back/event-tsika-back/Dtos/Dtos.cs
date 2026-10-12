using System.ComponentModel.DataAnnotations;
using event_tsika_back.Domain;

namespace event_tsika_back.Dtos;

public record RegisterRequest(
    [Required, EmailAddress, MaxLength(200)] string Email,
    [Required, MinLength(8), MaxLength(100)] string Password,
    [Required, MaxLength(100)] string DisplayName,
    bool IsOrganizer);

public record LoginRequest([Required] string Email, [Required] string Password);

public record AuthResponse(string Token, Guid UserId, string DisplayName, Role Role);

public record EventRequest(
    [Required, MaxLength(150)] string Title,
    [Required, MaxLength(4000)] string Description,
    EventCategory Category,
    [Required, MaxLength(100)] string City,
    [Required, MaxLength(150)] string Venue,
    DateTime StartsAtUtc,
    [Range(1, 100_000)] int Capacity,
    [Range(0, 100_000_000)] int PriceMga);

public record EventDto(
    Guid Id, string Title, string Description, EventCategory Category, string City, string Venue,
    DateTime StartsAtUtc, int Capacity, int PriceMga, int SeatsLeft, string OrganizerName);

public record ReserveRequest([Range(1, 10)] int Quantity);

public record ReservationDto(
    Guid Id, Guid EventId, string EventTitle, DateTime EventStartsAtUtc, int Quantity,
    int TotalMga, ReservationStatus Status, string TicketCode, DateTime CreatedAtUtc);

public record ParticipantDto(string DisplayName, int Quantity, string TicketCode, ReservationStatus Status, DateTime CreatedAtUtc);

public record EventStatsDto(Guid EventId, string Title, int Capacity, int SeatsSold, int SeatsLeft,
    double FillRate, int RevenueMga, int CommissionMga, int NetMga);
