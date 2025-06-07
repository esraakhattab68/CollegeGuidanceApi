using AutoMapper;
using CollegeGuideApi.DTOs.FinancialAidDtos;
using CollegeGuideApi.DTOs.SearchDtos;
using CollegeGuideApi.Models.Entities;

namespace CollegeGuideApi.Profiles
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            #region Serach
            CreateMap<City, CityDto>().ReverseMap();
            CreateMap<University, UniversityDto>().ReverseMap();
            CreateMap<College, CollegeDto>().ReverseMap();
            CreateMap<Department, DepartmentDto>().ReverseMap();
            CreateMap<FutureJob, FutureJobDto>().ReverseMap();
            #endregion

            #region FinancialAid
            CreateMap<FinancialAidType, FinancialAidTypeDto>().ReverseMap();
            CreateMap<ScholarshipCategory, ScholarshipCategoryDto>().ReverseMap();
            CreateMap<ScholarshipItem, ScholarshipItemDto>().ReverseMap();
            #endregion
        }
    }
}
