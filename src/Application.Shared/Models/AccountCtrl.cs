namespace Application.Shared.Models
{
    public class AccountCtrl
    {
        public int Id { get; set; }

        public string Name { get; set; } = null!;
        public string Password { get; set; } = null!;

        public string Pin { get; set; } = null!;

        public string Pic { get; set; } = null!;
        public sbyte Characterslots { get; set; } = 3;

        public DateTime Birthday { get; set; }

        public sbyte Gender { get; set; }

        public bool Tos { get; set; }

        public sbyte GMLevel { get; set; }

        public bool CanFly { get; set; }

        public string CurrentIP { get; set; } = null!;
        public string CurrentMac { get; set; } = null!;
        public string CurrentHwid { get; set; } = null!;
        public string GetSessionRemoteHost()
        {
            return $"{CurrentIP}-{CurrentHwid}";
        }

    }

    public static class AccountExtensions
    {
        public static bool IsGmAccount(this AccountCtrl acc) => acc.GMLevel > 1;
    }
}
