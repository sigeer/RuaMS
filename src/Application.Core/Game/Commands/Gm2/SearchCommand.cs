using Application.Resources.Messages;
using Application.Templates.String;
using System.Diagnostics;
using System.Text;

namespace Application.Core.Game.Commands.Gm2;

public class SearchCommand : CommandBase
{
    public SearchCommand() : base(2, "search")
    {
    }

    public override async Task Execute(IChannelClient c, string[] paramsValue)
    {
        var player = c.OnlinedCharacter;
        if (paramsValue.Length < 2)
        {
            await player.Yellow(nameof(ClientMessage.SearchCommand_Syntax));
            return;
        }
        StringBuilder sb = new StringBuilder();

        string search = joinStringFrom(paramsValue, 1);

        if (Enum.TryParse<StringCategory>(paramsValue[0], true, out var type))
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();

            foreach (var item in c.CurrentCulture.StringProvider.Search(type, search))
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
                else if (item is StringQuestTemplate questData)
                {
                    sb.Append("#b").Append(questData.TemplateId).Append("#k - #r");
                    if (!string.IsNullOrEmpty(questData.ParentName))
                    {
                        sb.Append(questData.ParentName).Append(" - ");
                    }
                    sb.Append(questData.Name).Append("\r\n");
                }
            }

            sw.Stop();
            sb.Append("\r\n").Append("#k");
            sb.Append(c.CurrentCulture.GetMessageByKey(nameof(ClientMessage.LoadedWithin), sw.Elapsed.TotalSeconds.ToString()));
        }
        else
        {
            sb.Append("#b");
            sb.Append(c.OnlinedCharacter.GetMessageByKey(nameof(ClientMessage.SearchCommand_Wrong)));
        }

        if (sb.Length == 0)
        {
            sb.Append("#bNo ").Append(paramsValue[0].ToLower()).Append("s found.\r\n");
        }

        await c.OnlinedCharacter.Dialog(sb.ToString());
    }
}
