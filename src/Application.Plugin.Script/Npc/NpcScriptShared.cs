
using Application.Plugin.Script;
using Application.Resources.Messages;

namespace Application.Plugin.Script.Npc
{
    /// <summary>
    /// 非数字命名的共享 NPC 脚本（无自身 NPC 入口，由其他脚本或 GM 指令间接调用）。
    /// 这些脚本在当前 WZ 中没有被任何 NPC 的 <c>script</c> 节点引用，属"改 WZ 即可用"的储备实现。
    /// </summary>
    internal partial class NpcScript
    {
        // Npc: 共享脚本 unidentifiedNpc
        /// <summary>找不到对应脚本时的兜底提示。</summary>
        [ScriptName("unidentifiedNpc")]
        public async Task unidentifiedNpc()
        {
            await SayOK(this.GetTalkMessage(ScriptMessage.Shared_UnidentifiedNpc, getNpc()));
        }

        // Npc: 共享脚本 ThiefPassword
        /// <summary>盗贼据点暗号门：答对暗号且完成前置任务后传送。</summary>
        [ScriptName("ThiefPassword")]
        public async Task ThiefPassword()
        {
            var input = await AskText(this.GetTalkMessage(ScriptMessage.Shared_SuspiciousVoice));
            if (input != this.GetTalkMessage(ScriptMessage.Shared_OpenSesame))
            {
                await SayOK(this.GetTalkMessage(ScriptMessage.Shared_Wrong));
                return;
            }

            if (!isQuestCompleted(3925))
            {
                await this.Pink(this.GetTalkMessage(ScriptMessage.Shared_AlthoughSaid));
                return;
            }

            await warp(260010402, 1);
        }

        // Npc: 共享脚本 9220005_old
        /// <summary>旧版雪原向导：确认后传送。</summary>
        [ScriptName("9220005_old")]
        public async Task OldSnowfieldGuide()
        {
            if (await AskYesNo(this.GetTalkMessage(ScriptMessage.Shared_WantGo)))
            {
                await warp(209080000, 0);
            }
        }

        // Npc: 共享脚本 9010022_old
        /// <summary>旧版次元之镜。</summary>
        [ScriptName("9010022_old")]
        public async Task OldDimensionalMirror()
        {
            await sendDimensionalMirror(this.GetTalkMessage(ScriptMessage.Shared_LikeGo));
        }

        // Npc: 共享脚本 9000021_old
        /// <summary>旧版转职 NPC：按职业推荐转职并重置属性。</summary>
        [ScriptName("9000021_old")]
        public async Task OldJobAdvancement()
        {
            var jobName = getJobName(getJobId());
            await SayNext(this.GetTalkMessage(ScriptMessage.Shared_JobAdv_Intro, getLevel(), jobName));
            await SayNext(this.GetTalkMessage(ScriptMessage.Shared_BeforeJob));

            if (!await AskYesNo(this.GetTalkMessage(ScriptMessage.Shared_WantAdvance)))
                return;

            await resetStats();
            await maxMastery();
        }

        // Npc: 共享脚本 rebirth
        /// <summary>重生（转生）：保留等级上限内的一次重生流程。</summary>
        [ScriptName("rebirth")]
        public async Task Rebirth()
        {
            var sel = await AskMenu(this.GetTalkMessage(ScriptMessage.Shared_Rebirth_Menu, getJobName(getJobId()), getLevel()));
            if (sel != 0)
                return;

            if (!await AskYesNo(this.GetTalkMessage(ScriptMessage.Shared_RebirthSet)))
            {
                await SayOK(this.GetTalkMessage(ScriptMessage.Shared_AlrightThink));
                return;
            }

            await resetStats();
            await SayNext(this.GetTalkMessage(ScriptMessage.Shared_RebirthComplete));
        }

