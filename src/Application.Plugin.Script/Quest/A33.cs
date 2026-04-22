namespace Application.Plugin.Script.Quest
{
    // 冰峰雪域 + 水下世界
    internal partial class QuestScript
    {
        // Quest: 3108 
        public async Task q3108s()
        {
            await SayNext("(雕像散发出耀眼的美丽。像冰块儿一样透明，但好像又不是冰块儿。我到雕像周围仔细看了一下。)");
            forceCompleteQuest();
        }

    }
}