using CollegeGuideApi.DTOs.AdminDtos;
using CollegeGuideApi.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CollegeGuideApi.Controllers
{
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    [ApiController]
    public class AdminController : ControllerBase
    {
        private readonly IAdminService _adminService;
        private readonly ILogger<AdminController> _logger;
        public AdminController(IAdminService adminService, ILogger<AdminController> logger)
        {
            _adminService = adminService;
            _logger = logger;
        }

        #region University
        [HttpGet("GetAllCities")]
        public async Task<IActionResult> GetCities()
        {
            _logger.LogInformation("Fetching all cities.");
            var cities = await _adminService.GetAllCitiesAsync();
            if (!cities.Any())
            {
                _logger.LogInformation("No cities found.");
                return NotFound("No cities found.");
            }
            return Ok(cities);
        }


        [HttpGet("GetUnibycity/{cityId}")]
        public async Task<IActionResult> GetUniversitiesByCity(int cityId)
        {
            var universities = await _adminService.GetUniversitiesByCityAsync(cityId);
            return Ok(universities);
        }

        [HttpGet("GetUnibyId/{id}")]
        public async Task<IActionResult> GetUniversityById(int id)
        {
            var university = await _adminService.GetUniversityByIdAsync(id);
            if (university == null)
            {
                return NotFound(new { message = $"University with Id {id} not found." });
            }
            return Ok(university);
        }

        [HttpPost("AddUniversity")]
        public async Task<IActionResult> AddUniversity([FromBody] AddUniversityDto universityDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var createdUniversity = await _adminService.AddUniversityAsync(universityDto);
            if (createdUniversity == null)
            {
                return BadRequest(new { message = "City not found or error creating university." });
            }
            // هيرجع بيانات الجامعة اللي اتضافت و status code 201 Created
            return CreatedAtAction(nameof(GetUniversityById), new { id = createdUniversity.Id }, createdUniversity);
        }


        [HttpPut("EditUniversity/{id}")]
        public async Task<IActionResult> UpdateUniversity(int id, [FromBody] UpdateUniversityDto universityDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var success = await _adminService.UpdateUniversityAsync(id, universityDto);
            if (!success)
            {
                return NotFound(new { message = $"University with Id {id} not found or invalid CityId." });
            }
            return Ok(new { message = "University updated successfully." }); // ممكن ترجعي NoContent() أو الـ DTO بعد التعديل
        }
        #endregion

        #region College
        [HttpGet("GetCities")]
        public async Task<IActionResult> GetCitiesForDropdown()
        {
            var cities = await _adminService.GetAllCitiesAsync();
            return Ok(cities);
        }

        [HttpGet("GetUniversities/{cityId}")]
        public async Task<IActionResult> GetUniversitiesForDropdown(int cityId)
        {
            var universities = await _adminService.GetUniversitiesByCityAsync(cityId);
            // ممكن نرجع DTO أبسط هنا لو محتاجين بس الـ Id و الـ Name للجامعة
            var universityDropdowns = universities.Select(u => new { u.Id, u.Name });
            return Ok(universityDropdowns);
        }



        [HttpGet("GetCollegesByUniversityId/{universityId}")]
        public async Task<IActionResult> GetCollegesByUniversity(int universityId)
        {
            var colleges = await _adminService.GetCollegesByUniversityAsync(universityId);
            return Ok(colleges);
        }

        [HttpGet("GetCollegeById/{id}")]
        public async Task<IActionResult> GetCollegeById(int id)
        {
            var college = await _adminService.GetCollegeByIdAsync(id);
            if (college == null)
            {
                return NotFound(new { message = $"College with Id {id} not found." });
            }
            return Ok(college);
        }

        [HttpPost("AddCollege")]
        public async Task<IActionResult> AddCollege([FromBody] AddCollegeDto collegeDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var createdCollege = await _adminService.AddCollegeAsync(collegeDto);
            if (createdCollege == null)
            {
                return BadRequest(new { message = "University not found or error creating college." });
            }
            return CreatedAtAction(nameof(GetCollegeById), new { id = createdCollege.Id }, createdCollege);
        }

        [HttpPut("EditCollege/{id}")]
        public async Task<IActionResult> UpdateCollege(int id, [FromBody] UpdateCollegeDto collegeDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var success = await _adminService.UpdateCollegeAsync(id, collegeDto);
            if (!success)
            {
                return NotFound(new { message = $"College with Id {id} not found or invalid UniversityId." });
            }
            return Ok(new { message = "College updated successfully." });
        }
        #endregion

        #region FinancilaAids
        [HttpGet("FinancialAidOptions")]
        public IActionResult GetFinancialAidEditOptions()
        {
            _logger.LogInformation("Admin requested financial aid edit options.");
            // This doesn't fetch data, just provides conceptual navigation for the frontend.
            // The frontend will then call either /types or /scholarship-categories
            return Ok(new
            {
                message = "Choose what to edit:",
                options = new[] {
                    new { name = "Financial Aid Types", endpoint = "/api/Admin/financial-aid/types" },
                    new { name = "Scholarships", endpoint = "/api/Admin/financial-aid/scholarship-categories" }
                }
            });
        }

        [HttpGet("GetFinancialAidTypes")]
        public async Task<IActionResult> GetAllFinancialAidTypes()
        {
            _logger.LogInformation("Admin fetching all financial aid types.");
            var types = await _adminService.GetAllFinancialAidTypesAsync();
            if (!types.Any())
            {
                _logger.LogInformation("No financial aid types found.");
                return NotFound(new { message = "No financial aid types found." });
            }
            return Ok(types);
        }

        [HttpGet("FinancialAidTypes/{id}")]
        public async Task<IActionResult> GetFinancialAidType(int id)
        {
            _logger.LogInformation("Admin fetching financial aid type with ID: {TypeId}", id);
            var type = await _adminService.GetFinancialAidTypeByIdAsync(id);
            if (type == null)
            {
                _logger.LogWarning("Financial aid type with ID: {TypeId} not found.", id);
                return NotFound(new { message = $"Financial aid type with ID {id} not found." });
            }
            return Ok(type);
        }

        [HttpPut("EditFinancialAidTypes/{id}")]
        public async Task<IActionResult> UpdateFinancialAidType(int id, [FromBody] UpdateFinancialAidTypeRequestDto dto)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid model state for updating financial aid type ID: {TypeId}", id);
                return BadRequest(ModelState);
            }

            _logger.LogInformation("Admin attempting to update financial aid type with ID: {TypeId}", id);
            var success = await _adminService.UpdateFinancialAidTypeAsync(id, dto);
            if (!success)
            {
                _logger.LogWarning("Failed to update financial aid type with ID: {TypeId}. It might not exist.", id);
                return NotFound(new { message = $"Financial aid type with ID {id} not found or update failed." });
            }

            return Ok(new { message = $"Financial aid type with ID {id} updated successfully." });
        }

        [HttpGet("GetScholarshipCategories")]
        public async Task<IActionResult> GetAllScholarshipCategories()
        {
            _logger.LogInformation("Admin fetching all scholarship categories.");
            var categories = await _adminService.GetAllScholarshipCategoriesAsync();
            if (!categories.Any())
            {
                _logger.LogInformation("No scholarship categories found.");
                return NotFound(new { message = "No scholarship categories found." });
            }
            return Ok(categories); // This will include ScholarshipItems
        }

        [HttpGet("ScholarshipCategories/{categoryId}")]
        public async Task<IActionResult> GetScholarshipCategory(int categoryId)
        {
            _logger.LogInformation("Admin fetching scholarship category with ID: {CategoryId}", categoryId);
            var category = await _adminService.GetScholarshipCategoryByIdAsync(categoryId);
            if (category == null)
            {
                _logger.LogWarning("Scholarship category with ID: {CategoryId} not found.", categoryId);
                return NotFound(new { message = $"Scholarship category with ID {categoryId} not found." });
            }
            return Ok(category); // This will include ScholarshipItems
        }

        [HttpPut("EditScholarshipCategories/{categoryId}")]
        public async Task<IActionResult> UpdateScholarshipCategory(int categoryId, [FromBody] UpdateScholarshipCategoryRequestDto dto)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid model state for updating scholarship category ID: {CategoryId}", categoryId);
                return BadRequest(ModelState);
            }
            _logger.LogInformation("Admin attempting to update scholarship category with ID: {CategoryId}", categoryId);
            var success = await _adminService.UpdateScholarshipCategoryAsync(categoryId, dto);
            if (!success)
            {
                _logger.LogWarning("Failed to update scholarship category with ID: {CategoryId}. It might not exist.", categoryId);
                return NotFound(new { message = $"Scholarship category with ID {categoryId} not found or update failed." });
            }
            return Ok(new { message = $"Scholarship category with ID {categoryId} updated successfully." });
        }

        // Endpoint to get a specific item for editing, though often items are edited in the context of their category
        [HttpGet("ScholarshipItems/{itemId}")]
        public async Task<IActionResult> GetScholarshipItem(int itemId)
        {
            _logger.LogInformation("Admin fetching scholarship item with ID: {ItemId}", itemId);
            var item = await _adminService.GetScholarshipItemByIdAsync(itemId);
            if (item == null)
            {
                _logger.LogWarning("Scholarship item with ID: {ItemId} not found.", itemId);
                return NotFound(new { message = $"Scholarship item with ID {itemId} not found." });
            }
            return Ok(item);
        }

        [HttpPut("EditScholarshipItems/{itemId}")]
        public async Task<IActionResult> UpdateScholarshipItem(int itemId, [FromBody] UpdateScholarshipItemRequestDto dto)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid model state for updating scholarship item ID: {ItemId}", itemId);
                return BadRequest(ModelState);
            }
            _logger.LogInformation("Admin attempting to update scholarship item with ID: {ItemId}", itemId);
            var success = await _adminService.UpdateScholarshipItemAsync(itemId, dto);
            if (!success)
            {
                _logger.LogWarning("Failed to update scholarship item with ID: {ItemId}. It might not exist.", itemId);
                return NotFound(new { message = $"Scholarship item with ID {itemId} not found or update failed." });
            }
            return Ok(new { message = $"Scholarship item with ID {itemId} updated successfully." });
        }
        #endregion
    }
}
