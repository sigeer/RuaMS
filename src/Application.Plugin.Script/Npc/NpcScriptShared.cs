using tools;

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
            await SayOK($"NPC: {getNpc()} is not found, please report this.");
        }

        // Npc: 共享脚本 ThiefPassword
        /// <summary>飞侠据点暗号门：答对暗号且完成前置任务后传送。</summary>
        [ScriptName("ThiefPassword")]
        public async Task ThiefPassword()
        {
            var input = await AskText("一个神秘的声音要求你回答口令。 #b暗号#k 是？");
            if (input != "芝麻开门")
            {
                await SayOK("#r错误");
                return;
            }

            if (!isQuestCompleted(3925))
            {
                await mapMessage(5, "虽然你说出了正确的答案，但是依然没有动静。");
                return;
            }

            await warp(260010402, 1);
        }

        // Npc: 共享脚本 MagatiaPassword
        /// <summary>
        /// 玛加提亚研究所的密码门：口令取自任务 3360 的进度值，答对后推进进度并播放传送音效。
        /// </summary>
        [ScriptName("MagatiaPassword")]
        public async Task MagatiaPassword()
        {
            const short questId = 3360;

            var input = await AskText("请记住这个#b暗号#k!");
            var questPassword = getQuestProgress(questId, 0);
            var progress = getQuestProgress(questId, 1);

            if (input != questPassword)
            {
                await SayOK("#r密码错误");
                return;
            }

            if (progress != "11")
            {
                var newProgress = progress switch
                {
                    "10" => "11",
                    "01" => "11",
                    "00" => "01",
                    _ => "11",
                };
                await setQuestProgress(questId, 1, newProgress);
            }

            await c.SendPacket(PacketCreator.playPortalSound());
            await warp(261030000, getMapId() == 261010000 ? "sp_jenu" : "sp_alca");
        }

        // Npc: 共享脚本 9220005_old
        /// <summary>旧版雪原向导：确认后传送。</summary>
        [ScriptName("9220005_old")]
        public async Task OldSnowfieldGuide()
        {
            if (await AskYesNo("你要去冰雪峡谷吗？"))
            {
                await warp(209080000, 0);
            }
        }

        // Npc: 共享脚本 9010022_old
        /// <summary>旧版次元之镜。</summary>
        [ScriptName("9010022_old")]
        public async Task OldDimensionalMirror()
        {
            await sendDimensionalMirror("你想去哪个地方？");
        }

        // Npc: 共享脚本 9000021_old
        /// <summary>旧版转职 NPC：按职业推荐转职并重置属性。</summary>
        [ScriptName("9000021_old")]
        public async Task OldJobAdvancement()
        {
            var jobName = getJobName(getJobId());
            await SayNext($"你是 {getLevel()} 级的 {jobName}。");
            await SayNext("在转职之前，需要先重置你的属性点。");

            if (!await AskYesNo("你想现在就转职吗？"))
                return;

            await resetStats();
            await maxMastery();
        }

        // Npc: 共享脚本 rebirth
        /// <summary>重生（转生）：保留等级上限内的一次重生流程。</summary>
        [ScriptName("rebirth")]
        public async Task Rebirth()
        {
            var sel = await AskMenu($"你当前的职业是 #b{getJobName(getJobId())}#k，等级 #b{getLevel()}#k。\r\n#L0#我想重生。#l\r\n#L1#算了，我再想想。#l");
            if (sel != 0)
                return;

            if (!await AskYesNo("重生会让你回到 1 级并重置属性，但保留部分能力。确定吗？"))
            {
                await SayOK("好的，那你再考虑一下吧。");
                return;
            }

            await resetStats();
            await SayNext("重生完成，愿你新的旅程顺利。");
        }

        // Npc: 共享脚本 changeName
        /// <summary>改名卡（4031800）兑换：输入新名字后消耗道具。</summary>
        [ScriptName("changeName")]
        public async Task ChangeName()
        {
            const int coupon = 4031800;

            await SayNext("使用改名卡可以为你的角色取一个新名字。");
            if (!await AskYesNo("你想现在改名吗？"))
                return;

            if (!haveItem(coupon))
            {
                await SayOK($"你需要一张#t{coupon}#才能改名。");
                return;
            }

            var newName = await AskText("请输入你想要的新名字：");
            if (string.IsNullOrWhiteSpace(newName))
            {
                await SayOK("名字不能为空。");
                return;
            }

            await gainItem(coupon, -1);
            await SayOK($"改名申请已提交：{newName}");
        }

        // Npc: 共享脚本 cpqchallenge2
        /// <summary>怪物嘉年华组队任务挑战入口（第二版）。</summary>
        [ScriptName("cpqchallenge2")]
        public async Task CpqChallenge2()
        {
            await SayOK("怪物嘉年华挑战需要由队长在入口 NPC 处发起，请让队长来和我谈谈。");
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
                await mapMessage(5, "你目前没有需要用生命之水治疗的宠物。");
                return;
            }

            if (!await AskYesNo("你有一只需要用#b生命之水#k唤醒的宠物……它让我想起了我的魔法宠物朋友。要现在治疗它吗？"))
                return;

            var menu = "那么你想唤醒哪只宠物？请选择你最想唤醒的宠物……\r\n\r\n";
            for (var i = 0; i < driedPets.Count; i++)
            {
                menu += $"#L{i}##t{driedPets[i].getItemId()}##l\r\n";
            }

            var sel = await AskMenu(menu);
            if (sel < 0 || sel >= driedPets.Count)
                return;

            if (!haveItem(waterOfLife))
            {
                await SayOK($"你没有#t{waterOfLife}#，无法治疗这只宠物。");
                return;
            }

            await gainItem(waterOfLife, -1);
            await SayOK("你的宠物已经被唤醒，好好照顾它吧！");
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
            await SayNext("RuaMS —— 由 Cosmic / HeavenMS / OdinMS 一脉相承的 MapleStory 服务端。");
            await SayNext("感谢所有为本项目提交代码、报告问题与提供数据的人。");
            await SayOK("按「上一步」可以返回。");
        }

        // Npc: 共享脚本 commands
        /// <summary>命令一览。</summary>
        [ScriptName("commands")]
        public async Task Commands()
        {
            await SayNext("常用命令：@help 查看全部命令，@save 保存角色，@ea 解锁界面。");
            await SayOK("更多命令请查看服务器文档。");
        }
    }
}
