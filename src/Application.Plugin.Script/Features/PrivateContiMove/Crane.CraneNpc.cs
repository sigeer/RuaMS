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
                if (await AskYesNo($"你好。旅行进行得怎么样？我一直在像你这样的旅行者运送到#b{c.CurrentCulture.GetMapStreetName(250000100)}#k，而且……你有兴趣吗？这种方式没有船稳定，所以你得紧紧抓住，但我可以比船快得多地到达那里。只要你支付#b金币#k，我就会带你去那里。"))
                {
                    if (getMeso() > 1500)
                    {
                        await warp(250000100, 0);
                        await gainMeso(-1500);
                    }
                    else
                    {
                        await SayNext(GetClientMessage(nameof(ClientMessage.MesoNotEnough)));
                    }
                }
                else
                {
                    await SayNext("改变想法随时跟我搭话吧。");
                }
                return;

            }
            else if (curMap == 200000141)
            {
                // 天空之城
                await AskMenu("你好。旅行进行得怎么样？我已无数次将像你这样的冒险家快速送达其他区域，现在……你有兴趣吗？若愿意，请选择你想前往的城镇。", [
                    $"#m250000100# (1500 金币)",
                    ]);
                if (!await AskYesNo($"你确定要去 #b{c.CurrentCulture.GetMapStreetName(250000100)}#k 吗？如果你有 #b1500 金币#k, 我现在就带你去。"))
                {
                    await SayNext("改变想法随时跟我搭话吧。");
                    return;
                }
                contiMove = GetEventManager(CraneFeature.EventName);
            }
            else if (curMap == 250000100)
            {
                // 武陵
                int[] options = [200000141, 251000000];
                var option = await AskMenu("你好。旅行进行得怎么样？我明白，与能翱翔天际的我相比，双腿行走要艰难得多。我已无数次将像你这样的冒险家快速送达其他区域，现在……你有兴趣吗？若愿意，请选择你想前往的城镇。",
                                    options.Select(x => $"{c.CurrentCulture.GetMapStreetName(x)} (1500 金币)"));

                if (option == 1)
                {
                    if (getMeso() > 1500)
                    {
                        await warp(options[option], 0);
                        await gainMeso(-1500);
                    }
                    else
                    {
                        await SayNext(GetClientMessage(nameof(ClientMessage.MesoNotEnough)));
                    }
                    return;
                }
                else
                {
                    if (!await AskYesNo($"你确定要去 #b{c.CurrentCulture.GetMapStreetName(options[option])}#k 吗？如果你有 #b1500 金币#k, 我现在就带你去。"))
                    {
                        await SayNext("改变想法随时跟我搭话吧。");
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
