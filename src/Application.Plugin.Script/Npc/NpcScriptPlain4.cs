
using Application.Plugin.Script;
using Application.Resources.Messages;

namespace Application.Plugin.Script.Npc
{
    /// <summary>
    /// 新手训练营引导 NPC（WZ 无 script 节点，运行期键 <c>n&lt;id&gt;</c>）。
    /// </summary>
    internal partial class NpcScript
    {
        // Npc: 2100
        /// <summary>
        /// 训练营入口：在出生地图（0 / 3）询问是否进入训练营，
        /// 否则讲解职业介绍。JS 用 <c>mode==1 &amp;&amp; type==1</c> 区分「训练营」与「直接开始冒险」。
        /// </summary>
        [ScriptName("n2100")]
        public async Task MapleIslandTrainingCamp()
        {
            var onBirthMap = getMapId() == 0 || getMapId() == 3;
            if (!onBirthMap)
            {
                await SayNext(this.GetTalkMessage(ScriptMessage.Plain4_ImageRoom));
                await SayNext(this.GetTalkMessage(ScriptMessage.Plain4_OnceTrain));
                return;
            }

            var enterCamp = await AskYesNo(this.GetTalkMessage(ScriptMessage.Plain4_WelcomeWorld));
            if (enterCamp)
            {
                await SayNext(this.GetTalkMessage(ScriptMessage.Plain4_LetEnter));
                await warp(1, 0);
                return;
            }

            if (await AskYesNo(this.GetTalkMessage(ScriptMessage.Plain4_ReallyWant)))
            {
                await SayNext(this.GetTalkMessage(ScriptMessage.Plain4_SeemsLike));
                await warp(40000, 0);
                return;
            }

            await SayNext(this.GetTalkMessage(ScriptMessage.Plain4_PleaseTalk));
        }

        // Npc: 2101
        /// <summary>训练营出口：确认训练完成后送回出生地图；在 10000 号地图时只讲解操作。</summary>
        [ScriptName("n2101")]
        public async Task MapleIslandTrainingCampExit()
        {
            if (getMapId() == 10000)
            {
                await SayOK(this.GetTalkMessage(ScriptMessage.Plain4_YoureNew));
                return;
            }

            if (!await AskYesNo(this.GetTalkMessage(ScriptMessage.Plain4_DoneTraining)))
            {
                await SayOK(this.GetTalkMessage(ScriptMessage.Plain4_HaventFinished));
                return;
            }

            await SayNext(this.GetTalkMessage(ScriptMessage.Plain4_SendOut));
            await warp(0);
        }
    }
}
