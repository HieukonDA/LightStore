using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using TheLightStore.Hubs;

namespace TheLightStore.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class SignalRDebugController : ControllerBase
{
    private readonly IHubContext<NotificationHub> _hubContext;
    private readonly ILogger<SignalRDebugController> _logger;

    public SignalRDebugController(
        IHubContext<NotificationHub> hubContext,
        ILogger<SignalRDebugController> logger)
    {
        _hubContext = hubContext;
        _logger = logger;
    }

    /// <summary>
    /// Test SignalR Hub connection
    /// </summary>
    [HttpGet("test-hub")]
    public async Task<IActionResult> TestHub()
    {
        try
        {
            await _hubContext.Clients.All.SendAsync("TestMessage", "SignalR Hub is working!", DateTime.UtcNow);
            return Ok(new { 
                success = true, 
                message = "SignalR test message sent successfully",
                hubEndpoint = "/notificationHub",
                timestamp = DateTime.UtcNow 
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "SignalR Hub test failed");
            return StatusCode(500, new { 
                success = false, 
                message = "SignalR Hub test failed", 
                error = ex.Message 
            });
        }
    }

    /// <summary>
    /// Test admin notification
    /// </summary>
    [HttpPost("test-admin")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> TestAdminNotification()
    {
        try
        {
            var notification = new
            {
                Type = "test",
                Title = "Test Admin Notification",
                Content = "This is a test notification for admins",
                Priority = "normal",
                Timestamp = DateTime.UtcNow
            };

            await _hubContext.Clients.Group("AdminGroup").SendAsync("ReceiveNotification", notification);
            
            return Ok(new { 
                success = true, 
                message = "Admin test notification sent successfully",
                notification 
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Admin notification test failed");
            return StatusCode(500, new { 
                success = false, 
                message = "Admin notification test failed", 
                error = ex.Message 
            });
        }
    }

    /// <summary>
    /// Test customer notification
    /// </summary>
    [HttpPost("test-customer/{customerId}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> TestCustomerNotification(int customerId)
    {
        try
        {
            var notification = new
            {
                Type = "test",
                Title = "Test Customer Notification",
                Content = $"This is a test notification for customer {customerId}",
                Priority = "normal",
                Timestamp = DateTime.UtcNow
            };

            await _hubContext.Clients.Group($"Customer_{customerId}").SendAsync("ReceiveNotification", notification);
            
            return Ok(new { 
                success = true, 
                message = $"Customer test notification sent to {customerId}",
                notification 
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Customer notification test failed");
            return StatusCode(500, new { 
                success = false, 
                message = "Customer notification test failed", 
                error = ex.Message 
            });
        }
    }

    /// <summary>
    /// Get SignalR Hub info
    /// </summary>
    [HttpGet("hub-info")]
    public IActionResult GetHubInfo()
    {
        var info = new
        {
            hubEndpoint = "/notificationHub",
            negotiateEndpoint = "/notificationHub/negotiate",
            supportedTransports = new[] { "WebSockets", "ServerSentEvents", "LongPolling" },
            authenticationRequired = true,
            corsEnabled = true,
            allowedOrigins = new[] {
                "http://localhost:5173",
                "https://localhost:5264",
                "http://localhost:5264",
                "https://thelightstore.io.vn",
                "http://thelightstore.io.vn"
            },
            groups = new[] {
                "AdminGroup",
                "CustomersGroup",
                "User_{userId}",
                "Customer_{customerId}"
            },
            methods = new[] {
                "JoinAdminGroup",
                "LeaveAdminGroup", 
                "JoinCustomerGroup",
                "LeaveCustomerGroup",
                "Ping"
            }
        };

        return Ok(info);
    }

    /// <summary>
    /// Check if user can connect to SignalR
    /// </summary>
    [HttpGet("connection-check")]
    [Authorize]
    public IActionResult CheckConnection()
    {
        var userId = User.FindFirst("sub")?.Value ?? User.FindFirst("id")?.Value;
        var roles = User.FindAll("role").Select(c => c.Value).ToArray();
        var name = User.FindFirst("name")?.Value ?? User.FindFirst("email")?.Value;

        var connectionInfo = new
        {
            canConnect = true,
            userId = userId,
            roles = roles,
            name = name,
            expectedGroups = GetExpectedGroups(userId, roles),
            hubUrl = "/notificationHub",
            authToken = "Use Authorization header Bearer token",
            instructions = new
            {
                step1 = "Get JWT token from login",
                step2 = "Connect to /notificationHub with access_token query parameter",
                step3 = "Listen for 'ReceiveNotification' events",
                step4 = "Call JoinAdminGroup() or JoinCustomerGroup() if needed"
            }
        };

        return Ok(connectionInfo);
    }

    private string[] GetExpectedGroups(string? userId, string[] roles)
    {
        var groups = new List<string>();
        
        if (!string.IsNullOrEmpty(userId))
        {
            groups.Add($"User_{userId}");
            
            if (roles.Contains("Admin") || roles.Contains("Staff"))
            {
                groups.Add("AdminGroup");
            }
            
            if (roles.Contains("Customer"))
            {
                groups.Add($"Customer_{userId}");
                groups.Add("CustomersGroup");
            }
        }
        
        return groups.ToArray();
    }
}