namespace Application.Core.Login.Commands
{
    internal class CommitDBCommand : IMasterCommand
    {
        public string? Name => nameof(CommitDBCommand);
        public void Execute(MasterServer ctx)
        {
            ctx.ServerManager.CommitAll();
        }
    }
}
