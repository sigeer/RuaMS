using Application.Core.Channel.DataProviders;
using Application.Core.scripting.npc;
using Application.Resources.Messages;
using Application.Templates.String;
using server.life;
using System.Text;

namespace Application.Core.Game.Commands.Gm2
{
    public class ItemQuestCommand : ParamsCommandBase
    {
        public ItemQuestCommand() : base(["<quest>"], 2, ["itemquest"])
        {
        }

        public override async Task Execute(IChannelClient client, string[] values)
        {
            var questInput = GetParamByIndex(0) ?? string.Empty;

            if (!int.TryParse(questInput, out var questId))
            {
                var matchedQuestList = client.CurrentCulture.StringProvider.GetSubProvider(Templates.String.StringCategory.Quest)
                    .Search(questInput).OfType<StringQuestTemplate>().ToList();
                if (matchedQuestList.Count == 0)
                {
                    await client.OnlinedCharacter.Yellow(nameof(ClientMessage.QuestNotFound), questInput);
                }
                else if (matchedQuestList.Count == 1)
                {
                    questId = matchedQuestList[0].TemplateId;
                }
                else
                {
                    var sb = new StringBuilder();
                    for (int i = 0; i < matchedQuestList.Count; i++)
                    {
                        var item = matchedQuestList[i];
                        sb.Append($"\r\n#L{i}# {item.TemplateId} #t{item.TemplateId}# - {item.Name} #l");
                    }
                    await TempConversation.CreateScope(client, async ctx =>
                    {
                        var i = await ctx.AskMenu(sb.ToString());
                        await ApplyQuestData(client, questInput, matchedQuestList[i].TemplateId);
                    });
                }
                return;
            }

            await ApplyQuestData(client, questInput, questId);
        }

        async Task ApplyQuestData(IChannelClient client, string input, int questId)
        {
            var questInfo = QuestFactory.Instance.GetInstance(questId);
            var itemRequirement = questInfo.GetItemRequirement();
            if (itemRequirement != null)
            {
                foreach (var item in itemRequirement.RequiredItems)
                {
                    await client.OnlinedCharacter.GainItem(item.Key, (short)item.Value);
                }
            }

            var mobRequirement = questInfo.GetMobRequirement();
            if (mobRequirement != null)
            {
                foreach (var item in mobRequirement.RequiredMobs)
                {
                    for (int i = 0; i < item.Value; i++)
                    {
                        await client.OnlinedCharacter.getMap().spawnMonsterOnGroundBelow(LifeFactory.Instance.GetMonsterTrust(item.Key), client.OnlinedCharacter.getPosition());
                    }
                }
            }
        }
    }
}
