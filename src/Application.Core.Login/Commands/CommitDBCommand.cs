namespace Application.Core.Login.Commands
{
    internal class CommitDBCommand : IMasterCommand
    {
        public void Execute(MasterCommandContext ctx)
        {
            ctx.Server.ServerManager.CommitAll();
        }
    }
}
