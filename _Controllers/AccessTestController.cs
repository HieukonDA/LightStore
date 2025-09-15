using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TheLightStore.Services.Auth;

namespace TheLightStore.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize] // bắt buộc login
public class AccessTestController : ControllerBase
{
    private readonly IRbacService _rbacService;

    public AccessTestController(IRbacService rbacService)
    {
        _rbacService = rbacService;
    }

    [HttpGet("whoami")]
    public async Task<IActionResult> WhoAmI()
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        if (await _rbacService.HasRoleAsync(userId, "Admin"))
        {
            return Ok("Bạn là Admin, có toàn quyền.");
        }
        else if (await _rbacService.HasRoleAsync(userId, "Staff"))
        {
            return Ok("Bạn là Nhân viên, có quyền thêm/sửa sản phẩm.");
        }
        else if (await _rbacService.HasRoleAsync(userId, "Customer"))
        {
            return Ok("Bạn là Khách, chỉ có quyền xem sản phẩm.");
        }

        return Ok("Bạn chưa có vai trò nào trong hệ thống.");
    }
}
