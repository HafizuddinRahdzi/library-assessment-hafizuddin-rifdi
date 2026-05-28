using LibraryManagementSystem.Data;
using LibraryManagementSystem.Dtos;
using LibraryManagementSystem.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagementSystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BookController : Controller
    {
        private readonly DBContext _context;

        private readonly ILogger<BookController> _logger;

        public BookController(DBContext context, ILogger<BookController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // POST: api/book
        [HttpPost]
        public async Task<IActionResult> AddBook([FromBody] Book book)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage);

                _logger.LogWarning("Invalid book data submitted: {@Errors}", errors);
                return BadRequest(ModelState);
            }

            _context.Books.Add(book);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Book added successfully: Book ID: {BookId}, Title: {Title}, Author: {Author}",
                book.Id, book.Title, book.Author);

            return CreatedAtAction(nameof(GetBookById), new { id = book.Id }, book);
        }

        // GET: api/book
        [HttpGet]
        public async Task<IActionResult> GetBooks([FromQuery] string? author, [FromQuery] string? title)
        {
            var query = _context.Books.AsQueryable();

            if (!string.IsNullOrEmpty(author))
            {
                query = query.Where(b => b.Author.Contains(author));

                _logger.LogInformation("Filtering books by author: {Author}", author);
            }

            if (!string.IsNullOrEmpty(title))
            {
                query = query.Where(b => b.Title.Contains(title));

                _logger.LogInformation("Filtering books by title: {Title}", title);
            }
            
            var books = await query.ToListAsync();

            _logger.LogInformation("Retrieved {Count} books", books.Count);

            return Ok(books);
        }

        // GET: api/book/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetBookById(int id)
        {
            var book = await _context.Books
                .Include(b => b.Loans)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (book == null)
            {
                _logger.LogWarning("Book not found with ID: {BookId}", id);
                return NotFound();
            }

            var availableCopies = book.TotalCopies - book.Loans.Count(l => l.ReturnedDate == null);

            var dto = new BookDto
            {
                Id = book.Id,
                Title = book.Title,
                Author = book.Author,
                ISBN = book.ISBN,
                PublishedYear = book.PublishedYear,
                TotalCopies = book.TotalCopies,
                AvailableCopies = availableCopies
            };

            _logger.LogInformation("Book retrieved: Book ID: {BookId}, Title: {Title}, Author: {Author}", book.Id, book.Title, book.Author);

            return Ok(dto);
        }

    }
}
