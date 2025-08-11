using Application.Core.Channel.DataProviders;
using Application.Core.Channel.ServerData;
using Application.Core.Models;
using Application.Core.scripting.npc;

namespace Application.Core.Game.Commands.Gm0;

public class GachaCommand : CommandBase
{
    readonly GachaponManager _gachaponManager;
    public GachaCommand(GachaponManager gachaponManager) : base(0, "gacha")
    {
        Description = "Show gachapon rewards.";
        _gachaponManager = gachaponManager;
    }

    public override void Execute(IChannelClient c, string[] paramValues)
    {
        GachaponDataObject? gacha = null;
        string search = c.OnlinedCharacter.getLastCommandMessage();
        string gachaName = "";
        string[] names = { "Henesys", "Ellinia", "Perion", "Kerning City", "Sleepywood", "Mushroom Shrine", "Showa Spa Male", "Showa Spa Female", "New Leaf City", "Nautilus Harbor" };
        int[] ids = [
            NpcId.GACHAPON_HENESYS,
            NpcId.GACHAPON_ELLINIA,
            NpcId.GACHAPON_PERION,
            NpcId.GACHAPON_KERNING,
            NpcId.GACHAPON_SLEEPYWOOD,
            NpcId.GACHAPON_MUSHROOM_SHRINE,
            NpcId.GACHAPON_SHOWA_MALE,
            NpcId.GACHAPON_SHOWA_FEMALE,
            NpcId.GACHAPON_NLC,
            NpcId.GACHAPON_NAUTILUS];

        for (int i = 0; i < names.Length; i++)
        {
            if (search.Equals(names[i], StringComparison.OrdinalIgnoreCase))
            {
                gachaName = names[i];
                gacha = _gachaponManager.GetByNpcId(ids[i]);
                break;
            }
        }

        if (gacha == null)
        {
            c.OnlinedCharacter.yellowMessage("Please use @gacha <name> where name corresponds to one of the below:");
            foreach (string name in names)
            {
                c.OnlinedCharacter.yellowMessage(name);
            }
            return;
        }
        string talkStr = "The #b" + gachaName + "#k Gachapon contains the following items.\r\n\r\n";
        foreach (var chance in _gachaponManager.GetPoolLevelList(gacha.Id))
        {
            foreach (var item in _gachaponManager.GetItems(gacha.Id, chance.Level))
            {
                talkStr += "-" + ItemInformationProvider.getInstance().getName(item.ItemId) + "\r\n";
            }
        }
        talkStr += "\r\nPlease keep in mind that there are items that are in all gachapons and are not listed here.";

        TempConversation.Create(c)?.RegisterTalk(talkStr);
    }
}
