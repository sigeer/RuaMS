namespace Application.Core.model
{
    public record CharacterAccountIdPair(int AccountId, int CharacterId);

    public record CharacterBaseInfo(int AccountId, int CharacterId, string CharacterName);
}