        // Npc: 共享脚本 changeName
        /// <summary>改名卡（4031800）兑换：输入新名字后消耗道具。</summary>
        [ScriptName("changeName")]
        public async Task ChangeName()
        {
            const int coupon = 4031800;

            await SayNext(this.GetTalkMessage(ScriptMessage.Shared_GiveCharacter));
            if (!await AskYesNo(this.GetTalkMessage(ScriptMessage.Shared_WantChange)))
                return;

            if (!haveItem(coupon))
            {
                await SayOK(this.GetTalkMessage(ScriptMessage.Shared_ChangeName_NeedCoupon, coupon));
                return;
            }

            var newName = await AskText(this.GetTalkMessage(ScriptMessage.Shared_PleaseInput));
            if (string.IsNullOrWhiteSpace(newName))
            {
                await SayOK(this.GetTalkMessage(ScriptMessage.Shared_NameCannot));
                return;
            }

            await gainItem(coupon, -1);
            await SayOK(this.GetTalkMessage(ScriptMessage.Shared_ChangeName_Done, newName));
        }

        // Npc: 共享脚本 cpqchallenge2
        /// <summary>怪物嘉年华组队任务挑战入口（第二版）。</summary>
        [ScriptName("cpqchallenge2")]
        public async Task CpqChallenge2()
        {
            await SayOK(this.GetTalkMessage(ScriptMessage.Shared_MonsterCarnival));
        }

        // Npc: 共享脚本 waterOfLife
        /// <summary>生命之水：让一只干燥的宠物复活。</summary>
        [ScriptName("waterOfLife")]
        public async Task WaterOfLife()
        {
            const int waterOfLife = 5180000;

            var driedPets = getDriedPets();
            if (driedPets.Count == 0)
            {
                await this.Pink(this.GetTalkMessage(ScriptMessage.Shared_CurrentlyOwn));
                return;
            }

            if (!await AskYesNo(this.GetTalkMessage(ScriptMessage.Shared_HaveWater)))
                return;

            var menu = this.GetTalkMessage(ScriptMessage.Shared_PetWant);
            for (var i = 0; i < driedPets.Count; i++)
            {
                menu += this.GetTalkMessage(ScriptMessage.Shared_WaterOfLife_Entry, i, driedPets[i].getItemId());
            }

            var sel = await AskMenu(menu);
            if (sel < 0 || sel >= driedPets.Count)
                return;

            if (!haveItem(waterOfLife))
            {
                await SayOK(this.GetTalkMessage(ScriptMessage.Shared_WaterOfLife_NoItem, waterOfLife));
                return;
            }

            await gainItem(waterOfLife, -1);
            await SayOK(this.GetTalkMessage(ScriptMessage.Shared_DollHas));
        }

        // Npc: 共享脚本 mapleTV
        /// <summary>枫叶电视：开启自定义脚本时转向卷轴生成器，否则无行为。</summary>
        [ScriptName("mapleTV")]
        public async Task MapleTV()
        {
            if (Application.Utility.Configs.YamlConfig.config.server.USE_ENABLE_CUSTOM_NPC_SCRIPT)
            {
                await openNpc(9201088, "scroll_generator");
            }
        }

        // Npc: 共享脚本 credits
        /// <summary>制作人员名单。</summary>
        [ScriptName("credits")]
        public async Task Credits()
        {
            await SaySpeech([
                this.GetTalkMessage(ScriptMessage.Shared_RuamsMaplestory),
                this.GetTalkMessage(ScriptMessage.Shared_ThanksEveryone),
                this.GetTalkMessage(ScriptMessage.Shared_PressPrev)
            ]);
        }

        // Npc: 共享脚本 commands
        /// <summary>命令一览。</summary>
        [ScriptName("commands")]
        public async Task Commands()
        {
            await SayNext(this.GetTalkMessage(ScriptMessage.Shared_CommonCommands));
            await SayOK(this.GetTalkMessage(ScriptMessage.Shared_SeeServer));
        }
    }
}
