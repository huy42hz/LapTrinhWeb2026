namespace SV22T1020158.Models.Partner
{
    public class Customer
    {
        public int CustomerID { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string ContactName { get; set; } = string.Empty;
        public string? Province { get; set; }
        public string? Address { get; set; }
        public string? Phone { get; set; }
        public string Email { get; set; } = string.Empty;
        public string? Password { get; set; }  // Đã có
        public bool IsLocked { get; set; }     // Đã có
    }
}