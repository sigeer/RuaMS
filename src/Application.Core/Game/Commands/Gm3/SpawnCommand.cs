using Application.Core.Channel.ServerData;
using Application.Core.scripting.npc;
using Application.Resources.Messages;
using server.life;
using System.Text;

namespace Application.Core.Game.Commands.Gm3;

public class SpawnCommand : CommandBase
{
    readonly WzStringQueryService _wzManager;
    public SpawnCommand(WzStringQueryService wzManager) : base(3, "spawn")
    {
        _wzManager = wzManager;
    }

    public override async Task Execute(IChannelClient c, string[] paramsValue)
    {
        var player = c.OnlinedCharacter;
        if (paramsValue.Length < 1 || paramsValue.Length > 2)
        {
            await player.Yellow(nameof(ClientMessage.SpawnCommand_Syntax));
            return;
        }

        if (!int.TryParse(paramsValue[0], out var mobId))
        {
            var list = _wzManager.FindMobIdByName(c, paramsValue[0]);
            if (list.BestMatch != null)
            {
                mobId = list.BestMatch.Id;
            }
            else if (list.MatchedItems.Count > 0)
            {
                var messages = new StringBuilder();
                messages.Append(player.GetMessageByKey(nameof(ClientMessage.FindSimilarItem))).Append("\r\n");
                for (int i = 0; i < list.MatchedItems.Count; i++)
                {
                    var item = list.MatchedItems[i];
                    messages.Append($"\r\n#L{i}# {item.Id} - {item.Name} #l");
                }

                await TempConversation.CreateScope(c, async ctx =>
                 {
                     var idx = await ctx.AskMenu(messages.ToString());
                     var item = list.MatchedItems[idx];
                     if (await ctx.AskYesNo($"你确定要召唤 {item.Id} - {item.Name}？"))
                     {
                         await SpawnById(player, item.Id, paramsValue);
                     }
                 });
                return;
            }
            else
            {
                await player.Yellow(nameof(ClientMessage.MobNotFound), paramsValue[0]);
                return;
            }

        }

        await SpawnById(player, mobId, paramsValue);
    }

    private async Task SpawnById(Player player, int mobId, string[] paramsValue)
    {
        int monsterCount = paramsValue.Length != 2 ? 1 : (int.TryParse(paramsValue[1], out var d) ? d : 1);
        if (monsterCount < 1)
        {
            return;
        }

        for (int i = 0; i < monsterCount; i++)
        {
            await player.getMap().spawnMonsterOnGroundBelow(LifeFactory.Instance.GetMonsterTrust(mobId), player.getPosition());
        }
    }
}
