namespace Application.Core.Login.Commands
{
    internal class CommitDBCommand : IMasterCommand
    {
        public void Execute(MasterServer ctx)
        {
            ctx.ServerManager.CommitAll();
        }
    }
}
