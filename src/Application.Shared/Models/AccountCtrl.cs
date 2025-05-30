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

        public sbyte Banned { get; set; }

        public string? Banreason { get; set; }

        public string? Macs { get; set; }

        public sbyte Gender { get; set; }

        public DateTimeOffset? Tempban { get; set; }

        public sbyte Greason { get; set; }

        public bool Tos { get; set; }

        public sbyte GMLevel { get; set; }

        public string? Ip { get; set; }

        public string Hwid { get; set; } = null!;

        public int Language { get; set; } = 2;
    }
}
