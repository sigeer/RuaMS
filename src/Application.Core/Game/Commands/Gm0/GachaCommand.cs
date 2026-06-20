using Application.Core.Channel.ServerData;
using Application.Core.Models;
using Application.Resources.Messages;
using System.Text;

namespace Application.Core.Game.Commands.Gm0;

public class GachaCommand : CommandBase
{
    readonly GachaponManager _gachaponManager;
    public GachaCommand(GachaponManager gachaponManager) : base(0, "gacha")
    {
        _gachaponManager = gachaponManager;
    }

    public override async Task Execute(IChannelClient c, string[] paramValues)
    {
        GachaponDataObject? gacha = null;
        string search = c.OnlinedCharacter.getLastCommandMessage();
        string gachaName = "";
        string[] names = {
            c.CurrentCulture.GetMessageByKey(nameof(ClientMessage.Map_Henesys)),
            c.CurrentCulture.GetMessageByKey(nameof(ClientMessage.Map_Ellinia)),
            c.CurrentCulture.GetMessageByKey(nameof(ClientMessage.Map_Perion)),
            c.CurrentCulture.GetMessageByKey(nameof(ClientMessage.Map_KerningCity)),
            c.CurrentCulture.GetMessageByKey(nameof(ClientMessage.Map_Sleepywood)),
            c.CurrentCulture.GetMessageByKey(nameof(ClientMessage.Map_MushroomShrine)),
            c.CurrentCulture.GetMessageByKey(nameof(ClientMessage.Map_ShowaSpaMale)),
            c.CurrentCulture.GetMessageByKey(nameof(ClientMessage.Map_ShowaSpaFemale)),
            c.CurrentCulture.GetMessageByKey(nameof(ClientMessage.Map_Ludibrium)),
            c.CurrentCulture.GetMessageByKey(nameof(ClientMessage.Map_NewLeafCity)),
            c.CurrentCulture.GetMessageByKey(nameof(ClientMessage.Map_ElNath)),
            c.CurrentCulture.GetMessageByKey(nameof(ClientMessage.Map_NautilusHarbor))
        };
        int[] ids = [
            NpcId.GACHAPON_HENESYS,
            NpcId.GACHAPON_ELLINIA,
            NpcId.GACHAPON_PERION,
            NpcId.GACHAPON_KERNING,
            NpcId.GACHAPON_SLEEPYWOOD,
            NpcId.GACHAPON_MUSHROOM_SHRINE,
            NpcId.GACHAPON_SHOWA_MALE,
            NpcId.GACHAPON_SHOWA_FEMALE,
            NpcId.GACHAPON_LUDIBRIUM,
            NpcId.GACHAPON_NLC,
            NpcId.GACHAPON_EL_NATH,
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
            await c.OnlinedCharacter.Yellow(nameof(ClientMessage.GachaCommand_Syntax));
            foreach (string name in names)
            {
                await c.OnlinedCharacter.Yellow(name);
            }
            return;
        }
        StringBuilder sb = new StringBuilder();
        sb.Append(c.CurrentCulture.GetMessageByKey(nameof(ClientMessage.GachaCommand_Message1), gachaName));
        foreach (var chance in _gachaponManager.GetPoolLevelList(gacha.Id))
        {
            foreach (var item in _gachaponManager.GetItems(gacha.Id, chance.Level))
            {
                sb.Append("-").Append(c.CurrentCulture.GetItemName(item.ItemId)).Append("\r\n");
            }
        }
        sb.Append("\r\n").Append(c.CurrentCulture.GetMessageByKey(nameof(ClientMessage.GachaCommand_Message2)));

        await c.OnlinedCharacter.Dialog(sb.ToString());
    }
}
