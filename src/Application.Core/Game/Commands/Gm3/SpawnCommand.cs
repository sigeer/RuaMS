using Application.Core.Channel.ServerData;
using Application.Core.scripting.npc;
using server.life;
using System.Text;

namespace Application.Core.Game.Commands.Gm3;

public class SpawnCommand : CommandBase
{
    readonly WzStringQueryService _wzManager;
    public SpawnCommand(WzStringQueryService wzManager) : base(3, "spawn")
    {
        _wzManager = wzManager;
        Description = "Spawn mob(s) on your location.";
    }

    public override void Execute(IChannelClient c, string[] paramsValue)
    {
        var player = c.OnlinedCharacter;
        if (paramsValue.Length < 1 || paramsValue.Length > 2)
        {
            player.yellowMessage("Syntax: !spawn <mobid> [<mobqty>]");
            return;
        }

        if (!int.TryParse(paramsValue[0], out var mobId))
        {
            var list = _wzManager.FindMobIdByName(paramsValue[0]);
            if (list.BestMatch != null)
            {
                mobId = list.BestMatch.Id;
            }
            else if (list.MatchedItems.Count > 0)
            {
                var messages = new StringBuilder("找到了这些相似项：\r\n");
                for (int i = 0; i < list.MatchedItems.Count; i++)
                {
                    var item = list.MatchedItems[i];
                    messages.Append($"\r\n#L{i}# {item.Id} - {item.Name} #l");
                }

                TempConversation.Create(c, NpcId.MAPLE_ADMINISTRATOR)?
                    .RegisterSelect(messages.ToString(), (idx, ctx) =>
                    {
                        var item = list.MatchedItems[idx];
                        ctx.RegisterYesOrNo($"你确定要召唤 {item.Id} - {item.Name}？", ctx =>
                        {
                            SpawnById(player, item.Id, paramsValue);
                            ctx.dispose();
                        });
                    });
                return;
            }
            else
            {
                player.yellowMessage("Syntax: <mobid> invalid");
                return;
            }

        }

        SpawnById(player, mobId, paramsValue);
    }

    private void SpawnById(IPlayer player, int mobId, string[] paramsValue)
    {
        int monsterCount = paramsValue.Length != 2 ? 1 : (int.TryParse(paramsValue[1], out var d) ? d : 1);
        if (monsterCount < 1)
        {
            return;
        }

        for (int i = 0; i < monsterCount; i++)
        {
            player.getMap().spawnMonsterOnGroundBelow(LifeFactory.Instance.getMonster(mobId), player.getPosition());
        }
    }
}
