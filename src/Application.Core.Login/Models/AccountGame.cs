namespace Application.Core.Login.Models
{
    public class AccountGame
    {
        public int Id { get; set; }

        public int NxCredit { get; set; }

        public int MaplePoint { get; set; }

        public int NxPrepaid { get; set; }

        public QuickSlotModel? QuickSlot { get; set; }
        public StorageModel? Storage { get; set; }
        public ItemModel[] StorageItems { get; set; } = [];
        public ItemModel[] CashExplorerItems { get; set; } = [];
        public ItemModel[] CashCygnusItems { get; set; } = [];
        public ItemModel[] CashAranItems { get; set; } = [];
        public ItemModel[] CashOverallItems { get; set; } = [];

        public bool CanFly { get; set; }
    }
}
