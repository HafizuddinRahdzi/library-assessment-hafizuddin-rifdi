using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace LibraryManagementSystem.Dtos
{
    public class LoanDto
    {
        public int Id { get; set; }
        public string BookTitle { get; set; } = string.Empty;
        public DateTime BorrowedDate { get; set; }
        public DateTime? ReturnedDate { get; set; }
    }
}
