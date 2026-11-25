using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ComplaintManagement.Business.DTOs.Dashboard
{
    public class ComplaintsByStatusDto
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public int ComplaintCount { get; set; }
    }
}
