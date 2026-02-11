using Application.Core.Login.Models;

namespace Application.Core.Login.Services.PlayerCreator.Novice
{
    public abstract class NoviceCreator
    {
        protected NoviceCreator() : base()
        {
        }

        protected abstract NewCharacterBuilder CreateBuilder(string name, int gendar, int top, int bottom, int shoes, int weapon);

        public virtual NewCharacterPreview CreateCharacter(AccountCtrl account, string name, int face, int hair, int skin, int top, int bottom, int shoes, int weapon, int gender)
        {
            var builder = CreateBuilder(name, gender, top, bottom, shoes, weapon);
            builder.Face = face;
            builder.Hair = hair;
            builder.SkinColor = skin;

            return builder.Build(account);
        }
    }
}
