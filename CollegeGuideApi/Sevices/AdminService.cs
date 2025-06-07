using AutoMapper;
using CollegeGuideApi.DTOs.AdminDtos;
using CollegeGuideApi.DTOs.FinancialAidDtos;
using CollegeGuideApi.DTOs.SearchDtos;
using CollegeGuideApi.Interfaces;
using CollegeGuideApi.Models.DbContexts;
using CollegeGuideApi.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace CollegeGuideApi.Sevices
{
    public class AdminService : IAdminService
    {

        private readonly ApplicationDbContext _context;
        private readonly ILogger<AdminService> _logger;
        private readonly IMapper _mapper;
        public AdminService(ApplicationDbContext context, ILogger<AdminService> logger, IMapper mapper)
        {
            _context = context;
            _logger = logger;
            _mapper = mapper;
        }

        #region [Add-Edit] Universities

        public async Task<IEnumerable<CityDto>> GetAllCitiesAsync()
        {
            return await _context.Cities
                .Select(c => new CityDto { Id = c.Id, Name = c.Name })
                .ToListAsync();
        }
        public async Task<UniversityDetailsDto?> AddUniversityAsync(AddUniversityDto universityDto)
        {
            var cityExists = await _context.Cities.AnyAsync(c => c.Id == universityDto.CityId);
            if (!cityExists)
            {
                return null;
            }

            var university = new University
            {
                Name = universityDto.Name,
                Description = universityDto.Description,
                PhoneNumber = universityDto.PhoneNumber,
                Logo = universityDto.Logo,
                Website = universityDto.Website,
                Email = universityDto.Email,
                Youtube = universityDto.Youtube,
                Instagram = universityDto.Instagram,
                X = universityDto.X,
                Facebook = universityDto.Facebook,
                CityId = universityDto.CityId
            };

            _context.Universities.Add(university);
            await _context.SaveChangesAsync();

            // نرجع الجامعة اللي اتضافت مع اسم المدينة
            var city = await _context.Cities.FindAsync(university.CityId);
            return new UniversityDetailsDto
            {
                Name = universityDto.Name,
                Description = universityDto.Description,
                PhoneNumber = universityDto.PhoneNumber,
                Logo = universityDto.Logo,
                Website = universityDto.Website,
                Email = universityDto.Email,
                Youtube = universityDto.Youtube,
                Instagram = universityDto.Instagram,
                X = universityDto.X,
                Facebook = universityDto.Facebook
            };
        }
        public async Task<IEnumerable<UniversityDto>> GetUniversitiesByCityAsync(int cityId)
        {
            return await _context.Universities
                .Where(u => u.CityId == cityId)
                .Select(u => new UniversityDto
                {
                    Id = u.Id,
                    Name = u.Name,
                    //CityId = u.CityId,
                    //CityName = u.City != null ? u.City.Name : "N/A" // للـ include بتاع اسم المدينة
                })
                .ToListAsync();
        }

        public async Task<UniversityDetailsDto?> GetUniversityByIdAsync(int universityId)
        {
            var university = await _context.Universities
                                    .Include(u => u.City) // عشان نجيب اسم المدينة معاه
                                    .FirstOrDefaultAsync(u => u.Id == universityId);

            if (university == null) return null;

            return new UniversityDetailsDto
            {
                Name = university.Name,
                Description = university.Description,
                PhoneNumber = university.PhoneNumber,
                Logo = university.Logo,
                Website = university.Website,
                Email = university.Email,
                Youtube = university.Youtube,
                Instagram = university.Instagram,
                X = university.X,
                Facebook = university.Facebook
                //CityId = university.CityId,
                //CityName = university.City?.Name ?? "N/A"
            };
        }
        public async Task<bool> UpdateUniversityAsync(int universityId, UpdateUniversityDto universityDto)
        {
            var university = await _context.Universities.FindAsync(universityId);
            if (university == null) return false;

            var cityExists = await _context.Cities.AnyAsync(c => c.Id == universityDto.CityId);
            if (!cityExists) return false; // المدينة الجديدة مش موجودة

            university.Name = universityDto.Name;
            university.CityId = universityDto.CityId;

            _context.Universities.Update(university);
            await _context.SaveChangesAsync();
            return true;
        }
        #endregion

        #region [Add-Edit] Colleges
        public async Task<CollegeDetailsDto?> AddCollegeAsync(AddCollegeDto collegeDto)
        {
            var universityExists = await _context.Universities.AnyAsync(u => u.Id == collegeDto.UniversityId);
            if (!universityExists)
            {
                return null;
            }

            var college = new College
            {
                Name = collegeDto.Name,
                Description = collegeDto.Description,
                DurationOfStudy = collegeDto.DurationOfStudy,
                CreditHours = collegeDto.CreditHours,
                PreTest = collegeDto.PreTest,
                AppFeesEgy = collegeDto.AppFeesEgy,
                FeesEgy = collegeDto.FeesEgy,
                AppFeesInt = collegeDto.AppFeesInt,
                FeesInt = collegeDto.FeesInt,
                FirInstalment = collegeDto.FirInstalment,
                SecInstalment = collegeDto.SecInstalment,
                PaymentMethod = collegeDto.PaymentMethod,
                UniversityId = collegeDto.UniversityId
                //Name = collegeDto.Name,
                //Description = collegeDto.Description,

            };

            _context.Colleges.Add(college);
            await _context.SaveChangesAsync();

            var university = await _context.Universities.FindAsync(college.UniversityId);
            return new CollegeDetailsDto
            {
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
                //Description = college.Description,
                //UniversityId = college.UniversityId,
                //UniversityName = university?.Name ?? "N/A"
            };
        }

        public async Task<IEnumerable<CollegeDto>> GetCollegesByUniversityAsync(int universityId)
        {
            return await _context.Colleges
                .Where(c => c.UniversityId == universityId)
                .Include(c => c.University) // عشان نجيب اسم الجامعة
                .Select(c => new CollegeDto
                {
                    Name = c.Name,
                    //Description = c.Description,
                    //UniversityId = c.UniversityId,
                    //UniversityName = c.University != null ? c.University.Name : "N/A"
                })
                .ToListAsync();
        }

        public async Task<CollegeDetailsDto?> GetCollegeByIdAsync(int collegeId)
        {
            var college = await _context.Colleges
                                    .Include(c => c.University) // عشان نجيب اسم الجامعة
                                    .FirstOrDefaultAsync(c => c.Id == collegeId);

            if (college == null) return null;

            return new CollegeDetailsDto
            {
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
                //UniversityId = college.UniversityId,
                //UniversityName = college.University?.Name ?? "N/A"
            };
        }

        public async Task<bool> UpdateCollegeAsync(int collegeId, UpdateCollegeDto collegeDto)
        {
            var college = await _context.Colleges.FindAsync(collegeId);
            if (college == null) return false;

            var universityExists = await _context.Universities.AnyAsync(u => u.Id == collegeDto.UniversityId);
            if (!universityExists) return false; // الجامعة الجديدة مش موجودة

            college.Name = collegeDto.Name;
            college.Description = collegeDto.Description;
            college.DurationOfStudy = collegeDto.DurationOfStudy;
            college.CreditHours = collegeDto.CreditHours;
            college.PreTest = collegeDto.PreTest;
            college.AppFeesEgy = collegeDto.AppFeesEgy;
            college.FeesEgy = collegeDto.FeesEgy;
            college.AppFeesInt = collegeDto.AppFeesInt;
            college.FeesInt = collegeDto.FeesInt;
            college.FirInstalment = collegeDto.FirInstalment;
            college.SecInstalment = collegeDto.SecInstalment;
            college.PaymentMethod = collegeDto.PaymentMethod;
            //college.UniversityId = collegeDto.UniversityId;

            _context.Colleges.Update(college);
            await _context.SaveChangesAsync();
            return true;
        }

        #endregion

        #region Edit FinancilaAids
        public async Task<IEnumerable<FinancialAidTypeDto>> GetAllFinancialAidTypesAsync()
        {
            _logger.LogInformation("Fetching all financial aid types.");
            var types = await _context.FinancialAidTypes.ToListAsync();
            return _mapper.Map<IEnumerable<FinancialAidTypeDto>>(types);
        }

        public async Task<FinancialAidTypeDto?> GetFinancialAidTypeByIdAsync(int id)
        {
            _logger.LogInformation("Fetching financial aid type with ID: {Id}", id);
            var type = await _context.FinancialAidTypes.FindAsync(id);
            if (type == null)
            {
                _logger.LogWarning("Financial aid type with ID: {Id} not found.", id);
                return null;
            }
            return _mapper.Map<FinancialAidTypeDto>(type);
        }

        public async Task<bool> UpdateFinancialAidTypeAsync(int id, UpdateFinancialAidTypeRequestDto dto)
        {
            _logger.LogInformation("Attempting to update financial aid type with ID: {Id}", id);
            var financialAidType = await _context.FinancialAidTypes.FindAsync(id);
            if (financialAidType == null)
            {
                _logger.LogWarning("Update failed: Financial aid type with ID: {Id} not found.", id);
                return false;
            }

            //_mapper.Map(dto, financialAidType); // Or map manually
            financialAidType.Name = dto.Name;
            financialAidType.Content = dto.Content;

            _context.FinancialAidTypes.Update(financialAidType);
            try
            {
                await _context.SaveChangesAsync();
                _logger.LogInformation("Financial aid type with ID: {Id} updated successfully.", id);
                return true;
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogError(ex, "Concurrency error while updating financial aid type with ID: {Id}.", id);
                // Handle concurrency if needed, e.g., by checking if the entity still exists
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating financial aid type with ID: {Id}.", id);
                return false;
            }
        }

        public async Task<IEnumerable<ScholarshipCategoryDto>> GetAllScholarshipCategoriesAsync()
        {
            _logger.LogInformation("Fetching all scholarship categories with their items.");
            var categories = await _context.ScholarshipCategories
                                        .Include(sc => sc.Items)
                                        .ToListAsync();
            return _mapper.Map<IEnumerable<ScholarshipCategoryDto>>(categories);
        }

        public async Task<ScholarshipCategoryDto?> GetScholarshipCategoryByIdAsync(int categoryId)
        {
            _logger.LogInformation("Fetching scholarship category with ID: {CategoryId}", categoryId);
            var category = await _context.ScholarshipCategories
                                        .Include(sc => sc.Items)
                                        .FirstOrDefaultAsync(sc => sc.Id == categoryId);
            if (category == null)
            {
                _logger.LogWarning("Scholarship category with ID: {CategoryId} not found.", categoryId);
                return null;
            }
            return _mapper.Map<ScholarshipCategoryDto>(category);
        }

        public async Task<bool> UpdateScholarshipCategoryAsync(int categoryId, UpdateScholarshipCategoryRequestDto dto)
        {
            _logger.LogInformation("Attempting to update scholarship category with ID: {CategoryId}", categoryId);
            var category = await _context.ScholarshipCategories.FindAsync(categoryId);
            if (category == null)
            {
                _logger.LogWarning("Update failed: Scholarship category with ID: {CategoryId} not found.", categoryId);
                return false;
            }

            category.Name = dto.Name;
            // _mapper.Map(dto, category); // If you prefer AutoMapper

            _context.ScholarshipCategories.Update(category);
            try
            {
                await _context.SaveChangesAsync();
                _logger.LogInformation("Scholarship category with ID: {CategoryId} updated successfully.", categoryId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating scholarship category with ID: {CategoryId}.", categoryId);
                return false;
            }
        }

        public async Task<ScholarshipItemDto?> GetScholarshipItemByIdAsync(int itemId)
        {
            _logger.LogInformation("Fetching scholarship item with ID: {ItemId}", itemId);
            var item = await _context.ScholarshipItems
                                    .Include(si => si.ScholarshipCategory) // Optional: if you need category info with the item
                                    .FirstOrDefaultAsync(si => si.Id == itemId);
            if (item == null)
            {
                _logger.LogWarning("Scholarship item with ID: {ItemId} not found.", itemId);
                return null;
            }
            return _mapper.Map<ScholarshipItemDto>(item);
        }

        public async Task<bool> UpdateScholarshipItemAsync(int itemId, UpdateScholarshipItemRequestDto dto)
        {
            _logger.LogInformation("Attempting to update scholarship item with ID: {ItemId}", itemId);
            var item = await _context.ScholarshipItems.FindAsync(itemId);
            if (item == null)
            {
                _logger.LogWarning("Update failed: Scholarship item with ID: {ItemId} not found.", itemId);
                return false;
            }

            item.Name = dto.Name;
            item.Details = dto.Details;
            // _mapper.Map(dto, item); // If you prefer AutoMapper

            _context.ScholarshipItems.Update(item);
            try
            {
                await _context.SaveChangesAsync();
                _logger.LogInformation("Scholarship item with ID: {ItemId} updated successfully.", itemId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating scholarship item with ID: {ItemId}.", itemId);
                return false;
            }
        }
        #endregion

    }
}
