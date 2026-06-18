namespace Application.Core.Login.Commands
{
    internal class CommitDBCommand : IMasterAsyncCommand
    {
        public string? Name => nameof(CommitDBCommand);
        public async Task Execute(MasterServer ctx)
        {
            await ctx.ServerManager.CommitAllImmediately();
        }
    }
}
