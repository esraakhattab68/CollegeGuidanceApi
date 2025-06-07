namespace CollegeGuideApi.Models.Entities
{
    public class ExternalLogin
    {
        public int Id { get; set; }
        public string Provider { get; set; } 
        public string ProviderUserId { get; set; } 

        // Relationship with ApplicationUser
        public int UserId { get; set; } 
        public ApplicationUser User { get; set; }
    }
}
