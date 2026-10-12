using System.Net;
using System.Net.Http.Json;
using event_tsika_back.Domain;
using event_tsika_back.Dtos;
using event_tsika_back.Services;

namespace event_tsika_back.Tests;

public class ApiTests : IClassFixture<ApiFactory>
{
    private readonly ApiFactory _f;
    public ApiTests(ApiFactory f) => _f = f;

    private static async Task<T> Read<T>(HttpResponseMessage r) => (await r.Content.ReadFromJsonAsync<T>(ApiFactory.Json))!;

    private async Task<EventDto> CreateEvent(HttpClient organizer, EventRequest req)
    {
        var res = await organizer.PostAsJsonAsync("/api/events", req, ApiFactory.Json);
        Assert.Equal(HttpStatusCode.Created, res.StatusCode);
        return await Read<EventDto>(res);
    }

    [Fact]
    public async Task Register_with_existing_email_returns_conflict()
    {
        var client = _f.CreateClient();
        var req = new RegisterRequest("dup@test.mg", "motdepasse-ok", "Dup", false);
        await client.PostAsJsonAsync("/api/auth/register", req);
        var second = await client.PostAsJsonAsync("/api/auth/register", req);
        Assert.Equal(HttpStatusCode.Conflict, second.StatusCode);
    }

    [Fact]
    public async Task Login_with_wrong_password_returns_unauthorized()
    {
        var client = _f.CreateClient();
        await client.PostAsJsonAsync("/api/auth/register", new RegisterRequest("login@test.mg", "motdepasse-ok", "L", false));
        var res = await client.PostAsJsonAsync("/api/auth/login", new LoginRequest("login@test.mg", "mauvais"));
        Assert.Equal(HttpStatusCode.Unauthorized, res.StatusCode);
    }

    [Fact]
    public async Task Register_with_short_password_is_rejected()
    {
        var res = await _f.CreateClient().PostAsJsonAsync("/api/auth/register", new RegisterRequest("s@test.mg", "court", "S", false));
        Assert.Equal(HttpStatusCode.BadRequest, res.StatusCode);
    }

    [Fact]
    public async Task Participant_cannot_create_event()
    {
        var participant = await _f.ClientFor("part", organizer: false);
        var res = await participant.PostAsJsonAsync("/api/events", ApiFactory.NewEvent(), ApiFactory.Json);
        Assert.Equal(HttpStatusCode.Forbidden, res.StatusCode);
    }

    [Fact]
    public async Task Event_in_the_past_is_rejected()
    {
        var org = await _f.ClientFor("org", organizer: true);
        var past = ApiFactory.NewEvent() with { StartsAtUtc = DateTime.UtcNow.AddDays(-1) };
        var res = await org.PostAsJsonAsync("/api/events", past, ApiFactory.Json);
        Assert.Equal(HttpStatusCode.BadRequest, res.StatusCode);
    }

    [Fact]
    public async Task Catalogue_filters_by_category_and_text()
    {
        var org = await _f.ClientFor("org", organizer: true);
        var title = $"Concert-{Guid.NewGuid():N}";
        await CreateEvent(org, ApiFactory.NewEvent(title: title) with { Category = EventCategory.Entertainment });

        var found = await Read<List<EventDto>>(await _f.CreateClient().GetAsync($"/api/events?q={title}&category=Entertainment"));
        var other = await Read<List<EventDto>>(await _f.CreateClient().GetAsync($"/api/events?q={title}&category=Sport"));

        Assert.Single(found);
        Assert.Empty(other);
    }

    [Fact]
    public async Task Reservation_computes_total_commission_and_seats_left()
    {
        var org = await _f.ClientFor("org", organizer: true);
        var ev = await CreateEvent(org, ApiFactory.NewEvent(capacity: 10, priceMga: 10_000));
        var buyer = await _f.ClientFor("buyer", organizer: false);

        var res = await buyer.PostAsJsonAsync($"/api/events/{ev.Id}/reservations", new ReserveRequest(3), ApiFactory.Json);
        var reservation = await Read<ReservationDto>(res);
        var after = await Read<EventDto>(await _f.CreateClient().GetAsync($"/api/events/{ev.Id}"));
        var stats = await Read<EventStatsDto>(await org.GetAsync($"/api/organizer/events/{ev.Id}/stats"));

        Assert.Equal(HttpStatusCode.Created, res.StatusCode);
        Assert.Equal(30_000, reservation.TotalMga);
        Assert.Equal(7, after.SeatsLeft);
        Assert.Equal(3, stats.SeatsSold);
        Assert.Equal(1_500, stats.CommissionMga);
        Assert.Equal(28_500, stats.NetMga);
    }

    [Fact]
    public async Task Free_event_has_no_commission()
    {
        var org = await _f.ClientFor("org", organizer: true);
        var ev = await CreateEvent(org, ApiFactory.NewEvent(priceMga: 0));
        var buyer = await _f.ClientFor("buyer", organizer: false);
        await buyer.PostAsJsonAsync($"/api/events/{ev.Id}/reservations", new ReserveRequest(2), ApiFactory.Json);

        var stats = await Read<EventStatsDto>(await org.GetAsync($"/api/organizer/events/{ev.Id}/stats"));
        Assert.Equal(0, stats.CommissionMga);
        Assert.Equal(2, stats.SeatsSold);
    }

