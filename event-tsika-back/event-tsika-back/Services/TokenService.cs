using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using event_tsika_back.Domain;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace event_tsika_back.Services;

public class JwtOptions
{
    public const string Section = "Jwt";
    public const int MinKeyLength = 32;
    public string Key { get; set; } = "";
    public string Issuer { get; set; } = "event-tsika";
    public string Audience { get; set; } = "event-tsika";
    public int LifetimeMinutes { get; set; } = 120;
}

public class TokenService(IOptions<JwtOptions> options, TimeProvider clock)
{
    private readonly JwtOptions _o = options.Value;

    public string Create(User user)
    {
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(ClaimTypes.Role, user.Role.ToString()),
        };
        var creds = new SigningCredentials(SigningKey(_o.Key), SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(_o.Issuer, _o.Audience, claims,
            expires: clock.GetUtcNow().UtcDateTime.AddMinutes(_o.LifetimeMinutes), signingCredentials: creds);
        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public static SymmetricSecurityKey SigningKey(string key) => new(Encoding.UTF8.GetBytes(key));
}
