using CollegeGuideApi.Models.DbContexts;
using CollegeGuideApi.Models.Entities;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace CollegeGuideApi.Data.SeedingData
{
    public class DataSeeder
    {
        public static async Task SeedAsync(ApplicationDbContext context, IWebHostEnvironment env)
        {
            await context.Database.MigrateAsync();


            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            string baseDir = env.ContentRootPath;

            #region Search

            // Seed Cities
            if (!await context.Cities.AnyAsync())
            {
                var citiesJson = await File.ReadAllTextAsync(Path.Combine(baseDir, "Data", "Cities.json"));
                var cities = System.Text.Json.JsonSerializer.Deserialize<List<City>>(citiesJson, options);
                if (cities != null)
                {
                    await context.Cities.AddRangeAsync(cities);
                    await context.SaveChangesAsync();
                }
            }

            // Seed Universities
            if (!await context.Universities.AnyAsync())
            {
                var universitiesJson = await File.ReadAllTextAsync(Path.Combine(baseDir, "Data", "Universities.json"));
                var universities = System.Text.Json.JsonSerializer.Deserialize<List<University>>(universitiesJson, options);
                if (universities != null)
                {
                    await context.Universities.AddRangeAsync(universities);
                    await context.SaveChangesAsync();
                }
            }

            // Seed Colleges
            if (!await context.Colleges.AnyAsync())
            {
                var collegesJson = await File.ReadAllTextAsync(Path.Combine(baseDir, "Data", "Colleges.json"));
                var colleges = System.Text.Json.JsonSerializer.Deserialize<List<College>>(collegesJson, options);
                if (colleges != null)
                {
                    await context.Colleges.AddRangeAsync(colleges);
                    await context.SaveChangesAsync();
                }
            }

            // Seed Departments
            if (!await context.Departments.AnyAsync())
            {
                var departmentsJson = await File.ReadAllTextAsync(Path.Combine(baseDir, "Data", "Departments.json"));
                var departments = System.Text.Json.JsonSerializer.Deserialize<List<Department>>(departmentsJson, options);
                if (departments != null)
                {
                    await context.Departments.AddRangeAsync(departments);
                    await context.SaveChangesAsync();
                }
            }

            // Seed FutureJobs
            if (!await context.FutureJobs.AnyAsync())
            {
                var futureJobsJson = await File.ReadAllTextAsync(Path.Combine(baseDir, "Data", "futurejobs.json"));
                var futureJobs = System.Text.Json.JsonSerializer.Deserialize<List<FutureJob>>(futureJobsJson, options);
                if (futureJobs != null)
                {
                    await context.FutureJobs.AddRangeAsync(futureJobs);
                    await context.SaveChangesAsync();
                }
            }
            #endregion

            #region FinancialAid

            // Seed Financial Aid Types
            if (!await context.FinancialAidTypes.AnyAsync())
            {
                var aidTypesPath = Path.Combine(baseDir, "Data", "FinancialAidTypes.json");
                if (File.Exists(aidTypesPath))
                {
                    var aidTypesJson = await File.ReadAllTextAsync(aidTypesPath);
                    var aidTypes = System.Text.Json.JsonSerializer.Deserialize<List<FinancialAidType>>(aidTypesJson, options);
                    if (aidTypes != null && aidTypes.Any())
                    {
                        await context.FinancialAidTypes.AddRangeAsync(aidTypes);
                        await context.SaveChangesAsync();
                    }
                }
            }

            // Seed Scholarship Categories (and their items)
            if (!await context.ScholarshipCategories.AnyAsync())
            {
                var scholarshipCategoriesPath = Path.Combine(baseDir, "Data", "AvailableScholarships.json");
                if (File.Exists(scholarshipCategoriesPath))
                {
                    var scholarshipCategoriesJson = await File.ReadAllTextAsync(scholarshipCategoriesPath);
                    // The JSON directly maps to ScholarshipCategory which includes ScholarshipItem
                    var scholarshipCategories = System.Text.Json.JsonSerializer.Deserialize<List<ScholarshipCategory>>(scholarshipCategoriesJson, options);
                    if (scholarshipCategories != null && scholarshipCategories.Any())
                    {
                        // EF Core will handle inserting the related ScholarshipItem entities
                        // when ScholarshipCategory entities are added, due to the navigation property.
                        await context.ScholarshipCategories.AddRangeAsync(scholarshipCategories);
                        await context.SaveChangesAsync();
                    }
                }
            }

            #endregion
        }
    }
}
