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
                await SayNext("这是您的第一个训练课程开始的图像室。在这个房间里，您将提前了解您选择的职业。");
                await SayNext("一旦你训练得足够努力，你就有资格获得一份工作。你可以成为赫内西斯的弓箭手，艾琳尼亚的魔法师，佩里恩的战士，以及科尔宁城的盗贼……");
                return;
            }

            var enterCamp = await AskYesNo("欢迎来到冒险岛的世界。这个训练营的目的是帮助新手。你想要进入这个训练营吗？有些人在开始他们的冒险之旅时并没有参加训练计划。但我强烈建议你先参加训练计划。");
            if (enterCamp)
            {
                await SayNext("好的，那么，我会让你进入训练营地。请跟着你的教练。");
                await warp(1, 0);
                return;
            }

            if (await AskYesNo("你真的想立刻开始你的冒险吗？"))
            {
                await SayNext("看起来你想要开始你的冒险之旅而不接受训练计划。那么，我会让你前往训练场地。小心~");
                await warp(40000, 0);
                return;
            }

            await SayNext("请在你最终做出决定后再和我交谈。");
        }

        // Npc: 2101
        /// <summary>训练营出口：确认训练完成后送回出生地图；在 10000 号地图时只讲解操作。</summary>
        [ScriptName("n2101")]
        public async Task MapleIslandTrainingCampExit()
        {
            if (getMapId() == 10000)
            {
                await SayOK("您是一位新的冒险家，我会为你提供一些非常有用指示。如果您想与我们说话，你可以双击我们。您可以按#b左箭头和右箭头#k进行移动，通过按#bAlt#k来跳跃。来吧~试试吧！另外，有时候你需要爬梯子或使用绳子到达你想去的地方。你可以通过按#b上箭头#k来做到这一点。请记住这一点。");
                return;
            }

            if (!await AskYesNo("你的训练完成了吗？如果你愿意的话，我会让你离开这个训练营。"))
            {
                await SayOK("你还没完成训练计划吗？如果你想离开这个地方，请毫不犹豫地告诉我。");
                return;
            }

            await SayNext("接下来，我会把你从这里送出去，祝您一路顺风。");
            await warp(0);
        }
    }
}
