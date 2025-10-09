using Application.Core.Channel.Services;
using Application.Resources.Messages;
using Humanizer;

namespace Application.Core.Game.Commands.Gm0;

public class UptimeCommand : CommandBase
{
    public UptimeCommand(AdminService adminService) : base(0, "uptime")
    {
    }

    public override void Execute(IChannelClient c, string[] paramsValue)
    {
        var dur = TimeSpan.FromMilliseconds(c.CurrentServerContainer.getCurrentTimestamp());

        c.OnlinedCharacter.YellowMessageI18N(nameof(ClientMessage.UptimeCommand_Message1), dur.Humanize(culture: c.CurrentCulture.CultureInfo));
    }
}
