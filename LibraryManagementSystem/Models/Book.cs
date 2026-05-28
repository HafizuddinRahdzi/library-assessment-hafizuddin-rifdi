using System.ComponentModel.DataAnnotations;

namespace LibraryManagementSystem.Models
{
    public class Book
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Title { get; set; } = "";

        [Required]
        [StringLength(100)]
        public string Author { get; set; } = "";

        [Required]
        [StringLength(25, MinimumLength = 10)]
        public string ISBN { get; set; } = "";

        [Range(1450, 2100)]
        public int PublishedYear { get; set; }

        [Range(1, int.MaxValue)]
        public int TotalCopies {  get; set; }
        public ICollection<Loan> Loans { get; set; } = new List<Loan>();
    }
}
