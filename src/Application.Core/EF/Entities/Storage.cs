namespace Application.EF.Entities;

public partial class StorageEntity
{
    public int Storageid { get; set; }

    public int Accountid { get; set; }

    public int World { get; set; }

    public int Slots { get; set; }

    public int Meso { get; set; }
}
