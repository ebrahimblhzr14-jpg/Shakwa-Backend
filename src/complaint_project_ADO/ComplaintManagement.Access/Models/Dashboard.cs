namespace ComplaintManagement.Access.Models
{
    public class DashboardSummary
    {
        public int TotalComplaints { get; set; }

        public int PendingComplaints { get; set; }

        public int InProgressComplaints { get; set; }

        public int ResolvedComplaints { get; set; }

        public int ClosedComplaints { get; set; }
    }
}