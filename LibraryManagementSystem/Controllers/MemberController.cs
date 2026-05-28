using Microsoft.AspNetCore.Mvc;
using LibraryManagementSystem.Models;
using System.Security.Claims;
using LibraryManagementSystem.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagementSystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MemberController : Controller
    {
        private readonly DBContext _context;

        private readonly ILogger<MemberController> _logger;

        public MemberController(DBContext context, ILogger<MemberController> logger)
        {
            _context = context;
            _logger = logger;
        }

        //Get: api/member/me
        [Authorize]
        [HttpGet("me")]
        public async Task<IActionResult> GetMe()
        {
            var subjectId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");

            _logger.LogInformation("Fetching member profile for SubjectId: {SubjectId}", subjectId);

            var member = await _context.Members.FirstOrDefaultAsync(m => m.SubjectId == subjectId);

            if (member == null)
            {
                _logger.LogWarning("Member not provisioned. SubjectId: {SubjectId}", subjectId);
                return Unauthorized("Member not provisioned. Please check middleware configuration.");
            }

            _logger.LogInformation("Member profile retrieved successfully. MemberId: {MemberId}", member.Id);
            return Ok(member);
        }
    }
}
