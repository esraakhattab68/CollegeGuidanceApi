using CollegeGuideApi.DTOs.DashboardDtos;
using CollegeGuideApi.Interfaces;
using CollegeGuideApi.Models.DbContexts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CollegeGuideApi.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    [ApiController]
    public class DashboardController : ControllerBase
    {
        private readonly IDashboardService _dashboardService;
        private readonly ApplicationDbContext context;

        public DashboardController(IDashboardService dashboardService, ApplicationDbContext context)
        {
            _dashboardService = dashboardService;
            this.context = context;

        }


        [HttpPost("AddCollegeToDashboard")]
        public async Task<IActionResult> AddCollegeToDashboard([FromBody] AddSavedCollegesDto addSavedCollegesDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var response = await _dashboardService.AddSavedCollegeAsync(addSavedCollegesDto);

            if (response == null)
            {
                return BadRequest(new { message = "College already saved or not found." });
            }

            return Ok(response);
        }
        [HttpGet("GetFullSavedColleges/{studentId}")]
        public async Task<IActionResult> GetFullSavedColleges(int studentId)
        {
            var colleges = await _dashboardService.GetSavedCollegesWithFullCollegeDataAsync(studentId);

            if (!colleges.Any())
            {
                return NotFound(new { message = "No saved colleges found for this student." });
            }

            return Ok(colleges);
        }
    }
}
