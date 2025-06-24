namespace Application.Shared.NewYear
{
    public enum NewYearCardResponseCode
    {
        Success,

        Receive_SameSenderReceiver,
        Receive_AlreadReceived,
        Receive_AlreadyDiscard
    }
}
