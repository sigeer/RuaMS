namespace Application.Shared.Models
{
    public record CoupleIdPair(int HusbandId, int WifeId);
    public record CoupleNamePair(string CharacterName1, string CharacterName2);

    public record CoupleTotal(int MarriageId, int HusbandId, int WifeId);
}
