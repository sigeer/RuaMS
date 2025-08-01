using Application.Core.Channel.Services;
using Application.Core.scripting.npc;
using server.maps;
using System.Text;

namespace Application.Core.Game.Commands.Gm0;

public class OnlineCommand : CommandBase
{
    readonly AdminService _adminService;
    public OnlineCommand(AdminService adminService) : base(0, "online")
    {
        Description = "Show all online players.";
        _adminService = adminService;
    }

    public override void Execute(IChannelClient c, string[] paramsValue)
    {
        var player = c.OnlinedCharacter;

        var channelGroup = _adminService.GetOnlinedPlayers().GroupBy(x => x.Channel);

        StringBuilder sb = new StringBuilder();

        List<Dto.OnlinedPlayerInfoDto> list = [];
        int i = 0;
        foreach (var item in channelGroup)
        {
            sb.Append($"===> 频道{item.Key}：\r\n");
            foreach (var chr in item)
            {
                sb.Append($"\r\n#L{i}# {chr.Name}：{MapFactory.loadPlaceName(chr.MapId)} - {MapFactory.loadStreetName(chr.MapId)} #l");
                list.Add(chr);
            }
        }

        TempConversation.Create(c)?.RegisterSelect(sb.ToString(), (idx, ctx) =>
        {
            var item = list[idx];
            if (item.Id == c.OnlinedCharacter.Id)
            {
                ctx.RegisterTalk("不能传送到自己");
            }
            else
            {
                ctx.RegisterYesOrNo($"传送到 {item.Name} 身边？", ctx =>
                {
                    _adminService.WarpPlayerByName(c.OnlinedCharacter, item.Name);
                    list.Clear();
                    ctx.dispose();
                });
            }
        });
    }
}
