namespace Application.Core.Game.Commands.Gm0;

public class EnableAuthCommand : CommandBase
{
    public EnableAuthCommand() : base(0, "enableauth")
    {
    }

    public override async Task Execute(IChannelClient c, string[] paramValues)
    {
        {
            await c.tryacquireClient();
            try
            {
                //
                //LoginBypassCoordinator.getInstance().unregisterLoginBypassEntry(c.Hwid, c.AccountEntity.Id);
            }
            finally
            {
                c.releaseClient();
            }
        }
    }
}
