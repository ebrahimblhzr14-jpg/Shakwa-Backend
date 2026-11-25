using ComplainsSystemAPI.Controllers;
using ComplaintManagement.Business.DTOs.Assignments;
using ComplaintManagement.Business.DTOs.Complaints;
using ComplaintManagement.Business.Enums;
using ComplaintManagement.Business.Interfaces;
using ComplaintManagement.Tests.TestDoubles;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using System.Security.Claims;
using Xunit;

namespace ComplaintManagement.Tests
{
    public class ControllerResultMappingTests
    {
        private readonly FakeConfigurableComplaintService _compService;
        private readonly FakeAssignmentsRepository _assignRepo;
        private readonly ComplaintsController _controller;

        public ControllerResultMappingTests()
        {
            _compService = new FakeConfigurableComplaintService();
            _assignRepo = new FakeAssignmentsRepository();
            var fakeAssignmentService = new FakeAssignmentService();
            _controller = new ComplaintsController(
                _compService,
                fakeAssignmentService,
                _assignRepo,
                NullLogger<ComplaintsController>.Instance);
        }

        private void SetCallerUser(string userId, string role)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userId),
                new Claim(ClaimTypes.Role, role)
            };
            var identity = new ClaimsIdentity(claims, "TestAuth");
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(identity) }
            };
        }

        [Fact]
        public async Task ChangeStatus_WhenServiceReturnsSuccess_ControllerReturns204NoContent()
        {
            SetCallerUser("50", "Admin");
            _assignRepo.SeedAssignment(1, 50, isActive: true);
            _compService.ConfiguredResult = StatusUpdateResult.Success;

            var result = await _controller.ChangeStatus(1, new UpdateComplaintStatusDto { StatusId = 3 });

            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task ChangeStatus_WhenServiceReturnsNotFound_ControllerReturns404NotFound()
        {
            SetCallerUser("50", "Admin");
            _assignRepo.SeedAssignment(1, 50, isActive: true);
            _compService.ConfiguredResult = StatusUpdateResult.NotFound;

            var result = await _controller.ChangeStatus(1, new UpdateComplaintStatusDto { StatusId = 3 });

            var notFound = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal(404, notFound.StatusCode);
        }

        [Fact]
        public async Task ChangeStatus_WhenServiceReturnsAlreadyClosed_ControllerReturns409Conflict()
        {
            SetCallerUser("50", "Admin");
            _assignRepo.SeedAssignment(1, 50, isActive: true);
            _compService.ConfiguredResult = StatusUpdateResult.AlreadyClosed;

            var result = await _controller.ChangeStatus(1, new UpdateComplaintStatusDto { StatusId = 5 });

            var conflict = Assert.IsType<ConflictObjectResult>(result);
            Assert.Equal(409, conflict.StatusCode);
        }

        [Fact]
        public async Task ChangeStatus_WhenServiceReturnsCannotModifyClosed_ControllerReturns400BadRequest()
        {
            SetCallerUser("50", "Admin");
            _assignRepo.SeedAssignment(1, 50, isActive: true);
            _compService.ConfiguredResult = StatusUpdateResult.CannotModifyClosed;

            var result = await _controller.ChangeStatus(1, new UpdateComplaintStatusDto { StatusId = 2 });

            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(400, badRequest.StatusCode);
        }

        [Fact]
        public async Task ChangeStatus_WhenServiceReturnsInvalidTransition_ControllerReturns400BadRequest()
        {
            SetCallerUser("50", "Admin");
            _assignRepo.SeedAssignment(1, 50, isActive: true);
            _compService.ConfiguredResult = StatusUpdateResult.InvalidTransition;

            var result = await _controller.ChangeStatus(1, new UpdateComplaintStatusDto { StatusId = 99 });

            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(400, badRequest.StatusCode);
        }

        [Fact]
        public async Task ChangeStatus_WhenAdminIsNotAssigned_ControllerReturns403Forbidden()
        {
            SetCallerUser("50", "Admin");
            // No assignment seeded for Admin 50 on Complaint 1

            var result = await _controller.ChangeStatus(1, new UpdateComplaintStatusDto { StatusId = 3 });

            var forbidden = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status403Forbidden, forbidden.StatusCode);
        }
    }

    public class FakeConfigurableComplaintService : IComplaintService
    {
        public StatusUpdateResult ConfiguredResult = StatusUpdateResult.Success;

        public Task<StatusUpdateResult> UpdateStatusAsync(int complaintId, int statusId, int userId) =>
            Task.FromResult(ConfiguredResult);

        public Task<int> CreateAsync(int userId, CreateComplaintDto dto) => Task.FromResult(1);
        public Task<IEnumerable<ComplaintDto>> GetAllAsync() => Task.FromResult<IEnumerable<ComplaintDto>>(new List<ComplaintDto>());
        public Task<ComplaintDto?> GetByIdAsync(int id) => Task.FromResult<ComplaintDto?>(new ComplaintDto { Id = id });
        public Task<IEnumerable<ComplaintDto>> GetByUserAsync(int userId) => Task.FromResult<IEnumerable<ComplaintDto>>(new List<ComplaintDto>());
        public Task<IEnumerable<ComplaintDto>> GetAssignedToAdminAsync(int adminId) => Task.FromResult<IEnumerable<ComplaintDto>>(new List<ComplaintDto>());
        public Task<bool> UpdateAsync(int id, int userId, CreateComplaintDto dto) => Task.FromResult(true);
        public Task<bool> DeleteAsync(int id, int userId) => Task.FromResult(true);
    }

    public class FakeAssignmentService : IAssignmentService
    {
        public Task<int> CreateAsync(int assignedByUserId, CreateAssignmentDto dto) => Task.FromResult(1);
        public Task<IEnumerable<AssignmentDto>> GetAllAsync() => Task.FromResult<IEnumerable<AssignmentDto>>(new List<AssignmentDto>());
        public Task<AssignmentDto?> GetByIdAsync(int id) => Task.FromResult<AssignmentDto?>(null);
        public Task<bool> UpdateAsync(int id, UpdateAssignmentDto dto) => Task.FromResult(true);
        public Task<bool> DeleteAsync(int id) => Task.FromResult(true);
    }
}
