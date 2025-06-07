namespace CollegeGuideApi.Models.Entities
{
    public class RevokedToken
    {
        public int Id { get; set; }
        public string Jti { get; set; }
        public DateTime ExpirationDate { get; set; }
    }
}
