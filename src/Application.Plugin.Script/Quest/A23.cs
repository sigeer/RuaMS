namespace Application.Plugin.Script.Quest
{
    /// <summary>
    /// 蘑菇王国主线任务 2312 ~ 2321（史莱姆菇/奇拉蘑菇孢子线）。
    /// 对应 JS scripts/quest/{2312,2313,2315,2316,2317,2318,2319,2320,2321}.js。
    /// 这些任务已有 <c>Quest.wz</c> 记录但未声明 startscript/endscript，故运行期不会分发；
    /// 按「文件全覆盖」口径补齐。
    /// </summary>
    internal partial class QuestScript
    {
        // Quest: 2312
        public async Task q2312s()
        {
            if (await SayAcceptDecline("勇士，现在我们王国正面临建国以来最大的危机，所以我们需要强壮的勇士。你是得到推荐之后过来的吧？但是我们需要判断一下你是否是我们王国需要的勇士。不好意思，我能请你完成一件事吗？"))
            {
                await forceStartQuest();
                await SayOK("从这里往前走，可以看到很多背叛了蘑菇王国的#b得意的蘑菇仔#k。他们是背叛了我们王国，投降了敌人的人。请你去教训教训他们，并搜集#b50个#k#v4000499##t4000499##b#k作为证据。");
            }
            else
            {
                await SayOK("嗯……如果你没有自信的话，随时可以再来找我。");
            }
        }

        // Quest: 2312
        public async Task q2312e()
        {
            if (!await SayAcceptDecline("你帮我教训了得意的蘑菇仔了吗？"))
                return;

            await forceCompleteQuest();
            await gainExp(11500);
            await gainItem(4000499, -50);
            await SayOK("真了不起。不好意思，我不该怀疑你的能力。请你务必把我们蘑菇王国从危机中拯救出来。");
        }

        // Quest: 2313
        public async Task q2313s()
        {
            if (await SayAcceptDecline("我已经把你的事情跟我们的#b内务大臣#k说了。请你去见见#b内务大臣#k。"))
            {
                await forceStartQuest();
                await SayOK("请你一定要拯救我们王国！");
            }
            else
            {
                await SayOK("没有时间了……请你快一点。");
            }
        }

        // Quest: 2313
        public async Task q2313e()
        {
            await forceCompleteQuest();
            await gainExp(4000);
        }

        // Quest: 2315
        public async Task q2315s()
        {
            if (await SayAcceptDecline("强力的魔法结界……这可怎么办呢……如果不能穿过结界，就无法救出公主。既然你说用物理的力量无法消除结界，我想最好去问问#b魔法大臣#k。"))
            {
                await forceStartQuest();
                await SayOK("请你马上去见见#b魔法大臣#k。他的性格虽然有些古怪，但他是个知识渊博的人，也许会知道些什么。");
            }
            else
            {
                await SayOK("希望你不要放弃，勇士。");
            }
        }

        // Quest: 2315
        public async Task q2315e()
        {
            if (!await SayAcceptDecline("什么？你对蘑菇森林的结界进行了调查？"))
                return;

            await forceCompleteQuest();
            await gainExp(4000);
            await SayOK("嗯……有意思。如果是有人用强力的魔法生成的结界，靠人力是无法穿越的。");
        }

        // Quest: 2316
        public async Task q2316s()
        {
            if (await SayAcceptDecline("我曾经听说有种药品可以解开这种类型的结界。好像叫#b奇拉蘑菇孢子#k？嗯……外面的#b斯卡斯#k是蘑菇专家，你去问问#b斯卡斯#k吧。"))
            {
                await forceStartQuest();
                await SayOK("#b斯卡斯#k应该可以帮助你。");
            }
            else
            {
                await SayOK("既然你愿意，那为什么还要犹豫呢？勇敢的家伙……");
            }
        }

        // Quest: 2316
        public async Task q2316e()
        {
            if (!await SayAcceptDecline("啊！你就是那个勇士吧。我是蘑菇王国的#b王室蘑菇研究家斯卡斯#k。嗯？你需要#b奇拉蘑菇孢子#k？"))
                return;

            await forceCompleteQuest();
            await gainExp(4200);
            await SayOK("#b奇拉蘑菇孢子#k嘛……好像从哪里听说过。");
        }

        // Quest: 2317
        public async Task q2317s()
        {
            if (await SayAcceptDecline("啊！#b奇拉蘑菇孢子#k嘛，小时候我好像在书里看到过。想起来了。那是用紫色毒蘑菇中提取的强力毒素制成的，所以需要紫色毒蘑菇盖。如果你能帮我搜集紫色毒蘑菇盖，我就能帮你制作。"))
            {
                await forceStartQuest();
                await SayOK("请你去打猎#b紫色毒蘑菇#k，搜集100个#b紫色毒蘑菇盖#k。");
            }
            else
            {
                await SayOK("要穿过结界，就需要紫色毒蘑菇盖。想好的话再来找我。");
            }
        }

        // Quest: 2317
        public async Task q2317e()
        {
            await SayOK("100个紫色毒蘑菇盖全部搜集到了吗？");
            await SayOK("紫色毒蘑菇盖很难搜集到，没想到你这么快就搜集到了100个。这样应该就能制作出#b奇拉蘑菇孢子#k了。");
            await forceCompleteQuest();
            await gainExp(13500);
            await gainItem(4000500, -100);
        }

        // Quest: 2318
        public async Task q2318s()
        {
            if (await SayAcceptDecline("嗯……在你搜集紫色毒蘑菇盖的时间里，我查了一下，我还需要另外的材料。请你再去帮我搜集一种材料。"))
            {
                await forceStartQuest();
                await SayOK("很好。请你帮我去打猎得意的蘑菇仔，搜集#b50个#t4000499##k。");
            }
            else
            {
                await SayOK("我知道这不容易，但没有材料的话，就无法制作#b奇拉蘑菇孢子#k。请再好好考虑一下。");
            }
        }

        // Quest: 2318
        public async Task q2318e()
        {
            await SayOK("需要的材料全部搜集到了吗？");

            if (!haveItem(4000499, 50))
            {
                await SayOK("嗯？你好像还没搜集到#b50个#t4000499##k啊。我需要#b50个#t4000499##k。");
                return;
            }

            await SayNext("好的！有了它的话，我就能制作#b奇拉蘑菇孢子#k了。请稍等一下。");
            await SayOK("给，奇拉蘑菇孢子做好了。希望它能对你起到一点帮助，帮助你救出公主，夺回王宫。加油，勇士！");
            await forceCompleteQuest();
            await gainExp(11500);
            await gainItem(4000499, -50);
            await gainItem(2430014, 1);
        }

        // Quest: 2319
        public async Task q2319s()
        {
            if (await SayAcceptDecline("啊！差点忘了！看我的记性。请你把做好的#b#t4032389##k交给#b#p1300004##k，并向他报告。"))
            {
                await forceStartQuest();
                await gainItem(4032389, 1);
                await SayOK("#b魔法大臣#k让我制作好#b奇拉蘑菇孢子#k之后，给他送一份样本过去。这是样本，请你把它交给#b魔法大臣#k。");
            }
            else
            {
                await SayOK("你应该还有别的事情要办吧？想好的话再来找我。");
            }
        }

        // Quest: 2319
        public async Task q2319e()
        {
            if (!await SayAcceptDecline("#b奇拉蘑菇孢子#k做好了吗？"))
                return;

            await forceCompleteQuest();
            await gainExp(4200);
            await gainItem(4032389, -1);
            await SayOK("嗯……这就是#b奇拉蘑菇孢子#k啊。谢谢你。帮我谢谢#b斯卡斯#k。");
        }

        // Quest: 2320
        public async Task q2320s()
        {
            if (await SayAcceptDecline("我还有一个请求，你愿意帮我吗？"))
            {
                await forceStartQuest();
                await gainItem(4032389, 1);
                await SayOK("事实上，#b奇拉蘑菇孢子#k是我的研究成果。你知道#b射手村#k的#b布鲁斯#k吗？他是我小时候的好朋友，他把他的研究成果告诉了我，我才制作出了#b奇拉蘑菇孢子#k。我能取得这样的成果，多亏了他的帮助。请你帮我把它交给布鲁斯。");
            }
            else
            {
                await SayOK("我实在不好意思再麻烦你，所以只能等你愿意的时候再说。");
            }
        }

        // Quest: 2320
        public async Task q2320e()
        {
            await SayOK("哦！是#b斯卡斯#k让你来的吗？ \r\n\r\n#fUI/UIWindow.img/QuestIcon/4/0# \r\n#fUI/UIWindow.img/QuestIcon/8/0# 8800 经验");
            await forceCompleteQuest();
            await gainExp(8800);
            await gainItem(4032389, -1);
            await SayOK("呵呵，这是我过去研究的#b奇拉蘑菇孢子#k啊。由于材料很难搜集，所以我只停留在理论层面，没想到被那个家伙做出来了。请你帮我谢谢他。");
        }

        // Quest: 2321
        public async Task q2321s()
        {
            if (await SayAcceptDecline("我们也得开始准备了。还要把这件事告诉#p1300005#。我先告诉#p1300005#后再跟你说吧。"))
            {
                await forceStartQuest();
                await SayOK("祝你好运。");
            }
            else
            {
                await SayOK("你是个勇敢的孩子。想好的话再来找我。");
            }
        }

        // Quest: 2321
        public async Task q2321e()
        {
            await SayOK("我一直听闻勇士你的丰功伟绩，据说你用一种叫#b奇拉蘑菇孢子#k的神秘药物打破了树林的结界？真是了不起啊！");
            await forceCompleteQuest();
            await gainExp(2500);
            await SayOK("接下去的问题，就是怎么进入蘑菇城了。");
        }
    }
}
