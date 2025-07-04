namespace Application.Module.BBS.Master.Models
{
    public class BBSThreadModel
    {
        public int Postercid { get; set; }

        public string Name { get; set; } = null!;

        public long Timestamp { get; set; }

        public short Icon { get; set; }

        public List<BBSReplyModel> Replies { get; set; } = [];

        public string Startpost { get; set; } = null!;

        public int Guildid { get; set; }

        public int ThreadId { get; set; }
    }
}
