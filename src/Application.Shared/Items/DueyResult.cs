namespace Application.Shared.Items
{
    public enum SendDueyItemResponseCode
    {
        Success = 0,
        // You cannot use Duey to send items at your GM level.
        GMLevelCheck = 1,
        SendMessageCheck,
        QuickCheck,
        CostCheck,
        MesoCheck,
        CharacterNotExisted,
        SameAccount,
        CreateFailed,

    }

    public record SendDueyItemResponse(SendDueyItemResponseCode Code, int PackageId = -1, int Cost = 0);
}
