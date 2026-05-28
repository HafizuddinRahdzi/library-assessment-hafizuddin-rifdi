using LibraryManagementSystem.Models;
using System.Collections.Generic;

namespace LibraryManagementSystem.Models
{
    public class Loan
    {
        public int Id { get; set; }
        public int MemberId { get; set; }
        public Member Member { get; set; } = null!;
        public int BookId { get; set; }
        public Book Book { get; set; } = null!;
        public DateTime BorrowedDate { get; set; }
        public DateTime? ReturnedDate { get; set; }
    }
}
