namespace EcoRoute.Models.Entities
{
    public class EmailOtp
    {
        public int Id { get; set; }
        public string Email { get; set; }
        public string OtpHash { get; set; }
        public DateTime ExpiryTime { get; set; }
        public bool IsVerified { get; set; }
    }
}
