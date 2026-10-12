using Microsoft.AspNetCore.Hosting;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using event_tsika_back.Dtos;
using Microsoft.AspNetCore.Mvc.Testing;

namespace event_tsika_back.Tests;

/// <summary>API réelle sur une base SQLite temporaire (les migrations sont donc exercées).</summary>
public sealed class ApiFactory : WebApplicationFactory<Program>
{
    private readonly string _dbPath = Path.Combine(Path.GetTempPath(), $"event-tsika-test-{Guid.NewGuid():N}.db");

    public static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web) { Converters = { new JsonStringEnumConverter() } };

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseSetting("Jwt:Key", "tests-only-signing-key-0123456789-abcdef");
        builder.UseSetting("ConnectionStrings:Default", $"Data Source={_dbPath}");
    }

    public async Task<HttpClient> ClientFor(string name, bool organizer)
    {
        var client = CreateClient();
        var res = await client.PostAsJsonAsync("/api/auth/register",
            new RegisterRequest($"{name}-{Guid.NewGuid():N}@test.mg", "motdepasse-ok", name, organizer));
        res.EnsureSuccessStatusCode();
        var auth = (await res.Content.ReadFromJsonAsync<AuthResponse>(Json))!;
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth.Token);
        return client;
    }

    public static EventRequest NewEvent(int capacity = 10, int priceMga = 10_000, string title = "Festival") =>
        new(title, "Description", event_tsika_back.Domain.EventCategory.Culture, "Antananarivo", "Place", DateTime.UtcNow.AddDays(10), capacity, priceMga);

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        Microsoft.Data.Sqlite.SqliteConnection.ClearAllPools();
        try { File.Delete(_dbPath); } catch (IOException) { }
    }
}
