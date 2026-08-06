using Application.Core.Login.Dtos.Character;

namespace Application.Core.Login.Dtos.Report
{
    public class ReportResponseDto
    {
        public int Id { get; set; }

        public DateTimeOffset ReportTime { get; set; }

        public int ReporterId { get; set; }
        public CharacterResponseDto? Reporter { get; set; }

        public int VictimId { get; set; }
        public CharacterResponseDto? Victim { get; set; }

        public sbyte Reason { get; set; }

        public string ChatMessage { get; set; } = null!;

        public string Description { get; set; } = null!;
        public bool Processed { get; set; }
    }
}
