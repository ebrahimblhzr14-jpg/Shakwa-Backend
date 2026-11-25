using ComplaintManagement.Access.Models;
using ComplaintManagement.Business.Enums;
using ComplaintManagement.Business.Services;
using ComplaintManagement.Tests.TestDoubles;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace ComplaintManagement.Tests
{
    public class ConcurrencyClosureTests
    {
        [Fact]
        public async Task ConcurrentClosures_OnSameComplaint_OnlyOneSucceeds_AndStoredProcCalledOnce()
        {
            // Arrange
            var repo = new FakeComplaintsRepository();
            repo.SimulatedDelayMs = 50; // Simulate DB latency to ensure overlapping execution

            repo.SeedComplaint(new Complaint
            {
                Id = 100,
                UserId = 10,
                StatusId = 2, // Under Review
                Title = "Concurrent Test Complaint"
            });

            // Simulate two distinct scoped requests by instantiating two separate ComplaintService instances
            var serviceScope1 = new ComplaintService(NullLogger<ComplaintService>.Instance, repo);
            var serviceScope2 = new ComplaintService(NullLogger<ComplaintService>.Instance, repo);

            // Act: Launch both closures simultaneously via Task.WhenAll
            var task1 = Task.Run(() => serviceScope1.UpdateStatusAsync(100, 5, 101));
            var task2 = Task.Run(() => serviceScope2.UpdateStatusAsync(100, 5, 102));

            var results = await Task.WhenAll(task1, task2);

            // Assert
            Assert.Contains(StatusUpdateResult.Success, results);
            Assert.Contains(StatusUpdateResult.AlreadyClosed, results);

            // CRITICAL CONCURRENCY INVARIANT: Stored procedure was executed EXACTLY once
            Assert.Equal(1, repo.UpdateStatusCallCount);

            // Final state verification
            var finalState = await repo.GetByIdAsync(100);
            Assert.NotNull(finalState);
            Assert.Equal(5, finalState.StatusId);
            Assert.NotNull(finalState.ClosedAt);
        }

        [Fact]
        public async Task ConcurrentClosureAndNormalStatusUpdate_AreSerializedAndPreventRaceCondition()
        {
            // Arrange
            var repo = new FakeComplaintsRepository();
            repo.SimulatedDelayMs = 50;

            repo.SeedComplaint(new Complaint
            {
                Id = 101,
                UserId = 10,
                StatusId = 2, // Under Review
                Title = "Race Test Complaint"
            });

            var serviceScope1 = new ComplaintService(NullLogger<ComplaintService>.Instance, repo);
            var serviceScope2 = new ComplaintService(NullLogger<ComplaintService>.Instance, repo);

            // Task 1: Close complaint
            // Task 2: Move to In Progress (3)
            var closeTask = Task.Run(() => serviceScope1.UpdateStatusAsync(101, 5, 101));
            var updateTask = Task.Run(async () =>
            {
                await Task.Delay(10); // Slight stagger to hit lock while Task 1 is inside
                return await serviceScope2.UpdateStatusAsync(101, 3, 102);
            });

            var results = await Task.WhenAll(closeTask, updateTask);

            // Assert: Close succeeded
            Assert.Equal(StatusUpdateResult.Success, results[0]);
            // The competing update saw the complaint was already closed and was rejected
            Assert.Equal(StatusUpdateResult.CannotModifyClosed, results[1]);

            // Stored procedure called only once (for closure)
            Assert.Equal(1, repo.UpdateStatusCallCount);
        }

        [Fact]
        public async Task ConcurrentUpdates_OnDifferentComplaints_ExecuteIndependently()
        {
            // Arrange
            var repo = new FakeComplaintsRepository();
            repo.SimulatedDelayMs = 25;

            repo.SeedComplaint(new Complaint { Id = 201, StatusId = 1, Title = "Complaint 201" });
            repo.SeedComplaint(new Complaint { Id = 202, StatusId = 1, Title = "Complaint 202" });

            var serviceScope1 = new ComplaintService(NullLogger<ComplaintService>.Instance, repo);
            var serviceScope2 = new ComplaintService(NullLogger<ComplaintService>.Instance, repo);

            // Act: Update Complaint 201 and Complaint 202 simultaneously
            var task1 = Task.Run(() => serviceScope1.UpdateStatusAsync(201, 3, 101));
            var task2 = Task.Run(() => serviceScope2.UpdateStatusAsync(202, 3, 102));

            var results = await Task.WhenAll(task1, task2);

            // Assert: Both independent complaints succeeded
            Assert.Equal(StatusUpdateResult.Success, results[0]);
            Assert.Equal(StatusUpdateResult.Success, results[1]);
            Assert.Equal(2, repo.UpdateStatusCallCount);
        }

        [Fact]
        public async Task UpdateStatus_WhenExceptionOccurs_LockIsSafelyReleased()
        {
            // Arrange: A faulty repository that throws once
            var repo = new FaultyComplaintsRepository();
            repo.SeedComplaint(new Complaint { Id = 301, StatusId = 1, Title = "Faulty Complaint" });

            var service = new ComplaintService(NullLogger<ComplaintService>.Instance, repo);

            // Act 1: First call throws
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                service.UpdateStatusAsync(301, 3, 99));

            // Act 2: Second call on same complaint should NOT deadlock because lock was released in finally
            repo.ThrowOnUpdate = false;
            var result = await service.UpdateStatusAsync(301, 3, 99);

            // Assert
            Assert.Equal(StatusUpdateResult.Success, result);
        }

        private class FaultyComplaintsRepository : FakeComplaintsRepository
        {
            public bool ThrowOnUpdate = true;

            public override Task<bool> UpdateStatusAsync(int complaintId, int statusId, int userId)
            {
                if (ThrowOnUpdate)
                    throw new InvalidOperationException("Simulated transient database crash");

                return base.UpdateStatusAsync(complaintId, statusId, userId);
            }
        }
    }
}
