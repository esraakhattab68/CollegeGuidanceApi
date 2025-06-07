namespace CollegeGuideApi.DTOs.Authentication.Responses
{
    public class UserProfileResponseDTO
    {
        public int Id { get; set; }
        public string FName { get; set; }
        public string LName { get; set; }
        public string Email { get; set; }
        public bool EmailConfirmed { get; set; }
        public string PhoneNumber { get; set; }
        public bool PhoneNumberConfirmed { get; set; } 
        public string UserType { get; set; }
    }
}
