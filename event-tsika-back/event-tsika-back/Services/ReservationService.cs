using event_tsika_back.Data;
using event_tsika_back.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace event_tsika_back.Services;

public class PlatformOptions
{
    public const string Section = "Platform";
    /// <summary>Commission prélevée sur les événements payants (concept : ~5 %).</summary>
    public decimal CommissionRate { get; set; } = 0.05m;
}

public enum ReserveError { None, EventNotFound, EventStarted, NotEnoughSeats, PaymentFailed, NotFound, AlreadyCancelled, Forbidden }

public record ReserveOutcome(ReserveError Error, Reservation? Reservation = null);

public class ReservationService(AppDbContext db, IPaymentGateway payments, IOptions<PlatformOptions> options, TimeProvider clock)
{
    // ponytail: verrou process-local, suffisant pour une instance SQLite ; passer à une transaction sérialisable si PostgreSQL / multi-instances.
    private static readonly SemaphoreSlim Gate = new(1, 1);

    public static int CommissionFor(int totalMga, decimal rate) => (int)Math.Round(totalMga * rate, MidpointRounding.AwayFromZero);

    public async Task<ReserveOutcome> ReserveAsync(Guid userId, Guid eventId, int quantity)
    {
        await Gate.WaitAsync();
        try
        {
            var ev = await db.Events.FindAsync(eventId);
            if (ev is null) return new(ReserveError.EventNotFound);
            var now = clock.GetUtcNow().UtcDateTime;
            if (ev.StartsAtUtc <= now) return new(ReserveError.EventStarted);

            var taken = await db.Reservations
                .Where(r => r.EventId == eventId && r.Status == ReservationStatus.Confirmed)
                .SumAsync(r => (int?)r.Quantity) ?? 0;
            if (taken + quantity > ev.Capacity) return new(ReserveError.NotEnoughSeats);

            var total = ev.PriceMga * quantity;
            if (total > 0)
            {
                var pay = await payments.ChargeAsync(userId, total);
                if (!pay.Success) return new(ReserveError.PaymentFailed);
            }

            var reservation = new Reservation
            {
                EventId = eventId,
                UserId = userId,
                Quantity = quantity,
                TotalMga = total,
                CommissionMga = CommissionFor(total, options.Value.CommissionRate),
                Status = ReservationStatus.Confirmed,
                TicketCode = Guid.NewGuid().ToString("N")[..12].ToUpperInvariant(),
                CreatedAtUtc = now,
            };
            db.Reservations.Add(reservation);
            await db.SaveChangesAsync();
            return new(ReserveError.None, reservation);
        }
        finally { Gate.Release(); }
    }

    public async Task<ReserveOutcome> CancelAsync(Guid userId, Guid reservationId)
    {
        var r = await db.Reservations.Include(x => x.Event).FirstOrDefaultAsync(x => x.Id == reservationId);
        if (r is null) return new(ReserveError.NotFound);
        if (r.UserId != userId) return new(ReserveError.Forbidden);
        if (r.Status == ReservationStatus.Cancelled) return new(ReserveError.AlreadyCancelled);
        if (r.Event!.StartsAtUtc <= clock.GetUtcNow().UtcDateTime) return new(ReserveError.EventStarted);

        if (r.TotalMga > 0) await payments.RefundAsync(r.TicketCode);
        r.Status = ReservationStatus.Cancelled;
        await db.SaveChangesAsync();
        return new(ReserveError.None, r);
    }
}
