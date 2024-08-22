namespace Application.EF.Entities;

public partial class Skillmacro
{
    public Skillmacro()
    {
    }

    public Skillmacro(int characterid, sbyte position, int skill1, int skill2, int skill3, string? name, sbyte shout)
    {
        Characterid = characterid;
        Position = position;
        Skill1 = skill1;
        Skill2 = skill2;
        Skill3 = skill3;
        Name = name;
        Shout = shout;
    }

    public int Id { get; set; }

    public int Characterid { get; set; }

    public sbyte Position { get; set; }

    public int Skill1 { get; set; }

    public int Skill2 { get; set; }

    public int Skill3 { get; set; }

    public string? Name { get; set; }

    public sbyte Shout { get; set; }
}
