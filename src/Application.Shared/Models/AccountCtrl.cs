namespace Application.Shared.Models
{
    public class AccountCtrl: AccountInfoModel
    {
        public string Password { get; set; } = "";

        public string Pin { get; set; } = "";

        public string Pic { get; set; } = "";



        public bool Tos { get; set; }

        public string CurrentIP { get; set; } = "";
        public string CurrentMac { get; set; } = "";

        public string GetSessionRemoteHost()
        {
            return $"{CurrentIP}-{CurrentHwid}";
        }

    }

    public static class AccountExtensions
    {
        public static bool IsGmAccount(this AccountInfoModel acc) => acc.GMLevel > 1;
    }
}
