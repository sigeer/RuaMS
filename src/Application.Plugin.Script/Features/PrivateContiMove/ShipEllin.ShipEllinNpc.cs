using Application.Core.Client;
using Application.Core.Game.Life;
using Application.Core.scripting.Events.Abstraction;
using Application.Core.scripting.npc;

namespace Application.Plugin.Script.Features.PrivateContiMove
{
    internal sealed class ShipEllinNpc : NpcScriptBase
    {
        public ShipEllinNpc(IChannelClient c, int npc, NPC? npcObject) : base(c, npc, npcObject)
        {
        }

        public async Task contimoveEreEli()
        {
            var contiMove = GetEventManager(ShipEllinFeature.EventName);
            if (await AskYesNo("嗯，你好...又来了。你想离开圣地去别的地方吗？如果是的话，你来对地方了。我经营着一艘从#b圣地#k到#b金银岛#k的渡船，如果你愿意的话，我可以带你去#b金银岛#k...你需要支付#b1000#k金币的费用。\r\n"))
            {
                if (getMeso() < 1000)
                {
                    await SayNext("嗯... 你确定你有 #b1000#k 金币吗？检查一下你的背包，确保你有足够的金币。你必须支付费用，否则我不能让你上船...");
                }
                else
                {
                    var r = await contiMove.StartInstance(getPlayer());
                    if (r == CreateInstanceResult.Success)
                    {
                        await gainMeso(-1000);
                    }
                    else
                    {
                        await SayNext(contiMove.HandleCreateInstanceResult(r, c));
                    }

                }
            }
            else
            {
                await SayNext("好的。如果你改变主意了，请告诉我。");
            }
        }

        public async Task contimoveEliEre()
        {
            var contiMove = GetEventManager(ShipEllinFeature.EventName);
            if (await AskYesNo("嗯...那么...嗯...你是想离开金银岛去其他地区吗？你可以乘这艘船去#b圣地#k。在那里，你会看到明亮的阳光照在树叶上，感受到轻柔的微风拂过你的皮肤。那里是神兽和女皇所在的地方。你想去圣地吗？大约需要#b2分钟#k，费用是#b1000#k金币。\r\n"))
            {
                if (getMeso() < 1000)
                {
                    await SayNext("嗯... 你确定你有 #b1000#k 金币吗？检查一下你的背包，确保你有足够的金币。你必须支付费用，否则我不能让你上船...");
                }
                else
                {
                    var r = await contiMove.StartInstance(getPlayer());
                    if (r == CreateInstanceResult.Success)
                    {
                        await gainMeso(-1000);
                    }
                    else
                    {
                        await SayNext(contiMove.HandleCreateInstanceResult(r, c));
                    }
                }
            }
            else
            {
                await SayNext("好的。如果你改变主意了，请告诉我。");
            }
        }
    }
}
