using Application.Core.Channel;
using Application.Core.Channel.DataProviders;

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
            var displayName = name;
            if (medalItemId > 0)
            {
                var medalName = ClientCulture.SystemCulture.GetItemName(medalItemId);
                if (!string.IsNullOrWhiteSpace(medalName))
                    displayName = $"<{medalName}> {displayName}";
            }
            return displayName;
        }
    }
}
