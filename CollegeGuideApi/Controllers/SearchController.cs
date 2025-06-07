using CollegeGuideApi.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CollegeGuideApi.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    [ApiController]
    public class SearchController : ControllerBase
    {
        private readonly ISearchService _locationService;

        public SearchController(ISearchService locationService)
        {
            _locationService = locationService;
        }

        #region Cities

        [HttpGet("GetAllCities")]
        public async Task<IActionResult> GetAllCities()
        {
            var cities = await _locationService.GetAllCitiesAsync();
            return Ok(cities);
        }

        #endregion

        #region Universities

        [HttpGet("ByCity/{cityId}")]
        public async Task<IActionResult> GetUniversitiesByCity(int cityId)
        {
            if (cityId <= 0)
            {
                return BadRequest("Invalid City ID.");
            }
            var universities = await _locationService.GetUniversitiesByCityAsync(cityId);
            return Ok(universities);
        }

        [HttpGet("University/{id}")]
        public async Task<IActionResult> GetUniversityDetails(int universityId)
        {
            if (universityId <= 0)
            {
                return BadRequest("Invalid University ID.");
            }
            var universityDetails = await _locationService.GetUniversityDetailsAsync(universityId);
            if (universityDetails == null)
            {
                return NotFound($"University with ID {universityId} not found.");
            }
            return Ok(universityDetails);
        }

        #endregion

        #region Colleges

        [HttpGet("ByUniversity/{id}")]
        public async Task<IActionResult> GetCollegesByUniversity(int universityId)
        {
            if (universityId <= 0)
            {
                return BadRequest("Invalid University ID.");
            }
            var colleges = await _locationService.GetCollegesByUniversityAsync(universityId);
            return Ok(colleges);
        }

        [HttpGet("College/{collegeId}")]
        public async Task<IActionResult> GetCollegeDetails(int collegeId)
        {
            if (collegeId <= 0)
            {
                return BadRequest("Invalid College ID.");
            }
            var collegeDetails = await _locationService.GetCollegeDetailsAsync(collegeId);
            if (collegeDetails == null)
            {
                return NotFound($"College with ID {collegeId} not found.");
            }
            return Ok(collegeDetails);
        }
        #endregion

    }
}
