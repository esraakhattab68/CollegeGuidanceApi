using CollegeGuideApi.DTOs.DashboardDtos;
using CollegeGuideApi.Interfaces;
using CollegeGuideApi.Models.DbContexts;
using CollegeGuideApi.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace CollegeGuideApi.Sevices
{
    public class DashboardService : IDashboardService
    {
        private readonly ApplicationDbContext _context;

        public DashboardService(ApplicationDbContext context)
        {
            _context = context;

        }

        public async Task<SavedCollegeResponseDto?> AddSavedCollegeAsync(AddSavedCollegesDto savedcollegesDto)
        {
            // Check if already saved
            var exists = await _context.SavedColleges
                .AnyAsync(sc => sc.CollegeId == savedcollegesDto.CollegeId && sc.StudentId == savedcollegesDto.StudentId);

            if (exists)
                return null;

            var savedColleges = new SavedColleges
            {
                CollegeId = savedcollegesDto.CollegeId,
                StudentId = savedcollegesDto.StudentId,
            };

            await _context.SavedColleges.AddAsync(savedColleges);
            await _context.SaveChangesAsync();

            // Get full college data
            var college = await _context.Colleges.FirstOrDefaultAsync(c => c.Id == savedcollegesDto.CollegeId);

            if (college == null)
                return null;

            return new SavedCollegeResponseDto
            {
                Message = "done",
                College = college
            };
        }

        public async Task<List<SavedCollegeWithFullDataDto>> GetSavedCollegesWithFullCollegeDataAsync(int studentId)
        {
            var savedColleges = await _context.SavedColleges
                .Include(sc => sc.College) // Include all College properties
                .Where(sc => sc.StudentId == studentId)
                .ToListAsync();

            return savedColleges
                .Where(sc => sc.College != null)
                .Select(sc => new SavedCollegeWithFullDataDto
                {
                    SavedCollegeId = sc.Id,
                    College = sc.College!
                })
                .ToList();
        }
    }
}
