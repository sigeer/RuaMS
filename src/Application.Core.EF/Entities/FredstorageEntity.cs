namespace Application.EF.Entities;

public partial class FredstorageEntity
{
    private FredstorageEntity() { }
    public FredstorageEntity(int id, int cid, int daynotes, int meso, DateTimeOffset timestamp)
    {
        Id = id;
        Cid = cid;
        Daynotes = daynotes;
        Timestamp = timestamp;
        Meso = meso;
    }

    public int Id { get; set; }

    public int Cid { get; set; }

    public int Daynotes { get; set; }
    public int Meso { get; set; }
    public int ItemMeso { get; set; }

    public DateTimeOffset Timestamp { get; set; }
}
