﻿
namespace Application.EF.Entities;

public partial class Famelog
{
    public int Famelogid { get; set; }

    public int Characterid { get; set; }

    public int CharacteridTo { get; set; }

    public DateTimeOffset When { get; set; }

    public virtual CharacterEntity Character { get; set; } = null!;
}
