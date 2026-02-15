using Application.Utility.Extensions;

namespace Application.Core.Login.Models
{
    public class NewCharacterPreview : CharacterLiveObject
    {
        public AccountCtrl Account { get; }
        public NewCharacterPreview(AccountCtrl account, CharacterModel character, ItemModel[] equips) : base(character, equips)
        {
            Account = account;
        }
    }

    public class EquippedViewModel
    {
        public EquippedViewModel(int itemId, int position)
        {
            ItemId = itemId;
            Position = position;
        }

        public int ItemId { get; set; }
        public int Position { get; set; }
    }
}
