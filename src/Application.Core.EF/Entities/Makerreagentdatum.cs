﻿namespace Application.EF.Entities;

public partial class Makerreagentdatum
{
    public int Itemid { get; set; }

    public string Stat { get; set; } = null!;

    public short Value { get; set; }
}
