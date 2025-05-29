namespace Application.EF.Entities;

public partial class Playerdisease
{
    private Playerdisease()
    {
    }

    public Playerdisease(int charid, int disease, int mobskillid, int mobskilllv, int length)
    {
        Charid = charid;
        Disease = disease;
        Mobskillid = mobskillid;
        Mobskilllv = mobskilllv;
        Length = length;
    }

    public int Id { get; set; }

    public int Charid { get; set; }

    public int Disease { get; set; }

    public int Mobskillid { get; set; }

    public int Mobskilllv { get; set; }

    public int Length { get; set; }
}
