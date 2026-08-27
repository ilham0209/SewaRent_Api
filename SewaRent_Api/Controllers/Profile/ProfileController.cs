using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SewaRent_Api.Features.Profile;

namespace SewaRent_Api.Controllers.Profile;

[ApiController]
[Route("api/users")]
[Authorize]
public class ProfileController(ISender sender) : ControllerBase
{
    [HttpGet("me")]
    public async Task<IActionResult> GetProfile(CancellationToken ct)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await sender.Send(new GetProfile.Query(userId), ct);
        return result is not null ? Ok(result) : NotFound();
    }

    [HttpPut("me")]
    public async Task<IActionResult> UpdateProfile(UpdateProfile.Command command, CancellationToken ct)
        => Ok(await sender.Send(command, ct));

    [HttpPost("me/profile-image")]
    public async Task<IActionResult> UpdateProfileImage(
        UpdateProfileImage.Command command, CancellationToken ct)
        => Ok(await sender.Send(command, ct));

    [HttpPut("me/bank-details")]
    public async Task<IActionResult> UpdateBankDetails(
        UpdateBankDetails.Command command, CancellationToken ct)
        => Ok(await sender.Send(command, ct));
}
