using CollegeGuideApi.DTOs.FinancialAidDtos;
using CollegeGuideApi.Interfaces;
using CollegeGuideApi.Models.DbContexts;
using Microsoft.EntityFrameworkCore;

namespace CollegeGuideApi.Sevices
{
    public class FinancialAidService : IFinancialAidService
    {
        private readonly ApplicationDbContext _context;

        public FinancialAidService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<FinancialAidTypeDto>> GetFinancialAidTypesAsync()
        {
            return await _context.FinancialAidTypes
                .OrderBy(fat => fat.Id) // Or by name, or a specific order column if you add one
                .Select(fat => new FinancialAidTypeDto
                {
                    Id = fat.Id,
                    Name = fat.Name,
                    Content = fat.Content
                }).ToListAsync();
        }

        public async Task<IEnumerable<ScholarshipCategoryDto>> GetScholarshipCategoriesAsync()
        {
            return await _context.ScholarshipCategories
                .Include(sc => sc.Items) // Eager load the scholarship items
                .OrderBy(sc => sc.Id) // Or by name
                .Select(sc => new ScholarshipCategoryDto
                {
                    Id = sc.Id,
                    Name = sc.Name,
                    Items = sc.Items.Select(item => new ScholarshipItemDto
                    {
                        Id = item.Id,
                        Name = item.Name,
                        Details = item.Details
                    }).ToList()
                }).ToListAsync();
        }
    }
}
