namespace CollegeGuideApi.Models.Entities
{
    public class EmailVerification
    {
        public int Id { get; set; }
        public string Email { get; set; }
        public string Otp { get; set; }
        public bool IsUsed { get; set; }
        public string? Purpose { get; set; }
        public DateTime ExpirationTime { get; set; }
        public int UserId { get;  set; }
    }
}
