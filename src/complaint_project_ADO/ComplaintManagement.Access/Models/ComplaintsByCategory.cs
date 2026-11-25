namespace ComplaintManagement.Access.Models
{
    public class ComplaintsByCategory
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public int ComplaintCount { get; set; }
    }
}