using Application.Core.Login.Models;
using Application.Shared.Constants.Item;
using Application.Shared.Constants.Map;
using Application.Shared.Items;

namespace Application.Core.Login.Services.PlayerCreator.Novice;

public class LegendCreator : NoviceCreator
{
    protected override NewCharacterBuilder CreateBuilder(string name, int gendar, int top, int bottom, int shoes, int weapon)
    {
        var builder = new NewCharacterBuilder(name, gendar, Job.LEGEND, 1, MapId.ARAN_TUTORIAL_START, top, bottom, shoes, weapon);
        return builder;
    }


    public override NewCharacterPreview CreateCharacter(AccountCtrl account, string name, int face, int hair, int skin, int top, int bottom, int shoes, int weapon, int gender)
    {
        var model = base.CreateCharacter(account, name, face, hair, skin, top, bottom, shoes, weapon, gender);
        model.Character.Data.Bag.EtcInv.Add(new Dto.ItemDto { Itemid = ItemId.LEGENDS_GUIDE, Position = 1 });
        return model;
    }
}
