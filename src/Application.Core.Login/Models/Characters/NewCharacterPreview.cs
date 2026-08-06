namespace Application.Core.Login.Models
{
    public class NewCharacterPreview : CharacterLiveObject
    {
        public AccountCtrl Account { get; }
        public NewCharacterPreview(AccountCtrl account, ProtoModel.CharacterProto character) : base(character)
        {
            Account = account;
        }
    }
}
