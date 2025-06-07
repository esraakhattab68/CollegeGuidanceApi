using AutoMapper;
using CollegeGuideApi.DTOs.SearchDtos;
using CollegeGuideApi.Interfaces;
using CollegeGuideApi.Models.DbContexts;
using Microsoft.EntityFrameworkCore;

namespace CollegeGuideApi.Sevices
{
    public class SearchService : ISearchService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public SearchService(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        #region City
        public async Task<IEnumerable<CityDto>> GetAllCitiesAsync()
        {
            return await _context.Cities
                .AsNoTracking()
                .Select(c => new CityDto { Id = c.Id, Name = c.Name })
                .OrderBy(c => c.Name)
                .ToListAsync();
        }
        #endregion

        #region University
        public async Task<IEnumerable<UniversityDto>> GetUniversitiesByCityAsync(int cityId)
        {
            return await _context.Universities
                .AsNoTracking()
                .Where(u => u.CityId == cityId)
                .Select(u => new UniversityDto { Id = u.Id, Name = u.Name })
                .OrderBy(u => u.Name)
                .ToListAsync();
        }

        public async Task<UniversityDetailsDto?> GetUniversityDetailsAsync(int universityId)
        {
            var university = await _context.Universities
                .AsNoTracking()
                .Include(u => u.Colleges)
                .FirstOrDefaultAsync(u => u.Id == universityId);

            if (university == null)
            {
                return null;
            }

            return new UniversityDetailsDto
            {
                Id = university.Id,
                Name = university.Name,
                Description = university.Description,
                PhoneNumber = university.PhoneNumber,
                Logo = university.Logo,
                Website = university.Website,
                Email = university.Email,
                Youtube = university.Youtube,
                X = university.X,
                Facebook = university.Facebook,
                Colleges = university.Colleges.Select(c => new CollegeDto { Id = c.Id, Name = c.Name }).OrderBy(c => c.Name).ToList()
            };
        }


        #endregion

        #region College
        public async Task<IEnumerable<CollegeDto>> GetCollegesByUniversityAsync(int universityId)
        {
            return await _context.Colleges
                .AsNoTracking()
                .Where(c => c.UniversityId == universityId)
                .Select(c => new CollegeDto { Id = c.Id, Name = c.Name })
                .OrderBy(c => c.Name)
                .ToListAsync();
        }

        public async Task<CollegeDetailsDto?> GetCollegeDetailsAsync(int collegeId)
        {
            var college = await _context.Colleges
                .AsNoTracking()
                .Include(c => c.Departments)
                    .ThenInclude(d => d.FutureJobs)
                .FirstOrDefaultAsync(c => c.Id == collegeId);

            if (college == null)
            {
                return null;
            }

            return new CollegeDetailsDto
            {
                Id = college.Id,
                Name = college.Name,
                Description = college.Description,
                DurationOfStudy = college.DurationOfStudy,
                CreditHours = college.CreditHours,
                PreTest = college.PreTest,
                AppFeesEgy = college.AppFeesEgy,
                FeesEgy = college.FeesEgy,
                AppFeesInt = college.AppFeesInt,
                FeesInt = college.FeesInt,
                FirInstalment = college.FirInstalment,
                SecInstalment = college.SecInstalment,
                PaymentMethod = college.PaymentMethod,
                Departments = college.Departments.Select(d => new DepartmentDto
                {
                    Id = d.Id,
                    Name = d.Name,
                    FutureJobs = d.FutureJobs.Select(fj => new FutureJobDto { Id = fj.Id, Title = fj.Title }).OrderBy(fj => fj.Title).ToList()
                }).OrderBy(d => d.Name).ToList()
            };
        }
        #endregion

        #region Department
        public async Task<IEnumerable<DepartmentDto>> GetByCollegeIdAsync(int collegeId)
        {
            var departments = await _context.Departments
                .Where(d => d.CollegeId == collegeId)
                .Include(d => d.FutureJobs)
                .ToListAsync();
            return _mapper.Map<List<DepartmentDto>>(departments);
        }
        #endregion

        #region FutureJob
        public async Task<IEnumerable<FutureJobDto>> GetByDepartmentIdAsync(int departmentId)
        {
            var jobs = await _context.FutureJobs
                .Where(j => j.DepartmentId == departmentId)
                .ToListAsync();
            return _mapper.Map<List<FutureJobDto>>(jobs);
        }
        #endregion
    }
}
