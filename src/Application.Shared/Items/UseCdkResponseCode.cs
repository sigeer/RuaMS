namespace Application.Shared.Items
{
    public enum UseCdkResponseCode
    {
        Success = 0,
        NotFound = 0xB0,
        Used = 0xB3,
        Expired = 0xB2,
        FetalError = 0xBB,
        TooManyError = 0xB1
    }
}
