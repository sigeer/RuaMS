using Application.Shared.Constants.Item;

namespace Application.Module.Marriage.Common.Models
{
    public class EngageItem
    {
        public static int GetEngagementBoxId(int useItemId)
        {
            return useItemId switch
            {
                ItemId.ENGAGEMENT_BOX_MOONSTONE => ItemId.EMPTY_ENGAGEMENT_BOX_MOONSTONE,
                ItemId.ENGAGEMENT_BOX_STAR => ItemId.EMPTY_ENGAGEMENT_BOX_STAR,
                ItemId.ENGAGEMENT_BOX_GOLDEN => ItemId.EMPTY_ENGAGEMENT_BOX_GOLDEN,
                ItemId.ENGAGEMENT_BOX_SILVER => ItemId.EMPTY_ENGAGEMENT_BOX_SILVER,
                _ => ItemId.CARAT_RING_BASE + (useItemId - ItemId.CARAT_RING_BOX_BASE),
            };
        }
    }
}
