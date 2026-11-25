using ComplaintManagement.Business.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace ComplainsSystemAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public class DashboardController : ControllerBase
    {
        private readonly IDashboardService _dashboardService;

        public DashboardController(
            IDashboardService dashboardService)
        {
            _dashboardService =
                dashboardService;
        }


        // =====================================================
        // GET: api/dashboard/summary
        // =====================================================

        [HttpGet("summary")]
        [EnableRateLimiting("GeneralApiLimiter")]

        public async Task<IActionResult> GetSummary()
        {
            var summary =
                await _dashboardService
                    .GetSummaryAsync();

            return Ok(summary);
        }


        // =====================================================
        // GET: api/dashboard/complaints-by-status
        // =====================================================

        [HttpGet("complaints-by-status")]
        [EnableRateLimiting("GeneralApiLimiter")]

        public async Task<IActionResult>
            GetComplaintsByStatus()
        {
            var result =
                await _dashboardService
                    .GetComplaintsByStatusAsync();

            return Ok(result);
        }


        // =====================================================
        // GET: api/dashboard/complaints-by-priority
        // =====================================================

        [HttpGet("complaints-by-priority")]
        [EnableRateLimiting("GeneralApiLimiter")]

        public async Task<IActionResult>
            GetComplaintsByPriority()
        {
            var result =
                await _dashboardService
                    .GetComplaintsByPriorityAsync();

            return Ok(result);
        }


        // =====================================================
        // GET: api/dashboard/complaints-by-category
        // =====================================================

        [HttpGet("complaints-by-category")]
        [EnableRateLimiting("GeneralApiLimiter")]

        public async Task<IActionResult>
            GetComplaintsByCategory()
        {
            var result =
                await _dashboardService
                    .GetComplaintsByCategoryAsync();

            return Ok(result);
        }


        [HttpGet("test-error")]
        public IActionResult TestError()
        {
            throw new Exception("Test exception");
        }
    }
}