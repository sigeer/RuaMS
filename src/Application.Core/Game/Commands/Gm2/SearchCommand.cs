using Application.Core.scripting.npc;
using Application.Resources;
using server.quest;
using System.Diagnostics;
using System.Text;

namespace Application.Core.Game.Commands.Gm2;

public class SearchCommand : CommandBase
{
    readonly WzStringProvider _wzStringProvider;
    public SearchCommand(WzStringProvider wzStringProvider) : base(2, "search")
    {
        _wzStringProvider = wzStringProvider;
        Description = "Search string.wz.";
    }

    public override void Execute(IChannelClient c, string[] paramsValue)
    {
        var player = c.OnlinedCharacter;
        if (paramsValue.Length < 2)
        {
            player.yellowMessage("Syntax: !search <type> <name>");
            return;
        }
        StringBuilder sb = new StringBuilder();

        string search = joinStringFrom(paramsValue, 1);
        Stopwatch sw = new Stopwatch();
        sw.Start();
        if (paramsValue[0].Equals("ITEM", StringComparison.OrdinalIgnoreCase))
        {
            foreach (var item in _wzStringProvider.GetAllItem().Where(x => x.Name.Contains(search, StringComparison.OrdinalIgnoreCase)).Take(50))
            {
                sb.Append("#b").Append(item.Id).Append("#k - #r").Append(item.Name).Append("\r\n");
            }
        }
        else if (paramsValue[0].Equals("NPC", StringComparison.OrdinalIgnoreCase))
        {
            foreach (var item in _wzStringProvider.GetAllNpcList().Where(x => x.Name.Contains(search, StringComparison.OrdinalIgnoreCase)).Take(50))
            {
                sb.Append("#b").Append(item.Id).Append("#k - #r").Append(item.Name).Append("\r\n");
            }
        }
        else if (paramsValue[0].Equals("MOB", StringComparison.OrdinalIgnoreCase) || paramsValue[0].Equals("MONSTER", StringComparison.OrdinalIgnoreCase))
        {
            foreach (var item in _wzStringProvider.GetAllMonster().Where(x => x.Name.Contains(search, StringComparison.OrdinalIgnoreCase)).Take(50))
            {
                sb.Append("#b").Append(item.Id).Append("#k - #r").Append(item.Name).Append("\r\n");
            }
        }
        else if (paramsValue[0].Equals("SKILL", StringComparison.OrdinalIgnoreCase))
        {
            foreach (var item in _wzStringProvider.GetAllSkillList().Where(x => x.Name.Contains(search, StringComparison.OrdinalIgnoreCase)).Take(50))
            {
                sb.Append("#b").Append(item.Id).Append("#k - #r").Append(item.Name).Append("\r\n");
            }
        }
        else if (paramsValue[0].Equals("MAP", StringComparison.OrdinalIgnoreCase))
        {
            foreach (var item in _wzStringProvider.GetAllMap().Where(x => x.PlaceName.Contains(search, StringComparison.OrdinalIgnoreCase)
                || x.StreetName.Contains(search, StringComparison.OrdinalIgnoreCase)).Take(50))
            {
                sb.Append("#b").Append(item.Id).Append("#k - #r").Append(item.StreetName).Append(" - ").Append(item.PlaceName).Append("\r\n");
            }
        }
        else if (paramsValue[0].Equals("QUEST", StringComparison.OrdinalIgnoreCase))
        {
            foreach (Quest mq in Quest.getMatchedQuests(search))
            {
                sb.Append("#b").Append(mq.getId()).Append("#k - #r");

                string parentName = mq.getParentName();
                if (parentName.Count() > 0)
                {
                    sb.Append(parentName).Append(" - ");
                }
                sb.Append(mq.getName()).Append("\r\n");
            }
        }
        else
        {
            sb.Append("#bInvalid search.\r\nSyntax: '!search [type] [name]', where [type] is MAP, QUEST, NPC, ITEM, MOB, or SKILL.");
        }
        if (sb.Length == 0)
        {
            sb.Append("#bNo ").Append(paramsValue[0].ToLower()).Append("s found.\r\n");
        }
        sw.Stop();
        sb.Append("\r\n#kLoaded within ").Append(sw.Elapsed.TotalSeconds).Append(" seconds.");//because I can, and it's free

        TempConversation.Create(c)?.RegisterTalk(sb.ToString());
    }
}
