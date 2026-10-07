namespace event_tsika_back.Domain;

public enum Role { Participant, Organizer }

public enum EventCategory { Culture, Sport, Education, Entertainment, Professional, Charity }

public enum ReservationStatus { Confirmed, Cancelled }

public class User
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public required string Email { get; set; }
    public required string DisplayName { get; set; }
    public required string PasswordHash { get; set; }
    public Role Role { get; set; }
}

public class Event
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid OrganizerId { get; set; }
    public User? Organizer { get; set; }
    public required string Title { get; set; }
    public required string Description { get; set; }
    public EventCategory Category { get; set; }
    public required string City { get; set; }
    public required string Venue { get; set; }
    public DateTime StartsAtUtc { get; set; }
    public int Capacity { get; set; }
    /// <summary>Prix par place en ariary (MGA). 0 = événement gratuit.</summary>
    public int PriceMga { get; set; }
}

public class Reservation
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid EventId { get; set; }
    public Event? Event { get; set; }
    public Guid UserId { get; set; }
    public User? User { get; set; }
    public int Quantity { get; set; }
    public int TotalMga { get; set; }
    /// <summary>Commission de la plateforme figée à la réservation (le taux peut changer ensuite).</summary>
    public int CommissionMga { get; set; }
    public ReservationStatus Status { get; set; }
    public required string TicketCode { get; set; }
    public DateTime CreatedAtUtc { get; set; }
}
