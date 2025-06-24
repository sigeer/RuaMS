namespace Application.EF;

public partial class NewYearCardEntity
{
    public int Id { get; set; }

    public int SenderId { get; set; }

    public int ReceiverId { get; set; }

    public string Message { get; set; } = null!;

    public bool SenderDiscard { get; set; }

    public bool ReceiverDiscard { get; set; }

    public bool Received { get; set; }

    public long TimeSent { get; set; }

    public long TimeReceived { get; set; }
}
