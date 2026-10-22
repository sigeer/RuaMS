namespace Application.Plugin.Script.Npc
{
    /// <summary>
    /// 训练中心入口 NPC（20 级以下才能进入，5 个房间，每间最多 5 人）。
    /// JS: scripts/npc/{1012118,1022104,1032113,1052113,1095001}.js —— 五份结构完全一致，仅地图号与个别文案不同。
    /// 另有 1012118 在玩家进行 22515~22518 任务时走「特殊训练中心」分支。
    /// </summary>
    internal partial class NpcScript
    {
        /// <summary>训练中心房间数。</summary>
        const int TRAINING_ROOMS = 5;
        /// <summary>每间房的人数上限。</summary>
        const int TRAINING_MAX_PLAYERS = 5;
        /// <summary>特殊训练中心（进行新手引导任务时进入）。</summary>
        const int TRAINING_SPECIAL_MAP = 910060100;

        /// <summary>新手引导任务，进行中时直接进入特殊训练中心。</summary>
        static readonly int[] BeginnerTutorialQuests = [22515, 22516, 22517, 22518];

        // Npc: 1012118, 1022104, 1032113, 1052113, 1095001
        [ScriptName("n1012118", "n1022104", "n1032113", "n1052113", "n1095001")]
        public async Task TrainingCenter()
        {
            if (getLevel() >= 20)
            {
                await SayOK("只有低于20级才能进入训练中心。");
                return;
            }

            // 仅 1012118 有特殊训练中心分支
            var hasSpecialBranch = getNpc() == 1012118;
            if (hasSpecialBranch && BeginnerTutorialQuests.Any(isQuestActive))
            {
                if (await AskYesNo("你要进入特殊的训练中心吗？"))
                {
                    await warp(TRAINING_SPECIAL_MAP, 0);
                }

                return;
            }

            var baseMap = getNpc() switch
            {
                1012118 => 910060000,
                1022104 => 910220000,
                1032113 => 910120000,
                1052113 => 910310000,
                _ => 912030000,
            };

            var menu = "你要进入训练中心吗？";
            for (var i = 0; i < TRAINING_ROOMS; i++)
            {
                menu += $"\r\n#b#L{i}#训练中心 {i} ({await getPlayerCount(baseMap + i)}/{TRAINING_MAX_PLAYERS})#l#k";
            }

            var sel = await AskMenu(menu);
            if (sel < 0 || sel >= TRAINING_ROOMS)
            {
                return;
            }

            if (await getPlayerCount(baseMap + sel) >= TRAINING_MAX_PLAYERS)
            {
                await SayNext("里面满人了，换个训练中心或频道试试。");
                return;
            }

            await warp(baseMap + sel, 0);
        }

        // Npc: 2100002, 2100003
        /// <summary>JS: <c>cm.openShopNPC(getNpc())</c> —— 直接打开自身关联的商店。</summary>
        [ScriptName("n2100002", "n2100003")]
        public async Task OpenOwnShop()
        {
            await openShopNPC(getNpc());
        }

        // Npc: 11000
        /// <summary>JS 直接取商店 11000 并发送（绕过正常 NPC 商店流程）。</summary>
        [ScriptName("n11000")]
        public async Task OpenShop11000()
        {
            await openShopNPC(11000);
        }

        // Npc: 9201079
        [ScriptName("n9201079")]
        public async Task WarpTo682000100()
        {
            await warp(682000100, 0);
        }

        // Npc: 2101010
        /// <summary>JS: <c>cm.warp(670010200, 3); cm.changeMusic("Bgm14/Ariant")</c></summary>
        [ScriptName("n2101010")]
        public async Task WarpToAmoriaAltar()
        {
            await warp(670010200, 3);
            await changeMusic("Bgm14/Ariant");
        }

        // Npc: 2110002
        [ScriptName("n2110002")]
        public async Task ShowPartyClearEffect()
        {
            await ShowEffect("quest/party/clear");
        }

        // Npc: 1013001
        /// <summary>龙之大师引导（两页对白后传送到龙蛋房）。</summary>
        [ScriptName("n1013001")]
        public async Task DragonMasterIntro()
        {
            await SayNext("你，注定成为龙之大师的人……你终于来了。");
            await SayNext("去履行你作为龙之主的职责吧...");
            await warp(900090101, 0);
        }

        // Npc: 1013002
        [ScriptName("n1013002")]
        public async Task DragonEggObtained()
        {
            await forceCompleteQuest(22011);
            await mapMessage(5, "你获得了龙蛋。");
            await warp(900090103, 0);
        }

        // Npc: 1013104
        [ScriptName("n1013104")]
        public async Task DragonEggPickup()
        {
            if (!isQuestStarted(22007))
            {
                await SayNext("#b(你现在不需要拿蛋。)#k");
                return;
            }

            if (!haveItem(4032451))
            {
                await gainItem(4032451, 1, true);
                await SayNext("#b(你获得了一个蛋。把它交给尤塔。)");
            }
            else
            {
                await SayNext("#b(你已经获得了一个蛋。拿出你手上的蛋，然后把它交给尤塔。)");
            }
        }

        // Npc: 1013200
        [ScriptName("n1013200")]
        public async Task RescueBabyPig()
        {
            if (!isQuestStarted(22015))
            {
                await SayOK("#b(你离小猪太远了。靠近一点抓住它。)");
                return;
            }

            await gainItem(4032449, 1, true);
            await forceCompleteQuest(22015);
            await mapMessage(5, "你救了小猪。");
        }

        // Npc: 1096010
        [ScriptName("n1096010")]
        public async Task IgnitionDevicePickup()
        {
            if (!isQuestStarted(2566))
            {
                return;
            }

            if (haveItem(4032985))
            {
                await EarnTitle("你已经拿过点火装置了。");
                return;
            }

            if (canHold(4032985))
            {
                await gainItem(4032985, 1, true);
                await EarnTitle("你找到了点火装置。把它交给 Cutter。");
            }
        }

        // Npc: 2082014
        /// <summary>飞船上的 NPC：开启了自定义 NPC 脚本时直接开店，否则按任务状态给提示或默认对话。</summary>
        [ScriptName("n2082014")]
        public async Task AirshipSibling()
        {
            if (Application.Utility.Configs.YamlConfig.config.server.USE_ENABLE_CUSTOM_NPC_SCRIPT)
            {
                await openShopNPC(2082014);
            }
            else if (isQuestStarted(3749))
            {
                await SayOK("我们已经找到了敌人的终极武器！沿着船头区域前进，你会找到我的妹妹 #b#p2082013##k。向她报告，获取有关任务的进一步指示。");
            }
            else
            {
                await sendDefault();
            }
        }
    }
}
