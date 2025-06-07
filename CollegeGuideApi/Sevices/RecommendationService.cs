using CollegeGuideApi.DTOs.RecommendationDtos;
using CollegeGuideApi.Interfaces;
using CollegeGuideApi.Models.DbContexts;
using Microsoft.EntityFrameworkCore;

namespace CollegeGuideApi.Sevices
{
    public class RecommendationService : IRecommendationService
    {
        private readonly ApplicationDbContext _context;

        public RecommendationService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<CollegeResponseDto>> GetAllRecommendedCollegesAsync(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                return new List<CollegeResponseDto>();

            query = query.ToLower();

            var colleges = await _context.Colleges
                .Include(c => c.University)
                .Where(c =>
                    c.Name.ToLower().Contains(query) ||
                    c.Description.ToLower().Contains(query) ||
                    c.University.Name.ToLower().Contains(query) ||
                    c.University.Description.ToLower().Contains(query)
                )
                .ToListAsync();

            return colleges.Select(c => new CollegeResponseDto
            {
                Id = c.Id,
                Name = c.Name,
                Description = c.Description,
                DurationOfStudy = c.DurationOfStudy,
                CreditHours = c.CreditHours,
                University = new UniversityResponseDto
                {
                    Id = c.University.Id,
                    Name = c.University.Name,
                    Description = c.University.Description,
                    Website = c.University.Website,
                    Logo = c.University.Logo
                }
            });
        }
    }
}
