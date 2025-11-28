using Application.Core.Channel;

namespace Application.Core.tools
{
    public class CharacterViewDtoUtils
    {
        public static string GetPlayerNameWithMedal(Dto.PlayerViewDto data)
        {
            return GetPlayerNameWithMedal(data.Character.Name,
                data.InventoryItems.FirstOrDefault(x => x.InventoryType == (int)InventoryType.EQUIPPED && x.Position == EquipSlot.Medal)?.Itemid ?? -1);
        }

        public static string GetPlayerNameWithMedal(string name, int medalItemId)
        {
            return GetPlayerNameWithMedal(name, ClientCulture.SystemCulture.GetItemName(medalItemId));
        }

        public static string GetPlayerNameWithMedal(string name, string? medalName)
        {
            var displayName = name;
            if (!string.IsNullOrWhiteSpace(medalName))
                displayName = $"<{medalName}> {displayName}";
            return displayName;
        }
    }
}
