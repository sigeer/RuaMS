using Application.Core.Channel.DataProviders;
using Application.Core.scripting.npc;
using Application.Resources.Messages;
using client.inventory.manipulator;
using server.life;
using System.Text;

namespace Application.Core.Game.Commands.Gm2
{
    public class ItemQuestCommand : ParamsCommandBase
    {
        public ItemQuestCommand() : base(["<quest>"], 2, ["itemquest"])
        {
        }

        public override void Execute(IChannelClient client, string[] values)
        {
            var questInput = GetParamByIndex(0) ?? string.Empty;

            if (!int.TryParse(questInput, out var questId))
            {
                var matchedQuestList = QuestFactory.Instance.getMatchedQuests(questInput);
                if (matchedQuestList.Count == 0)
                {
                    client.OnlinedCharacter.YellowMessageI18N(nameof(ClientMessage.QuestNotFound), questInput);
                }
                else if (matchedQuestList.Count == 1)
                {
                    questId = matchedQuestList[0].getId();
                }
                else
                {
                    var sb = new StringBuilder();
                    for (int i = 0; i < matchedQuestList.Count; i++)
                    {
                        var item = matchedQuestList[i];
                        sb.Append($"\r\n#L{i}# {item.getId()} #t{item.getId()}# - {item.Name} #l");
                    }
                    TempConversation.Create(client)?.RegisterSelect(sb.ToString(), (i, ctx) =>
                    {
                        ApplyQuestData(client, matchedQuestList[i].getId());
                    });
                }
                return;
            }

            ApplyQuestData(client, questId);
        }

        void ApplyQuestData(IChannelClient client, int questId)
        {
            var questInfo = QuestFactory.Instance.GetInstance(questId);
            if (string.IsNullOrEmpty(questInfo.Name))
            {
                return;
            }

            var itemRequirement = questInfo.GetItemRequirement();
            if (itemRequirement != null)
            {
                foreach (var item in itemRequirement.RequiredItems)
                {
                    InventoryManipulator.addById(client, item.Key, (short)item.Value, client.OnlinedCharacter.Name);
                }
            }

            var mobRequirement = questInfo.GetMobRequirement();
            if (mobRequirement != null)
            {
                foreach (var item in mobRequirement.RequiredMobs)
                {
                    for (int i = 0; i < item.Value; i++)
                    {
                        client.OnlinedCharacter.getMap().spawnMonsterOnGroundBelow(LifeFactory.Instance.getMonster(item.Value), client.OnlinedCharacter.getPosition());
                    }
                }
            }
        }
    }
}
