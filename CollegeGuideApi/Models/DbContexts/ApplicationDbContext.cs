using CollegeGuideApi.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace CollegeGuideApi.Models.DbContexts
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        #region DbSets
        public DbSet<ApplicationUser> Users { get; set; }
        public DbSet<ExternalLogin> ExternalLogins { get; set; }
        public DbSet<EmailVerification> EmailVerifications { get; set; }
        public DbSet<MailSetting> MailSettings { get; set; }
        public DbSet<City> Cities { get; set; }
        public DbSet<University> Universities { get; set; }
        public DbSet<College> Colleges { get; set; }
        public DbSet<Department> Departments { get; set; }
        public DbSet<FutureJob> FutureJobs { get; set; }
        public DbSet<FinancialAidType> FinancialAidTypes { get; set; }
        public DbSet<ScholarshipCategory> ScholarshipCategories { get; set; }
        public DbSet<ScholarshipItem> ScholarshipItems { get; set; }
        public DbSet<RevokedToken> RevokedTokens { get; set; }
        public DbSet<SavedColleges> SavedColleges { get; set; }
        #endregion

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            #region Admin Seeding
            string adminEmailValue = "esraakhattab068@gmail.com";
            string adminUserNameValue = "admin_esraa_khattab"; 
            string adminPasswordPlain = "ES@Khattab682003";

            
            string actualPasswordHash = BCrypt.Net.BCrypt.HashPassword(adminPasswordPlain);

            int adminRecordId = 1; 

            modelBuilder.Entity<Admin>().HasData(
                new Admin
                {
                    Id = adminRecordId,

                    FName = "Esraa",                        
                    LName = "Khattab",                      
                    Email = adminEmailValue,                
                    PasswordHash = actualPasswordHash,
                    ConfirmPassword = adminPasswordPlain,
                    UserName = adminUserNameValue,
                    UserType = "Admin",
                    NormalizedUserName = adminUserNameValue.ToUpperInvariant(), 
                    NormalizedEmail = adminEmailValue.ToUpperInvariant(),    
                    EmailConfirmed = true,                  
                    SecurityStamp = Guid.NewGuid().ToString(), 
                });
            #endregion
        }
    }
}
