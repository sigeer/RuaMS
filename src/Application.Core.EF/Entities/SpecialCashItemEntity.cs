namespace Application.EF.Entities;

public class SpecialCashItemEntity
{
    public int Id { get; set; }

    public int Sn { get; set; }

    /// <summary>
    /// 1024 is add/remove
    /// </summary>
    public int Modifier { get; set; }

    public int Info { get; set; }
}
