namespace Application.EF.Entities;

public partial class MakerCreatedataEntity
{
    public int Id { get; set; }

    public int Itemid { get; set; }

    public short ReqLevel { get; set; }

    public short ReqMakerLevel { get; set; }

    public int ReqMeso { get; set; }

    public int ReqItem { get; set; }

    public int ReqEquip { get; set; }

    public int Catalyst { get; set; }

    public short Quantity { get; set; }

    public sbyte Tuc { get; set; }
}
