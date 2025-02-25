namespace Bug_Tracking_System.Models
{
    public class LoginModel
    {
        public string? EmailOrUsername { get; set; }

        public string? UserName { get; set; }

        public string? Password { get; set; }

        public bool RememberMe { get; set; }
    }
}
