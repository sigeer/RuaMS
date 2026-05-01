using Application.Shared.GameProps;

namespace Application.Plugin.Script.Quest
{

    // 时间神殿
    internal partial class QuestScript
    {
        // Quest: 3514 
        public async Task q3514s()
        {
            if (getPlayer().getMeso() >= 1000000)
            {
                if (canHold(2022337, 1))
                {
                    gainItem(2022337, 1);
                    gainMeso(-1000000);

                    //await SayOK("Nice doing business with you~~.");
                    startQuest(3514);
                }
                else
                {
                    await SayOK("保你的消耗栏有空位.");
                }
            }
            else
            {
                await SayOK("喂，你没有足够的钱。我出售情感药剂需要 #r1,000,000金币#k。没钱就算了.");
            }
        }
        // Quest: 3514 
        public async Task q3514e()
        {
            if (getPlayer().getBuffSource(BuffStat.HPREC) != 2022337)
            {
                if (haveItem(2022337))
                {
                    await SayOK("你害怕喝这个药剂吗？我可以向你保证它只有轻微的 #r副作用#k.");
                }
                else
                {
                    if (canHold(2022337))
                    {
                        gainItem(2022337, 1);
                        await SayOK("丢了吗？幸运的是我成功找回了。拿去吧.");
                    }
                    else
                    {
                        await SayOK("丢了吗？幸运的是我成功找回了。请腾出空间来获取它.");
                    }
                }
                return;
            }
            else
            {
                await SayOK("你的情绪不再冻结了。哦，天啊... 你身体状况不佳，#b快速排毒#k.");

                gainExp(891500);
                completeQuest(3514);
            }

        }
        // Quest: 3523 
        public async Task q3523s()
        {
            startQuest();
            setQuestProgress(3507, 7081, 1); // thanks resinate for pointing out uncompletable quest due to non-updated progress
            completeQuest();
            await SayOK("啊，原来是你。没想到很久之后还能看到你。我很高兴看到曾经是青涩的新手的你成为了出色的战士。看到很久不见但还记得我的你，我的心里充满了温暖。你是在寻找遗忘的记忆吗？想起来那已经是很久很久以前的事了，事隔多年，真是让人怀念啊。这样吧。你再去找#b#p2140001##k吧。相信他会帮助你。那么再见……");
        }
        // Quest: 3524 
        public async Task q3524s()
        {
            startQuest();
            setQuestProgress(3507, 7081, 1);
            completeQuest();
            await SayOK("啊，原来是你。没想到很久之后还能看到你。我很高兴看到曾经是青涩的新手的你成为了大魔法师。看到很久不见但还记得我的你，我的心里充满了温暖。你是在寻找遗忘的记忆吗？想起来那已经是很久很久以前的事了，事隔多年，真是让人怀念啊。这样吧。你再去找#b#p2140001##k吧。相信他会帮助你。那么再见……");
        }
        // Quest: 3525 
        public async Task q3525s()
        {
            startQuest();
            setQuestProgress(3507, 7081, 1);
            completeQuest();
            await SayOK("啊，原来是你。没想到很久之后还能看到你。我很高兴看到曾经是青涩的新手的你成为了大魔法师。看到很久不见但还记得我的你，我的心里充满了温暖。你是在寻找遗忘的记忆吗？想起来那已经是很久很久以前的事了，事隔多年，真是让人怀念啊。这样吧。你再去找#b#p2140001##k吧。相信他会帮助你。那么再见……");
        }
        // Quest: 3526 
        public async Task q3526s()
        {
            startQuest();
            setQuestProgress(3507, 7081, 1);
            completeQuest();
            await SayOK("啊，原来是你。没想到很久之后还能看到你。我很高兴看到曾经是青涩的新手的你成为了大魔法师。看到很久不见但还记得我的你，我的心里充满了温暖。你是在寻找遗忘的记忆吗？想起来那已经是很久很久以前的事了，事隔多年，真是让人怀念啊。这样吧。你再去找#b#p2140001##k吧。相信他会帮助你。那么再见……");
        }
        // Quest: 3527 
        public async Task q3527s()
        {
            startQuest();
            setQuestProgress(3507, 7081, 1);
            completeQuest();
            await SayOK("啊，原来是你。没想到很久之后还能看到你。我很高兴看到曾经是青涩的新手的你成为了大魔法师。看到很久不见但还记得我的你，我的心里充满了温暖。你是在寻找遗忘的记忆吗？想起来那已经是很久很久以前的事了，事隔多年，真是让人怀念啊。这样吧。你再去找#b#p2140001##k吧。相信他会帮助你。那么再见……");
        }
        // Quest: 3529 
        public async Task q3529s()
        {
            await SayOK("'啊，原来是你。没想到很久之后还能看到你。我很高兴看到曾经是青涩的新手的你成为了大魔法师。看到很久不见但还记得我的你，我的心里充满了温暖。你是在寻找遗忘的记忆吗？想起来那已经是很久很久以前的事了，事隔多年，真是让人怀念啊。这样吧。你再去找#b#p2140001##k吧。相信他会帮助你。那么再见……");
            setQuestProgress(3507, 7081, 1);
            forceCompleteQuest();
        }
        // Quest: 3539 
        public async Task q3539s()
        {
            startQuest();
            setQuestProgress(3507, 7081, 1);
            completeQuest();
            await SayOK("啊，原来是你。没想到很久之后还能看到你。我很高兴看到曾经是青涩的新手的你成为了出色的战士。看到很久不见但还记得我的你，我的心里充满了温暖。你是在寻找遗忘的记忆吗？想起来那已经是很久很久以前的事了，事隔多年，真是让人怀念啊。这样吧。你再去找#b#p2140001##k吧。相信他会帮助你。那么再见……");
        }

    }
}