    [Fact]
    public async Task Reservation_beyond_capacity_returns_conflict()
    {
        var org = await _f.ClientFor("org", organizer: true);
        var ev = await CreateEvent(org, ApiFactory.NewEvent(capacity: 2));
        var buyer = await _f.ClientFor("buyer", organizer: false);

        var res = await buyer.PostAsJsonAsync($"/api/events/{ev.Id}/reservations", new ReserveRequest(3), ApiFactory.Json);
        Assert.Equal(HttpStatusCode.Conflict, res.StatusCode);
    }

    [Fact]
    public async Task Reservation_requires_authentication()
    {
        var org = await _f.ClientFor("org", organizer: true);
        var ev = await CreateEvent(org, ApiFactory.NewEvent());
        var res = await _f.CreateClient().PostAsJsonAsync($"/api/events/{ev.Id}/reservations", new ReserveRequest(1));
        Assert.Equal(HttpStatusCode.Unauthorized, res.StatusCode);
    }

    [Fact]
    public async Task Cancel_frees_seats_and_cannot_be_repeated_or_done_by_someone_else()
    {
        var org = await _f.ClientFor("org", organizer: true);
        var ev = await CreateEvent(org, ApiFactory.NewEvent(capacity: 5));
        var buyer = await _f.ClientFor("buyer", organizer: false);
        var stranger = await _f.ClientFor("stranger", organizer: false);
        var reservation = await Read<ReservationDto>(
            await buyer.PostAsJsonAsync($"/api/events/{ev.Id}/reservations", new ReserveRequest(4), ApiFactory.Json));

        var byStranger = await stranger.PostAsync($"/api/reservations/{reservation.Id}/cancel", null);
        var first = await buyer.PostAsync($"/api/reservations/{reservation.Id}/cancel", null);
        var second = await buyer.PostAsync($"/api/reservations/{reservation.Id}/cancel", null);
        var after = await Read<EventDto>(await _f.CreateClient().GetAsync($"/api/events/{ev.Id}"));

        Assert.Equal(HttpStatusCode.Forbidden, byStranger.StatusCode);
        Assert.Equal(HttpStatusCode.OK, first.StatusCode);
        Assert.Equal(HttpStatusCode.Conflict, second.StatusCode);
        Assert.Equal(5, after.SeatsLeft);
    }

    [Fact]
    public async Task Event_with_reservations_cannot_be_deleted_or_shrunk_below_sold_seats()
    {
        var org = await _f.ClientFor("org", organizer: true);
        var ev = await CreateEvent(org, ApiFactory.NewEvent(capacity: 10));
        var buyer = await _f.ClientFor("buyer", organizer: false);
        await buyer.PostAsJsonAsync($"/api/events/{ev.Id}/reservations", new ReserveRequest(5), ApiFactory.Json);

        var delete = await org.DeleteAsync($"/api/events/{ev.Id}");
        var shrink = await org.PutAsJsonAsync($"/api/events/{ev.Id}", ApiFactory.NewEvent(capacity: 4), ApiFactory.Json);

        Assert.Equal(HttpStatusCode.Conflict, delete.StatusCode);
        Assert.Equal(HttpStatusCode.Conflict, shrink.StatusCode);
    }

    [Fact]
    public async Task Organizer_cannot_read_stats_of_another_organizer()
    {
        var owner = await _f.ClientFor("owner", organizer: true);
        var other = await _f.ClientFor("other", organizer: true);
        var ev = await CreateEvent(owner, ApiFactory.NewEvent());

        Assert.Equal(HttpStatusCode.Forbidden, (await other.GetAsync($"/api/organizer/events/{ev.Id}/stats")).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await other.DeleteAsync($"/api/events/{ev.Id}")).StatusCode);
    }

    [Fact]
    public async Task My_reservations_lists_only_own_tickets()
    {
        var org = await _f.ClientFor("org", organizer: true);
        var ev = await CreateEvent(org, ApiFactory.NewEvent());
        var a = await _f.ClientFor("a", organizer: false);
        var b = await _f.ClientFor("b", organizer: false);
        await a.PostAsJsonAsync($"/api/events/{ev.Id}/reservations", new ReserveRequest(1), ApiFactory.Json);

        Assert.Single(await Read<List<ReservationDto>>(await a.GetAsync("/api/reservations/mine")));
        Assert.Empty(await Read<List<ReservationDto>>(await b.GetAsync("/api/reservations/mine")));
    }
}

public class UnitTests
{
    [Fact]
    public void Password_hash_verifies_only_the_right_password()
    {
        var hash = PasswordHasher.Hash("secret-123");
        Assert.True(PasswordHasher.Verify("secret-123", hash));
        Assert.False(PasswordHasher.Verify("secret-124", hash));
    }

    [Theory]
    [InlineData(10_000, 500)]
    [InlineData(0, 0)]
    [InlineData(10, 1)] // 0,5 arrondi au supérieur
    public void Commission_is_five_percent_rounded_away_from_zero(int total, int expected) =>
        Assert.Equal(expected, ReservationService.CommissionFor(total, 0.05m));
}
