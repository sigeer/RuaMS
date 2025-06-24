namespace Application.Shared.NewYear
{
    public class NewYearCardModel
    {
        public int Id { get; set; }

        public int SenderId { get; set; }

        public string SenderName { get; set; } = null!;

        public int ReceiverId { get; set; }

        public string ReceiverName { get; set; } = null!;

        public string Message { get; set; } = null!;

        public bool SenderDiscard { get; set; }

        public bool ReceiverDiscard { get; set; }

        public bool Received { get; set; }

        public long TimeSent { get; set; }

        public long TimeReceived { get; set; }
    }
}
