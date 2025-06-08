using CollegeGuideApi.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CollegeGuideApi.Controllers
{
    [Route("api/[controller]")]
    //[Authorize]
    [ApiController]
    public class FinancialAidController : ControllerBase
    {
        private readonly IFinancialAidService _financialAidService;

        public FinancialAidController(IFinancialAidService financialAidService)
        {
            _financialAidService = financialAidService;
        }

        [HttpGet("types")]
        public async Task<IActionResult> GetFinancialAidTypes()
        {
            var aidTypes = await _financialAidService.GetFinancialAidTypesAsync();
            return Ok(aidTypes);
        }

        [HttpGet("scholarships")]
        public async Task<IActionResult> GetScholarshipCategories()
        {
            var scholarshipCategories = await _financialAidService.GetScholarshipCategoriesAsync();
            return Ok(scholarshipCategories);
        }
    }
}
