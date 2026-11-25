using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ComplaintManagement.Business.DTOs.Dashboard
{
    public class ComplaintSummaryDto
    {
        public int TotalComplaints { get; set; }

        public int PendingComplaints { get; set; }

        public int InProgressComplaints { get; set; }

        public int ResolvedComplaints { get; set; }

        public int ClosedComplaints { get; set; }
    }
}
