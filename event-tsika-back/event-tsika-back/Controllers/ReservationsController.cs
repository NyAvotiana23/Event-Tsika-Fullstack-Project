using event_tsika_back.Data;
using event_tsika_back.Dtos;
using event_tsika_back.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace event_tsika_back.Controllers;

[ApiController]
[Authorize]
public class ReservationsController(AppDbContext db, ReservationService service) : ApiControllerBase
{
    [HttpPost("api/events/{eventId:guid}/reservations")]
    public async Task<ActionResult<ReservationDto>> Reserve(Guid eventId, ReserveRequest req)
    {
        var outcome = await service.ReserveAsync(CurrentUserId, eventId, req.Quantity);
        if (outcome.Error != ReserveError.None) return Fail(outcome.Error);
        return CreatedAtAction(nameof(Mine), await ToDto(outcome.Reservation!.Id));
    }

    [HttpGet("api/reservations/mine")]
    public async Task<IEnumerable<ReservationDto>> Mine()
    {
        var userId = CurrentUserId;
        return await db.Reservations.AsNoTracking()
            .Where(r => r.UserId == userId)
            .OrderByDescending(r => r.CreatedAtUtc)
            .Select(r => new ReservationDto(r.Id, r.EventId, r.Event!.Title, r.Event.StartsAtUtc, r.Quantity,
                r.TotalMga, r.Status, r.TicketCode, r.CreatedAtUtc))
            .ToListAsync();
    }

    [HttpPost("api/reservations/{id:guid}/cancel")]
    public async Task<ActionResult<ReservationDto>> Cancel(Guid id)
    {
        var outcome = await service.CancelAsync(CurrentUserId, id);
        return outcome.Error == ReserveError.None ? await ToDto(id) : Fail(outcome.Error);
    }

    private Task<ReservationDto> ToDto(Guid id) =>
        db.Reservations.AsNoTracking().Where(r => r.Id == id)
            .Select(r => new ReservationDto(r.Id, r.EventId, r.Event!.Title, r.Event.StartsAtUtc, r.Quantity,
                r.TotalMga, r.Status, r.TicketCode, r.CreatedAtUtc))
            .FirstAsync();

    private ActionResult Fail(ReserveError error) => error switch
    {
        ReserveError.EventNotFound or ReserveError.NotFound => NotFound(),
        ReserveError.Forbidden => Forbid(),
        ReserveError.NotEnoughSeats => Problem(statusCode: 409, title: "Plus assez de places disponibles."),
        ReserveError.EventStarted => Problem(statusCode: 409, title: "L'événement a déjà commencé."),
        ReserveError.AlreadyCancelled => Problem(statusCode: 409, title: "Réservation déjà annulée."),
        ReserveError.PaymentFailed => Problem(statusCode: 402, title: "Le paiement a échoué."),
        _ => Problem(statusCode: 500),
    };
}
