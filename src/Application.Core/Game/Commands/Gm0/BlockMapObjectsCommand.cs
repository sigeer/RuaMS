namespace Application.Core.Game.Commands.Gm0
{
    internal class BlockMapObjectsCommand : ParamsCommandBase
    {
        public BlockMapObjectsCommand() : base(["[summon|dragon]"], 0, "block", "屏蔽")
        {
            Description = "屏蔽部分地图对象。";
        }

        public override void Execute(IChannelClient client, string[] values)
        {
            var type = GetParamByIndex(0) ?? throw new CommandArgumentException(); ;
            if (type.Equals("summon", StringComparison.OrdinalIgnoreCase))
            {
                client.OnlinedCharacter.FilterSummon = true;
            }
        }
    }
}
