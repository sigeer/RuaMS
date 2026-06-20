using Application.Core.Game.Life;

namespace Application.Core.Channel.Commands
{
    internal class MonsterPuppetAggroCommand : IWorldChannelAsyncCommand
    {
        public string Name => nameof(MonsterPuppetAggroCommand);
        Monster _mob;

        public MonsterPuppetAggroCommand(Monster mob)
        {
            _mob = mob;
        }

        public async Task Execute(WorldChannel ctx)
        {
            await _mob.ApplyPuppetAggro();
        }
    }
}
