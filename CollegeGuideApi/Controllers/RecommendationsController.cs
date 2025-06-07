using CollegeGuideApi.Interfaces;
using CollegeGuideApi.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CollegeGuideApi.Controllers
{
    [Route("api/[controller]")]
    [Authorize(Roles = "Student")]
    [ApiController]
    public class RecommendationsController : ControllerBase
    {
        private readonly IRecommendationService _recommendationService;


        public RecommendationsController(IRecommendationService recommendationService)
        {
            _recommendationService = recommendationService;
        }

        [HttpGet("GetAllRecommendedColleges")]
        public async Task<ActionResult<IEnumerable<College>>> GetAllRecommendedColleges(string query)
        {
            var result = await _recommendationService.GetAllRecommendedCollegesAsync(query);

            if (!result.Any())
                return NotFound("No colleges found matching the query.");

            return Ok(result);
        }
    }
}
