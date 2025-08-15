namespace Application.EF.Entities;

public partial class StorageEntity
{
    protected StorageEntity() { }
    public StorageEntity(int accountid, int slots, int meso)
    {
        Accountid = accountid;
        Slots = slots;
        Meso = meso;
    }

    public int Storageid { get; set; }

    public int Accountid { get; set; }


    public int Slots { get; set; }

    public int Meso { get; set; }
}
