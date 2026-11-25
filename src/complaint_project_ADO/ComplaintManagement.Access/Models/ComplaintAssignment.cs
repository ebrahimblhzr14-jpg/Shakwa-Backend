using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ComplaintManagement.Access.Models
{
    public class ComplaintAssignment
    {
        public int Id { get; set; }

        public int ComplaintId { get; set; }

        public int AssignedToUserId { get; set; }

        public string? AssignedToUserName { get; set; }

        public string? AssignedToUserImageUrl { get; set; }

        public int AssignedByUserId { get; set; }

        public string? AssignedByUserName { get; set; }

        public string? AssignedByUserImageUrl { get; set; }

        public DateTime AssignedAt { get; set; }

        public DateTime? UnassignedAt { get; set; }

        public string? Notes { get; set; }
    }
}
