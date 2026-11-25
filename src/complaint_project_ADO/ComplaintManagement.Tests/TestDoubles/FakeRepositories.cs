using ComplaintManagement.Access.Interfaces;
using ComplaintManagement.Access.Models;
using System.Collections.Concurrent;

namespace ComplaintManagement.Tests.TestDoubles
{
    public class FakeComplaintsRepository : IComplaintsRepository
    {
        private readonly ConcurrentDictionary<int, Complaint> _complaints = new();
        public int UpdateStatusCallCount = 0;
        public int SimulatedDelayMs = 0;

        public void SeedComplaint(Complaint complaint)
        {
            _complaints[complaint.Id] = complaint;
        }

        public Task<int> CreateAsync(Complaint complaint)
        {
            int id = _complaints.Count + 1;
            complaint.Id = id;
            _complaints[id] = complaint;
            return Task.FromResult(id);
        }

        public Task<IEnumerable<Complaint>> GetAllAsync()
        {
            return Task.FromResult<IEnumerable<Complaint>>(_complaints.Values.ToList());
        }

        public Task<Complaint?> GetByIdAsync(int id)
        {
            _complaints.TryGetValue(id, out var c);
            if (c == null)
                return Task.FromResult<Complaint?>(null);

            // Return a cloned copy to simulate database read
            var clone = new Complaint
            {
                Id = c.Id,
                UserId = c.UserId,
                UserName = c.UserName,
                CategoryId = c.CategoryId,
                DepartmentId = c.DepartmentId,
                StatusId = c.StatusId,
                StatusName = c.StatusName,
                PriorityId = c.PriorityId,
                Title = c.Title,
                Description = c.Description,
                CreatedAt = c.CreatedAt,
                UpdatedAt = c.UpdatedAt,
                ResolvedAt = c.ResolvedAt,
                ClosedAt = c.ClosedAt
            };
            return Task.FromResult<Complaint?>(clone);
        }

        public Task<IEnumerable<Complaint>> GetByUserAsync(int userId)
        {
            return Task.FromResult<IEnumerable<Complaint>>(
                _complaints.Values.Where(c => c.UserId == userId).ToList());
        }

        public Task<IEnumerable<Complaint>> GetAssignedToAdminAsync(int adminId)
        {
            return Task.FromResult<IEnumerable<Complaint>>(_complaints.Values.ToList());
        }

        public Task<bool> UpdateAsync(Complaint complaint)
        {
            _complaints[complaint.Id] = complaint;
            return Task.FromResult(true);
        }

        public Task<bool> DeleteAsync(int id)
        {
            return Task.FromResult(_complaints.TryRemove(id, out _));
        }

        public virtual async Task<bool> UpdateStatusAsync(int complaintId, int statusId, int userId)
        {
            Interlocked.Increment(ref UpdateStatusCallCount);

            if (SimulatedDelayMs > 0)
            {
                await Task.Delay(SimulatedDelayMs);
            }

            if (!_complaints.TryGetValue(complaintId, out var complaint))
                return false;

            complaint.StatusId = statusId;
            complaint.UpdatedAt = DateTime.UtcNow;

            if (statusId == 4)
                complaint.ResolvedAt = DateTime.UtcNow;

            if (statusId == 5)
                complaint.ClosedAt = DateTime.UtcNow;

            return true;
        }
    }

    public class FakeAssignmentsRepository : IAssignmentsRepository
    {
        private readonly List<ComplaintAssignment> _assignments = new();

        public void SeedAssignment(int complaintId, int adminId, bool isActive = true)
        {
            _assignments.Add(new ComplaintAssignment
            {
                Id = _assignments.Count + 1,
                ComplaintId = complaintId,
                AssignedToUserId = adminId,
                AssignedAt = DateTime.UtcNow.AddDays(-1),
                UnassignedAt = isActive ? null : DateTime.UtcNow.AddHours(-1)
            });
        }

        public Task<int> CreateAsync(ComplaintAssignment assignment)
        {
            assignment.Id = _assignments.Count + 1;
            _assignments.Add(assignment);
            return Task.FromResult(assignment.Id);
        }

        public Task<IEnumerable<ComplaintAssignment>> GetAllAsync()
        {
            return Task.FromResult<IEnumerable<ComplaintAssignment>>(_assignments.ToList());
        }

        public Task<ComplaintAssignment?> GetByIdAsync(int id)
        {
            return Task.FromResult(_assignments.FirstOrDefault(a => a.Id == id));
        }

        public Task<bool> UpdateAsync(ComplaintAssignment assignment)
        {
            return Task.FromResult(true);
        }

        public Task<bool> DeleteAsync(int id)
        {
            return Task.FromResult(true);
        }

        public Task<bool> HasActiveAssignmentAsync(int complaintId, int adminId)
        {
            bool hasActive = _assignments.Any(a =>
                a.ComplaintId == complaintId &&
                a.AssignedToUserId == adminId &&
                a.UnassignedAt == null);

            return Task.FromResult(hasActive);
        }
    }
}
