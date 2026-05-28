using LibraryManagementSystem.Data;
using LibraryManagementSystem.Dtos;
using LibraryManagementSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace LibraryManagementSystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LoanController : Controller
    {
        private readonly DBContext _context;
        private readonly ILogger<LoanController> _logger;

        public LoanController(DBContext context, ILogger<LoanController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // POST: api/loan/borrow/{bookId}
        [Authorize]
        [HttpPost("borrow/{bookId}")]
        public async Task<IActionResult> Borrow(int bookId)
        {
            var subjectId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");

            var member = await _context.Members.Include(m => m.Loans).FirstOrDefaultAsync(m => m.SubjectId == subjectId);
            if (member == null)
            {
                _logger.LogWarning("Unauthorized borrow attempt. SubjectId: {SubjectId}", subjectId);
                return Unauthorized("Member not provisioned");
            }

            var activeLoans = await _context.Loans.CountAsync(x => x.MemberId == member.Id && x.ReturnedDate == null);
            if (activeLoans >= 3)
            {
                _logger.LogInformation("Loan limit reached for MemberId: {MemberId}", member.Id);
                return BadRequest("Maximum active loans reached");
            }

            var book = await _context.Books.Include(x => x.Loans).FirstOrDefaultAsync(x => x.Id == bookId);
            if (book == null)
            {
                _logger.LogWarning("Book not found. BookId: {BookId}", bookId);
                return NotFound();
            }

            var available = book.TotalCopies - book.Loans.Count(x => x.ReturnedDate == null);
            if (available <= 0)
            {
                _logger.LogInformation("No copies available for BookId: {BookId}", bookId);
                return BadRequest("No copies available");
            }

            var loan = new Loan
            {
                BookId = bookId,
                MemberId = member.Id,
                BorrowedDate = DateTime.UtcNow
            };

            _context.Loans.Add(loan);

            await _context.SaveChangesAsync();

            _logger.LogInformation("Loan created successfully. LoanId: {LoanId}, MemberId: {MemberId}, BookId: {BookId}", loan.Id, member.Id, bookId);

            return Ok();
        }

        // POST: api/loan/return/{loanId}
        [Authorize]
        [HttpPost("return/{loanId}")]
        public async Task<IActionResult> ReturnBook(int loanId)
        {
            var subjectId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
            var loan = await _context.Loans
                .Include(l => l.Member)
                .Include(l => l.Book)
                .FirstOrDefaultAsync(l => l.Id == loanId);

            if (loan == null)
            {
                _logger.LogWarning("Loan not found. LoanId: {LoanId}", loanId);
                return NotFound();
            }
            if (loan.Member.SubjectId != subjectId)
            {
                _logger.LogWarning("Forbidden return attempt. LoanId: {LoanId}, SubjectId: {SubjectId}", loanId, subjectId);
                return Forbid();
            }

            loan.ReturnedDate = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            _logger.LogInformation("Loan returned successfully. LoanId: {LoanId}, MemberId: {MemberId}", loan.Id, loan.MemberId);

            var loanDto = new LoanDto
            {
                Id = loan.Id,
                BookTitle = loan.Book.Title,
                BorrowedDate = loan.BorrowedDate,
                ReturnedDate = loan.ReturnedDate
            };

            return Ok(loanDto);
        }

        // GET: api/loan/me
        [Authorize]
        [HttpGet("me")]
        public async Task<IActionResult> GetMyLoans()
        {
            var subjectId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");

            var member = await _context.Members.Include(m => m.Loans).ThenInclude(l => l.Book).FirstOrDefaultAsync(m => m.SubjectId == subjectId);

            if (member == null)
            {
                _logger.LogWarning("Unauthorized access to loans. SubjectId: {SubjectId}", subjectId);
                return Unauthorized();
            }

            var activeLoans = member.Loans
                .Where(l => l.ReturnedDate == null)
                .Select(l => new LoanDto
                {
                    Id = l.Id,
                    BookTitle = l.Book.Title,
                    BorrowedDate = l.BorrowedDate,
                    ReturnedDate = l.ReturnedDate
                })
                .ToList();

            _logger.LogInformation("Retrieved {Count} active loans for MemberId: {MemberId}", activeLoans.Count, member.Id);

            return Ok(activeLoans);
        }
    }
}
