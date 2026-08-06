namespace Application.Core.Mappers
{
    [Mapper]
    public interface IPlayerMapper
    {
        ProtoModel.CharacterProto MapToDto(Player item);

        Player MapToExisting(ProtoModel.CharacterProto dto, Player player);
    }
}
