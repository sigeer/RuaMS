using Application.Core.Channel.DataProviders;

namespace Application.Core.tools
{
    public class CharacterViewDtoUtils
    {
        public static string GetPlayerNameWithMedal(Dto.PlayerViewDto data)
        {
            return GetPlayerNameWithMedal(data.Character.Name, data.InventoryItems.FirstOrDefault(x => x.InventoryType == (int)InventoryType.EQUIPPED && x.Position == EquipSlot.Medal));
        }

        public static string GetPlayerNameWithMedal(string name, Dto.ItemDto? medalItem)
        {
            var displayName = name;
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
