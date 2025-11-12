namespace Application.Core.Login.Models
{
    public class StorageModel
    {

        public StorageModel() { }
        public StorageModel(int accountid, int type)
        {
            OwnerId = accountid;
            Slots = 4;
            Meso = 0;
            Type = type;
        }

        public int OwnerId { get; set; }

        public byte Slots { get; set; }

        public int Meso { get; set; }
        public ItemModel[] Items { get; set; } = [];
        public int Type { get; set; }
    }
}
