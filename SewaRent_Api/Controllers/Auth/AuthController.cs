using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SewaRent_Api.Features.Auth;

namespace SewaRent_Api.Controllers.Auth;

[ApiController]
[Route("api/auth")]
public class AuthController(ISender sender) : ControllerBase
{
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] Register.Command command, CancellationToken ct)
        => Ok(await sender.Send(command, ct));

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] Login.Command command, CancellationToken ct)
        => Ok(await sender.Send(command, ct));

    [Authorize]
    [HttpPost("change-password")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePassword.Command command, CancellationToken ct)
    {
        await sender.Send(command, ct);
        return Ok(new { message = "Password changed successfully." });
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<IActionResult> Me(CancellationToken ct)
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        var email = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
        var role = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;

        return Ok(new { id = userId, email, role });
    }

    [Authorize]
    [HttpPost("link-landlord")]
    public async Task<IActionResult> LinkLandlord([FromBody] LinkLandlord.Command command, CancellationToken ct)
    {
        var result = await sender.Send(command, ct);
        return Ok(new { message = "Successfully linked to landlord.", data = result });
    }
}
