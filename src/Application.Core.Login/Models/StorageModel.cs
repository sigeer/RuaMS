namespace Application.Core.Login.Models
{
    public class StorageModel
    {

        public StorageModel() { }

        public StorageModel(int accountid)
        {
            Accountid = accountid;
            Slots = 4;
            Meso = 0;
        }

        public int Accountid { get; set; }

        public byte Slots { get; set; }

        public int Meso { get; set; }
    }
}
