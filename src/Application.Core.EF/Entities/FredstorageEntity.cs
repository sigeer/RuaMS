using Application.Core.EF;
using Google.Protobuf;

namespace Application.EF.Entities;

public partial class FredstorageEntity: IKeyedEntity<int>
{
    private FredstorageEntity() 
    {
        ItemsBlob = new ItemProto.PlayerShopStoreItems().ToByteArray();
    }
    public FredstorageEntity(int id, int cid, int daynotes, int meso, DateTimeOffset timestamp)
        : this()
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
    public byte[] ItemsBlob { get; set; }

    public DateTimeOffset Timestamp { get; set; }
}
