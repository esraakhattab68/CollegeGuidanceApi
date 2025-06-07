namespace CollegeGuideApi.Models.Entities
{
    public class MailSetting
    {
        public int Id { get; set; }
        public string CompanyMail { get; set; } = string.Empty; 
        public string Mail { get; set; } = string.Empty;        
        public string DisplayName { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Host { get; set; } = string.Empty;
        public int Port { get; set; }
    }
}
