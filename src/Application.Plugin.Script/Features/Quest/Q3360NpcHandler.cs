using Application.Core.Client;
using Application.Core.Game.Life;
using Application.Core.scripting.npc;
using tools;

namespace Application.Plugin.Script.Features.Quest
{
    /// <summary>
    /// 任务 3360 相关脚本：secretNPC。
    /// 该任务在 NPC 与传送门两侧都有判定，故集中在此。
    /// </summary>
    internal sealed class Q3360NpcHandler : NpcScriptBase
    {
        public Q3360NpcHandler(IChannelClient c, int npc, NPC? npcObj) : base(c, npc, npcObj)
        {
        }

        public async Task secretNPC()
        {
            var pwd = await AskText("门锁住了请输入#b密码#k!");
            var questPassword = getQuestProgress(3360, 0);

            if (pwd == questPassword)
            {
                var progress = getQuestProgress(3360, 1);
                if (progress != "11")
                {
                    if (getMapId() == 261020200)
                    {
                        if (progress == "10")
                        {
                            await setQuestProgress(3360, 1, "11");
                            await forceCompleteQuest(3360);
                            await Pink("已完成任务");
                        }
                        else
                        {
                            await setQuestProgress(3360, 1, "01");
                        }
                    }
                    else if (getMapId() == 261010000)
                    {
                        if (progress == "01")
                        {
                            await setQuestProgress(3360, 1, "11");
                            await forceCompleteQuest(3360);
                            await Pink("已完成任务");
                        }
                        else
                        {
                            await setQuestProgress(3360, 1, "10");
                        }
                    }
                }

                await getPlayer().SendPacket(PacketCreator.playPortalSound());
                await warp(261030000, "sp_" + (getMapId() == 261010000 ? "jenu" : "alca"));
            }
            else
            {
                await SayOK("#r错误的密码！");
            }
        }
    }
}
