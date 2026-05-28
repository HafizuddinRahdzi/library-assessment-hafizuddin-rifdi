using LibraryManagementSystem.Controllers;
using LibraryManagementSystem.Data;
using LibraryManagementSystem.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using System.Security.Claims;

namespace LMSTest
{
    public class LoanLimitRule
    {
        [Fact]
        public async Task Borrow_ShouldReject_WhenMemberHasThreeActiveLoans()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<DBContext>()
                .UseInMemoryDatabase("LoanLimitTest")
                .Options;

            using var context = new DBContext(options);
            var controller = new LoanController(context, NullLogger<LoanController>.Instance);

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
                    {
                new Claim(ClaimTypes.NameIdentifier, "sub123")
                    }, "mock"))
                }
            };

            var member = new Member { Id = 1, SubjectId = "sub123" };
            context.Members.Add(member);

            // Add 3 active loans
            context.Loans.AddRange(
                new Loan { BookId = 1, MemberId = 1, BorrowedDate = DateTime.UtcNow },
                new Loan { BookId = 2, MemberId = 1, BorrowedDate = DateTime.UtcNow },
                new Loan { BookId = 3, MemberId = 1, BorrowedDate = DateTime.UtcNow }
            );
            context.SaveChanges();

            // Act
            var result = await controller.Borrow(4);

            // Assert
            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Maximum active loans reached", badRequest.Value);
        }

        [Fact]
        public async Task Borrow_ShouldReject_WhenNoCopiesAvailable()
        {
            var options = new DbContextOptionsBuilder<DBContext>()
                .UseInMemoryDatabase("NoCopiesTest")
                .Options;

            using var context = new DBContext(options);
            var controller = new LoanController(context, NullLogger<LoanController>.Instance);

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
                    {
            new Claim(ClaimTypes.NameIdentifier, "sub123")
                    }, "mock"))
                }
            };

            var member = new Member { Id = 1, SubjectId = "sub123" };
            context.Members.Add(member);

            var book = new Book { Id = 1, Title = "Test Book", Author = "Author", TotalCopies = 1 };
            context.Books.Add(book);

            context.Loans.Add(new Loan { BookId = 1, MemberId = 1, BorrowedDate = DateTime.UtcNow });
            context.SaveChanges();

            // Act
            var result = await controller.Borrow(1);

            // Assert
            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("No copies available", badRequest.Value);
        }

        [Fact]
        public async Task ReturnBook_ShouldForbid_WhenLoanBelongsToAnotherMember()
        {
            var options = new DbContextOptionsBuilder<DBContext>()
                .UseInMemoryDatabase("ReturnBookTest")
                .Options;

            using var context = new DBContext(options);
            var controller = new LoanController(context, NullLogger<LoanController>.Instance);

            var member1 = new Member { Id = 1, SubjectId = "sub123" };
            var member2 = new Member { Id = 2, SubjectId = "sub456" };
            context.Members.AddRange(member1, member2);

            var book = new Book { Id = 1, Title = "Test Book", Author = "Author", TotalCopies = 1 };
            context.Books.Add(book);

            var loan = new Loan { Id = 1, BookId = 1, MemberId = 2, BorrowedDate = DateTime.UtcNow };
            context.Loans.Add(loan);
            context.SaveChanges();

            // Simulate authenticated user with subjectId "sub123"
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
                    {
                new Claim(ClaimTypes.NameIdentifier, "sub123")
                    }, "mock"))
                }
            };

            var result = await controller.ReturnBook(1);

            Assert.IsType<ForbidResult>(result);
        }
    }
}