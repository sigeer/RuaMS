namespace Application.EF.Entities;

public partial class StorageEntity
{
    protected StorageEntity() { }
    public StorageEntity(int accountid, int type, int slots, int meso)
    {
        OwnerId = accountid;
        Slots = slots;
        Meso = meso;
        Type = type;
    }

    public int Storageid { get; set; }

    public int OwnerId { get; set; }


    public int Slots { get; set; }

    public int Meso { get; set; }
    /// <summary>
    /// 0. 仓库，1. 扭蛋仓库
    /// </summary>
    public int Type { get; set; }
}
