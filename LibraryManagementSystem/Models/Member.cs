namespace LibraryManagementSystem.Models
{
    public class Member
    {
        public int Id { get; set; }
        public string SubjectId { get; set; } = "";
        public string FullName { get; set; } = "";
        public string Email { get; set; } = "";
        public DateTime JoinedDate { get; set; }
        public ICollection<Loan> Loans { get; set; } = new List<Loan>();
    }
}
