using event_tsika_back.Data;
using event_tsika_back.Domain;
using event_tsika_back.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace event_tsika_back.Controllers;

[ApiController]
[Authorize(Roles = nameof(Role.Organizer))]
[Route("api/organizer/events")]
public class OrganizerController(AppDbContext db) : ApiControllerBase
{
    /// <summary>Événements de l'organisateur connecté, avec leurs statistiques de réservation.</summary>
    [HttpGet]
    public async Task<IEnumerable<EventStatsDto>> MyEvents()
    {
        var userId = CurrentUserId;
        var events = await db.Events.AsNoTracking().Where(e => e.OrganizerId == userId).OrderBy(e => e.StartsAtUtc).ToListAsync();
        var ids = events.Select(e => e.Id).ToList();
        var confirmed = await db.Reservations.AsNoTracking()
            .Where(r => ids.Contains(r.EventId) && r.Status == ReservationStatus.Confirmed).ToListAsync();
        return events.Select(e => BuildStats(e, confirmed.Where(r => r.EventId == e.Id)));
    }

    [HttpGet("{id:guid}/stats")]
    public async Task<ActionResult<EventStatsDto>> Stats(Guid id)
    {
        var ev = await db.Events.AsNoTracking().FirstOrDefaultAsync(e => e.Id == id);
        if (ev is null) return NotFound();
        if (ev.OrganizerId != CurrentUserId) return Forbid();
        var confirmed = await db.Reservations.AsNoTracking()
            .Where(r => r.EventId == id && r.Status == ReservationStatus.Confirmed).ToListAsync();
        return BuildStats(ev, confirmed);
    }

    [HttpGet("{id:guid}/participants")]
    public async Task<ActionResult<IEnumerable<ParticipantDto>>> Participants(Guid id)
    {
        var ev = await db.Events.AsNoTracking().FirstOrDefaultAsync(e => e.Id == id);
        if (ev is null) return NotFound();
        if (ev.OrganizerId != CurrentUserId) return Forbid();
        return await db.Reservations.AsNoTracking()
            .Where(r => r.EventId == id)
            .OrderBy(r => r.CreatedAtUtc)
            .Select(r => new ParticipantDto(r.User!.DisplayName, r.Quantity, r.TicketCode, r.Status, r.CreatedAtUtc))
            .ToListAsync();
    }

    private static EventStatsDto BuildStats(Event e, IEnumerable<Reservation> confirmed)
    {
        var list = confirmed.ToList();
        var sold = list.Sum(r => r.Quantity);
        var revenue = list.Sum(r => r.TotalMga);
        var commission = list.Sum(r => r.CommissionMga);
        return new EventStatsDto(e.Id, e.Title, e.Capacity, sold, e.Capacity - sold,
            Math.Round((double)sold / e.Capacity, 4), revenue, commission, revenue - commission);
    }
}
