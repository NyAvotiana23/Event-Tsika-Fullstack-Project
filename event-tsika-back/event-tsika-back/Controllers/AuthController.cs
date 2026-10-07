using event_tsika_back.Data;
using event_tsika_back.Domain;
using event_tsika_back.Dtos;
using event_tsika_back.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace event_tsika_back.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(AppDbContext db, TokenService tokens) : ControllerBase
{
    [HttpPost("register")]
    public async Task<ActionResult<AuthResponse>> Register(RegisterRequest req)
    {
        var email = req.Email.Trim().ToLowerInvariant();
        if (await db.Users.AnyAsync(u => u.Email == email))
            return Problem(statusCode: StatusCodes.Status409Conflict, title: "Cet email est déjà utilisé.");

        var user = new User
        {
            Email = email,
            DisplayName = req.DisplayName.Trim(),
            PasswordHash = PasswordHasher.Hash(req.Password),
            Role = req.IsOrganizer ? Role.Organizer : Role.Participant,
        };
        db.Users.Add(user);
        await db.SaveChangesAsync();
        return Ok(new AuthResponse(tokens.Create(user), user.Id, user.DisplayName, user.Role));
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login(LoginRequest req)
    {
        var email = req.Email.Trim().ToLowerInvariant();
        var user = await db.Users.FirstOrDefaultAsync(u => u.Email == email);
        if (user is null || !PasswordHasher.Verify(req.Password, user.PasswordHash))
            return Problem(statusCode: StatusCodes.Status401Unauthorized, title: "Email ou mot de passe incorrect.");
        return Ok(new AuthResponse(tokens.Create(user), user.Id, user.DisplayName, user.Role));
    }
}
