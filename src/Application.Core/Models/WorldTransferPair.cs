namespace Application.Core.model
{
    public record WorldTransferPair(int Old, int New);
    public record CharacterWorldTransferPair(int CharacterId, int OldId, int NewId);
}
