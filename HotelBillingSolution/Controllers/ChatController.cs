using Microsoft.AspNetCore.Mvc;
using HotelBilling.Application.Interfaces;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace HotelBillingSolution.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ChatController : ControllerBase
    {
        private readonly IChatService _chatService;

        public ChatController(IChatService chatService)
        {
            _chatService = chatService;
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Post([FromBody] ChatRequest req)
        {
            if (req == null || string.IsNullOrWhiteSpace(req.Message))
                return BadRequest(new { error = "Message is required" });

            var userName = User?.Identity?.Name ?? "Anonymous";
            var role = User?.FindFirst("role")?.Value ?? User?.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value ?? "User";

            var result = await _chatService.ProcessMessageAsync(req.Message, userName, role);
            return Ok(result);
        }

        public class ChatRequest { public string Message { get; set; } }
    }
}
