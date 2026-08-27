using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SewaRent_Api.Features.Dashboard;

namespace SewaRent_Api.Controllers.Dashboard;

[ApiController]
[Route("api/dashboard")]
[Authorize]
public class DashboardController(ISender sender) : ControllerBase
{
    [HttpGet("landlord")]
    public async Task<IActionResult> GetLandlord(CancellationToken ct)
        => Ok(await sender.Send(new GetLandlordDashboard.Query(), ct));

    [HttpGet("tenant")]
    public async Task<IActionResult> GetTenant(CancellationToken ct)
        => Ok(await sender.Send(new GetTenantDashboard.Query(), ct));
}
