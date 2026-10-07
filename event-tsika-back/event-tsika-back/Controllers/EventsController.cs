using event_tsika_back.Data;
using event_tsika_back.Domain;
using event_tsika_back.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace event_tsika_back.Controllers;

[ApiController]
[Route("api/events")]
public class EventsController(AppDbContext db, TimeProvider clock) : ApiControllerBase
{
    private const int MaxPageSize = 50;

    [HttpGet]
    public async Task<IEnumerable<EventDto>> List(
        string? q, EventCategory? category, string? city, int skip = 0, int take = 20)
    {
        var now = clock.GetUtcNow().UtcDateTime;
        var query = db.Events.AsNoTracking().Where(e => e.StartsAtUtc > now);
        if (!string.IsNullOrWhiteSpace(q))
            query = query.Where(e => EF.Functions.Like(e.Title, $"%{q}%") || EF.Functions.Like(e.Description, $"%{q}%"));
        if (category is not null) query = query.Where(e => e.Category == category);
        if (!string.IsNullOrWhiteSpace(city)) query = query.Where(e => e.City == city);

        return await Project(query.OrderBy(e => e.StartsAtUtc).Skip(Math.Max(skip, 0)).Take(Math.Clamp(take, 1, MaxPageSize))).ToListAsync();
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<EventDto>> Get(Guid id)
    {
        var dto = await Project(db.Events.AsNoTracking().Where(e => e.Id == id)).FirstOrDefaultAsync();
        return dto is null ? NotFound() : dto;
    }

    [HttpPost, Authorize(Roles = nameof(Role.Organizer))]
    public async Task<ActionResult<EventDto>> Create(EventRequest req)
    {
        if (req.StartsAtUtc.ToUniversalTime() <= clock.GetUtcNow().UtcDateTime)
            return Problem(statusCode: StatusCodes.Status400BadRequest, title: "La date de l'événement doit être dans le futur.");

        var ev = new Event { OrganizerId = CurrentUserId, Title = req.Title, Description = req.Description, City = req.City, Venue = req.Venue };
        Apply(ev, req);
        db.Events.Add(ev);
        await db.SaveChangesAsync();
        return CreatedAtAction(nameof(Get), new { id = ev.Id }, await Project(db.Events.Where(e => e.Id == ev.Id)).FirstAsync());
    }

    [HttpPut("{id:guid}"), Authorize(Roles = nameof(Role.Organizer))]
    public async Task<ActionResult<EventDto>> Update(Guid id, EventRequest req)
    {
        var ev = await db.Events.FindAsync(id);
        if (ev is null) return NotFound();
        if (ev.OrganizerId != CurrentUserId) return Forbid();

        var sold = await SeatsSold(id);
        if (req.Capacity < sold)
            return Problem(statusCode: StatusCodes.Status409Conflict, title: $"{sold} places sont déjà réservées : capacité minimale {sold}.");

        Apply(ev, req);
        await db.SaveChangesAsync();
        return await Project(db.Events.AsNoTracking().Where(e => e.Id == id)).FirstAsync();
    }

    [HttpDelete("{id:guid}"), Authorize(Roles = nameof(Role.Organizer))]
    public async Task<IActionResult> Delete(Guid id)
    {
        var ev = await db.Events.FindAsync(id);
        if (ev is null) return NotFound();
        if (ev.OrganizerId != CurrentUserId) return Forbid();
        if (await db.Reservations.AnyAsync(r => r.EventId == id && r.Status == ReservationStatus.Confirmed))
            return Problem(statusCode: StatusCodes.Status409Conflict, title: "Des réservations existent : impossible de supprimer l'événement.");

        db.Reservations.RemoveRange(db.Reservations.Where(r => r.EventId == id));
        db.Events.Remove(ev);
        await db.SaveChangesAsync();
        return NoContent();
    }

    private Task<int> SeatsSold(Guid eventId) =>
        db.Reservations.Where(r => r.EventId == eventId && r.Status == ReservationStatus.Confirmed).SumAsync(r => r.Quantity);

    private static void Apply(Event ev, EventRequest req)
    {
        ev.Title = req.Title.Trim();
        ev.Description = req.Description.Trim();
        ev.Category = req.Category;
        ev.City = req.City.Trim();
        ev.Venue = req.Venue.Trim();
        ev.StartsAtUtc = req.StartsAtUtc.ToUniversalTime();
        ev.Capacity = req.Capacity;
        ev.PriceMga = req.PriceMga;
    }

    private IQueryable<EventDto> Project(IQueryable<Event> events) =>
        events.Select(e => new EventDto(
            e.Id, e.Title, e.Description, e.Category, e.City, e.Venue, e.StartsAtUtc, e.Capacity, e.PriceMga,
            e.Capacity - db.Reservations.Where(r => r.EventId == e.Id && r.Status == ReservationStatus.Confirmed).Sum(r => (int?)r.Quantity).GetValueOrDefault(),
            e.Organizer!.DisplayName));
}
