using Application.Core.Channel.Services;
using Application.Core.scripting.npc;
using Application.Resources.Messages;
using System.Text;

namespace Application.Core.Game.Commands.Gm0;

public class OnlineCommand : CommandBase
{
    readonly AdminService _adminService;
    public OnlineCommand(AdminService adminService) : base(0, "online")
    {
        _adminService = adminService;
    }

    public override Task Execute(IChannelClient c, string[] paramsValue)
    {
        var player = c.OnlinedCharacter;

        var channelGroup = _adminService.GetOnlinedPlayers().GroupBy(x => x.Channel).OrderBy(x => x.Key).ToList();

        StringBuilder sb = new StringBuilder();

        List<SystemProto.OnlinedPlayerInfoDto> list = [];
        int i = 0;
        foreach (var item in channelGroup)
        {
            sb.Append($"===> {c.CurrentCulture.GetMessageByKey(nameof(ClientMessage.Channel))}{item.Key}：\r\n");
            foreach (var chr in item)
            {
                sb.Append($"\r\n#L{i}# {chr.Name}：{c.CurrentCulture.GetMapName(chr.MapId)} - {c.CurrentCulture.GetMapStreetName(chr.MapId)} #l\r\n");
                list.Add(chr);
                i++;
            }
        }

        TempConversation.Create(c)?.RegisterSelect(sb.ToString(), (idx, ctx) =>
        {
            var item = list[idx];
            if (item.Id == c.OnlinedCharacter.Id)
            {
                ctx.RegisterTalk(c.CurrentCulture.GetMessageByKey(nameof(ClientMessage.CannotWarpMySelf)));
            }
            else
            {
                ctx.RegisterYesOrNo(c.CurrentCulture.GetMessageByKey(nameof(ClientMessage.ConfirmWarpTo), item.Name), async ctx =>
                {
                   await _adminService.WarpPlayerByName(c.OnlinedCharacter, item.Name);
                    list.Clear();
                    ctx.dispose();
                });
            }
        });
        return Task.CompletedTask;
    }
}
