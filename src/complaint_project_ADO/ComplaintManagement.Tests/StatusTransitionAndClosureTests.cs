using ComplaintManagement.Access.Models;
using ComplaintManagement.Business.Enums;
using ComplaintManagement.Business.Services;
using ComplaintManagement.Tests.TestDoubles;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace ComplaintManagement.Tests
{
    public class StatusTransitionAndClosureTests
    {
        private readonly FakeComplaintsRepository _repo;
        private readonly ComplaintService _service;

        public StatusTransitionAndClosureTests()
        {
            _repo = new FakeComplaintsRepository();
            _service = new ComplaintService(NullLogger<ComplaintService>.Instance, _repo);
        }

        [Fact]
        public async Task UpdateStatus_ValidTransition_ReturnsSuccess()
        {
            // Arrange
            _repo.SeedComplaint(new Complaint
            {
                Id = 1,
                UserId = 10,
                StatusId = 1, // Pending
                Title = "Test Complaint"
            });

            // Act: Transition Pending (1) -> In Progress (3)
            var result = await _service.UpdateStatusAsync(1, 3, 99);

            // Assert
            Assert.Equal(StatusUpdateResult.Success, result);
            var updated = await _repo.GetByIdAsync(1);
            Assert.NotNull(updated);
            Assert.Equal(3, updated.StatusId);
        }

        [Fact]
        public async Task UpdateStatus_CloseActiveComplaint_SetsClosedStatusAndTimestamp()
        {
            // Arrange
            _repo.SeedComplaint(new Complaint
            {
                Id = 2,
                UserId = 10,
                StatusId = 2, // Under Review
                Title = "Active Complaint"
            });

            // Act: Close complaint (Status 5)
            var result = await _service.UpdateStatusAsync(2, 5, 99);

            // Assert
            Assert.Equal(StatusUpdateResult.Success, result);
            var updated = await _repo.GetByIdAsync(2);
            Assert.NotNull(updated);
            Assert.Equal(5, updated.StatusId);
            Assert.NotNull(updated.ClosedAt);
        }

        [Fact]
        public async Task UpdateStatus_AlreadyClosedComplaint_ReturnsAlreadyClosed()
        {
            // Arrange
            _repo.SeedComplaint(new Complaint
            {
                Id = 3,
                UserId = 10,
                StatusId = 5, // Already closed
                ClosedAt = DateTime.UtcNow.AddHours(-2),
                Title = "Closed Complaint"
            });

            // Act: Attempt to close again
            var result = await _service.UpdateStatusAsync(3, 5, 99);

            // Assert
            Assert.Equal(StatusUpdateResult.AlreadyClosed, result);
            // Verify stored procedure call was avoided
            Assert.Equal(0, _repo.UpdateStatusCallCount);
        }

        [Fact]
        public async Task UpdateStatus_ModifyClosedComplaintToOtherStatus_ReturnsCannotModifyClosed()
        {
            // Arrange
            _repo.SeedComplaint(new Complaint
            {
                Id = 4,
                UserId = 10,
                StatusId = 5, // Closed
                ClosedAt = DateTime.UtcNow.AddHours(-1),
                Title = "Closed Complaint"
            });

            // Act: Attempt to reopen/move to In Progress (3)
            var result = await _service.UpdateStatusAsync(4, 3, 99);

            // Assert
            Assert.Equal(StatusUpdateResult.CannotModifyClosed, result);
            Assert.Equal(0, _repo.UpdateStatusCallCount);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(6)]
        [InlineData(99)]
        public async Task UpdateStatus_InvalidStatusId_ReturnsInvalidTransition(int invalidStatusId)
        {
            // Arrange
            _repo.SeedComplaint(new Complaint
            {
                Id = 5,
                UserId = 10,
                StatusId = 1,
                Title = "Pending Complaint"
            });

            // Act
            var result = await _service.UpdateStatusAsync(5, invalidStatusId, 99);

            // Assert
            Assert.Equal(StatusUpdateResult.InvalidTransition, result);
            Assert.Equal(0, _repo.UpdateStatusCallCount);
        }

        [Fact]
        public async Task UpdateStatus_NonExistentComplaint_ReturnsNotFound()
        {
            // Act: Complaint ID 999 does not exist
            var result = await _service.UpdateStatusAsync(999, 2, 99);

            // Assert
            Assert.Equal(StatusUpdateResult.NotFound, result);
            Assert.Equal(0, _repo.UpdateStatusCallCount);
        }
    }
}
