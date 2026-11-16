using Application.Core.Client;
using Application.Core.Game.Life;
using Application.Core.scripting.Events.Abstraction;
using Application.Core.scripting.npc;
using Application.Core.Scripting.Events;
using Application.Resources.Messages;
using Application.Shared.Constants.Map;

namespace Application.Plugin.Script.Features.PrivateContiMove
{
    internal sealed class CraneNpc : NpcScriptBase
    {
        public CraneNpc(IChannelClient c, int npc, NPC? npcObject) : base(c, npc, npcObject)
        {
        }

        public async Task crane()
        {
            var curMap = getMapId();
            AbstractEventManager? contiMove = null;
            if (curMap == MapId.HERB_TOWN)
            {
                if (await AskYesNo(this.GetTalkMessage(ScriptMessage.Crane_IntroHerbTown, c.CurrentCulture.GetMapStreetName(250000100))))
                {
                    if (getMeso() > 1500)
                    {
                        await warp(250000100, 0);
                        await gainMeso(-1500);
                    }
                    else
                    {
                        await SayNext(this.GetTalkMessage(ScriptMessage.MesoNotEnough));
                    }
                }
                else
                {
                    await SayNext(this.GetTalkMessage(ScriptMessage.Crane_ChangedMind));
                }
                return;

            }
            else if (curMap == 200000141)
            {
                // 天空之城
                await AskMenu(this.GetTalkMessage(ScriptMessage.Crane_PickTown), [
                    this.GetTalkMessage(ScriptMessage.Crane_Option, 250000100),
                    ]);
                if (!await AskYesNo(this.GetTalkMessage(ScriptMessage.Crane_Confirm, c.CurrentCulture.GetMapStreetName(250000100))))
                {
                    await SayNext(this.GetTalkMessage(ScriptMessage.Crane_ChangedMind));
                    return;
                }
                contiMove = GetEventManager(CraneFeature.EventName);
            }
            else if (curMap == 250000100)
            {
                // 武陵
                int[] options = [200000141, 251000000];
                var option = await AskMenu(this.GetTalkMessage(ScriptMessage.Crane_IntroMuLung),
                                    options.Select(x => this.GetTalkMessage(ScriptMessage.Crane_OptionByName, c.CurrentCulture.GetMapStreetName(x))));

                if (option == 1)
                {
                    if (getMeso() > 1500)
                    {
                        await warp(options[option], 0);
                        await gainMeso(-1500);
                    }
                    else
                    {
                        await SayNext(this.GetTalkMessage(ScriptMessage.MesoNotEnough));
                    }
                    return;
                }
                else
                {
                    if (!await AskYesNo(this.GetTalkMessage(ScriptMessage.Crane_Confirm, c.CurrentCulture.GetMapStreetName(options[option]))))
                    {
                        await SayNext(this.GetTalkMessage(ScriptMessage.Crane_ChangedMind));
                        return;
                    }
                    contiMove = GetEventManager(CraneFeature.EventName);
                }


            }

            if (contiMove != null)
            {
                if (getMeso() > 1500)
                {
                    var r = await contiMove.StartInstance(getPlayer());
                    if (r == CreateInstanceResult.Success)
                    {
                        await gainMeso(-1500);
                    }
                    else
                    {
                        await SayOK(contiMove.HandleCreateInstanceResult(r, c));
                    }
                }
            }
        }
    }
}
