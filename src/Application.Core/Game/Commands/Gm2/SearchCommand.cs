using Application.Core.Channel.DataProviders;
using Application.Core.scripting.npc;
using Application.Resources;
using Application.Templates.Providers;
using Application.Templates.String;
using Application.Templates.XmlWzReader.Provider;
using server.quest;
using System.Diagnostics;
using System.Text;

namespace Application.Core.Game.Commands.Gm2;

public class SearchCommand : CommandBase
{
    readonly StringProvider _stringProvider = ProviderFactory.GetProvider<StringProvider>();
    public SearchCommand() : base(2, "search")
    {
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

        if (Enum.TryParse<StringCategory>(paramsValue[0], true, out var type) || paramsValue[0].Equals("QUEST", StringComparison.OrdinalIgnoreCase))
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();

            if (paramsValue[0].Equals("QUEST", StringComparison.OrdinalIgnoreCase))
            {
                foreach (Quest mq in QuestFactory.Instance.getMatchedQuests(search))
                {
                    sb.Append("#b").Append(mq.getId()).Append("#k - #r");
                    if (!string.IsNullOrEmpty(mq.Parent))
                    {
                        sb.Append(mq.Parent).Append(" - ");
                    }
                    sb.Append(mq.Name).Append("\r\n");
                }
            }
            else
            {
                foreach (var item in _stringProvider.Search(type, search))
                {
                    if (item is StringMapTemplate mapData)
                    {
                        sb.Append("#b").Append(item.TemplateId).Append("#k - #r").Append(mapData.StreetName).Append(" - ").Append(mapData.MapName).Append("\r\n");
                    }
                    else if (item is StringTemplate stringData)
                    {
                        sb.Append("#b").Append(item.TemplateId).Append("#k - #r").Append(stringData.Name).Append("\r\n");
                    }
                    else if (item is StringNpcTemplate npcData)
                    {
                        sb.Append("#b").Append(item.TemplateId).Append("#k - #r").Append(npcData.Name).Append("\r\n");
                    }
                }
            }

            sw.Stop();
            sb.Append("\r\n#kLoaded within ").Append(sw.Elapsed.TotalSeconds).Append(" seconds.");//because I can, and it's free
        }
        else
        {
            sb.Append("#bInvalid search.\r\nSyntax: '!search [type] [name]', where [type] is MAP, QUEST, NPC, ITEM, MOB, or SKILL.");

        }

        if (sb.Length == 0)
        {
            sb.Append("#bNo ").Append(paramsValue[0].ToLower()).Append("s found.\r\n");
        }


        TempConversation.Create(c)?.RegisterTalk(sb.ToString());
    }
}
