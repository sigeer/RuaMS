using Application.Core.Login.Models;
using Application.Shared.Constants.Item;
using Application.Shared.Constants.Map;

namespace Application.Core.Login.Services.PlayerCreator.Novice;

public class BeginnerCreator : NoviceCreator
{
    protected override NewCharacterBuilder CreateBuilder(string name, int gendar, int top, int bottom, int shoes, int weapon)
    {
        var builder = new NewCharacterBuilder(name, gendar, Job.BEGINNER, 1, MapId.MUSHROOM_TOWN, top, bottom, shoes, weapon);
        builder.Items.Add(ItemModel.NewEtcItem(ItemId.BEGINNERS_GUIDE, 1));
        return builder;
    }
}
