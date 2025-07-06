using Application.Core.Channel.DataProviders;

namespace Application.Core.tools
{
    public class CharacterViewDtoUtils
    {
        public static string GetPlayerNameWithMedal(Dto.PlayerViewDto data)
        {
            var displayName = data.Character.Name;
            var medalItem = data.InventoryItems.FirstOrDefault(x => x.InventoryType == (int)InventoryType.EQUIPPED && x.Position == EquipSlot.Medal);
            if (medalItem != null)
            {
                var medalName = ItemInformationProvider.getInstance().getName(medalItem.InventoryItemId);
                if (!string.IsNullOrWhiteSpace(medalName))
                    displayName = $"<{medalName}>{displayName}";
            }
            return displayName;
        }
    }
}
