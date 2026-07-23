namespace Application.Shared.Models
{
    public class AccountInfoModel
    {
        public int Id { get; set; }

        public string Name { get; set; } = "";
        public DateTime Birthday { get; set; }
        public sbyte Gender { get; set; }
        public sbyte GMLevel { get; set; }
        public bool GmMode { get; set; }
        public int Language { get; set; }
        public string CurrentHwid { get; set; } = "";
        public sbyte Characterslots { get; set; } = 3;
    }
}
