namespace Application.Host.Models
{
    public class WebUserInfo
    {
        public int UserId { get; set; }
        public string RealName { get; set; }
        public string[] Roles { get; set; }
    }
}
