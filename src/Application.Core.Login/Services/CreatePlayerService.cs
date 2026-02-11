using Application.Core.Login.Models;
using Application.Core.Login.Services.PlayerCreator;
using Application.Templates.Etc;
using System.Text.Json;

namespace Application.Core.Login.Services
{
    public class CreatePlayerService
    {
        readonly MasterServer _server;
        readonly MakerCharInfoTemplate _template;

        public CreatePlayerService(MasterServer server)
        {
            _server = server;
            _template = JsonSerializer.Deserialize<MakerCharInfoTemplate>(File.ReadAllText(
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "MakeCharInfo.json")))!;
        }

        public NewCharacterPreview? CreateCharacter(AccountCtrl accountInfo, int jobType, string name, int gender, int face, int hair, int skin, int top, int bottom, int shoes, int weapon)
        {
            if (accountInfo.Characterslots - _server.AccountManager.GetAccountPlayerIds(accountInfo.Id).Count <= 0)
            {
                // 角色槽不足
                return null;
            }

            if (!_server.CharacterManager.CheckCharacterName(name))
            {
                // 角色名不通过
                return null;
            }


            MakerCharInfoItemTemplate? item = GetCheckTemplate(jobType, gender);

            if (item != null)
            {
                if (CheckData(item, face, hair, skin, top, bottom, shoes, weapon))
                {
                    var model = CharacterFactory.GetNoviceCreator(jobType)
                        .CreateCharacter(accountInfo, name, face, hair, skin, top, bottom, shoes, weapon, gender);

                    _server.CharacterManager.InsertNewCharacter(model);
                    return model;
                }
            }
            return null;
        }


        MakerCharInfoItemTemplate? GetCheckTemplate(int jobType, int gender)
        {
            MakerCharInfoItemTemplate? item = null;
            var key = jobType * 10 + gender;
            switch (key)
            {
                case 0:
                    item = _template.PremiumCharMale;
                    break;
                case 1:
                    item = _template.PremiumCharFemale;
                    break;
                case 10:
                    item = _template.CharMale;
                    break;
                case 11:
                    item = _template.CharFemale;
                    break;
                case 20:
                    item = _template.OrientCharMale;
                    break;
                case 21:
                    item = _template.OrientCharFemale;
                    break;
                default:
                    break;
            }
            return item;

        }
        bool CheckData(MakerCharInfoItemTemplate template, int face, int hair, int skin, int top, int bottom, int shoes, int weapon)
        {
            return template.WeaponIdArray.Contains(weapon)
                && template.FaceIdArray.Contains(face)
                && template.HairIdArray.Contains((int)(hair / 10) * 10)
                && template.HairColorIdArray.Contains(hair % 10)
                && template.SkinIdArray.Contains(skin)
                && template.TopIdArray.Contains(top)
                && template.BottomIdArray.Contains(bottom)
                && template.ShoeIdArray.Contains(shoes);
        }
    }
}
