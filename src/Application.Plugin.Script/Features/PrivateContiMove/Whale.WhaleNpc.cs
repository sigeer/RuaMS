using Application.Core.Client;
using Application.Core.Game.Life;
using Application.Core.scripting.Events.Abstraction;
using Application.Core.scripting.npc;

namespace Application.Plugin.Script.Features.PrivateContiMove
{
    internal sealed class WhaleNpc : NpcScriptBase
    {
        public WhaleNpc(IChannelClient c, int npc, NPC? npcObject) : base(c, npc, npcObject)
        {
        }

        public async Task contimoveRieRit()
        {
            var contiMove = GetEventManager(WhaleFeature.EventName);
            if (await AskYesNo("搭上了这艘船，你可以前往更大的大陆冒险。 只要給我 #e80 金币#n，我会帶你去 #b金银岛#k 你想要去金银岛吗？"))
            {
                if (haveItem(4032338))
                {
                    await SaySpeech([
                        "搭上了这艘船，你可以前往更大的大陆冒险。 只要給我 #e80 金币#n，我会帶你去 #b金银岛#k 你想要去金银岛吗？",
                        "既然你有推荐信，我不会收你任何的费用。收起來，我们前往金银岛，坐好，旅途中可能会有点动荡！"
                        ], current: 1);

                    var r = await contiMove.StartInstance(getPlayer());
                    await SayNext(contiMove.HandleCreateInstanceResult(r, c));
                }
                else
                {
                    await SayNext("真的只要 #e80 金币#n 就能搭船!!");

                    if (getLevel() >= 8)
                    {
                        if (getMeso() < 80)
                        {
                            await SayOK("什么？你说你想搭免费的船？ 你真是个怪人！");
                            return;
                        }
                        else
                        {
                            await SayNext("哇! #e80#n 金币我收到了！ 好，准备触发去明珠港喽！");


                            var r = await contiMove.StartInstance(getPlayer());
                            if (r == CreateInstanceResult.Success)
                            {
                                await gainMeso(-80);
                            }
                            else
                            {
                                await SayNext(contiMove.HandleCreateInstanceResult(r, c));
                            }
                        }
                    }
                    else
                    {
                        await SayOK("让我看看... 我觉得你还不够强。 你至少要达到7级我才能让你到明珠港哦。");
                        return;
                    }
                }

            }
        }

        public async Task contimoveRitRie()
        {
            var contiMove = GetEventManager(WhaleFeature.EventName);

            await SayNext("你考虑离开金银岛前往我们的城镇吗？如果你登上这艘船，我可以带你从#b明珠港#k到#b里恩#k，然后再返回。但你必须支付#b800#k金币的费用。你想去#b里恩#k吗？");
            if (getMeso() < 800)
            {
                await SayNext("嗯... 你确定你有 #b800#k 金币吗？检查一下你的背包，确保你有足够的金币。你必须支付费用，否则我不能让你上船...");
            }
            else
            {
                var r = await contiMove.StartInstance(getPlayer());
                if (r == CreateInstanceResult.Success)
                {
                    await gainMeso(-800);
                }
                else
                {
                    await SayNext(contiMove.HandleCreateInstanceResult(r, c));
                }
            }
        }
    }
}
