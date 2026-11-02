using Application.Core.Client;

namespace Application.Plugin.Script.Features.Quest
{
    internal sealed class Q3933QuestHandler : Application.Core.scripting.quest.QuestScriptBase
    {
        public Q3933QuestHandler(IChannelClient c, Application.Core.Server.quest.Quest quest, int npc)
            : base(c, quest, npc)
        {
        }

        // Quest: 3933 
        public async Task q3933s()
        {
            await SayNext("没想到你会这么的强…以你的水平也许可以成为沙子图团的团员也说不定。对沙子图团员来说，最重要的就是力量的强大，而你…看来已经具备了足够的实力！但我还是要再进行一次测试…如何？可以接受吗？");
            if (await SayAcceptDecline("若想要实际测试你的力量，应该需要亲自去体验吧？我想和你进行一场对战！别担心，我也不想伤害你…就用我的分身来对付你好了！可以马上进行对战吗？"))
            {
                await SayNext("很好，我喜欢你的自信。");
                await TryStartSoloQuest(getQuest(), forceStartQuest);
            }
        }
    }
}
