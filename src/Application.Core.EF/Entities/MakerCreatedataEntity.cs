namespace Application.EF.Entities;

public partial class MakerCreatedataEntity
{
    public sbyte Id { get; set; }

    public int Itemid { get; set; }

    public sbyte ReqLevel { get; set; }

    public sbyte ReqMakerLevel { get; set; }

    public int ReqMeso { get; set; }

    public int ReqItem { get; set; }

    public int ReqEquip { get; set; }

    public int Catalyst { get; set; }

    public short Quantity { get; set; }

    public sbyte Tuc { get; set; }
}
