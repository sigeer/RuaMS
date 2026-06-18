namespace Application.Core.Game.Commands.Gm6
{
    public class SetConfigCommand : ParamsCommandBase
    {
        public SetConfigCommand() : base(["<name>", "<value>"], 6, ["设置"])
        {
        }

        public override async Task Execute(IChannelClient client, string[] values)
        {
            var result = YamlConfig.SetValue(GetParam("name"), GetParam("value"));
            if (!string.IsNullOrEmpty(result))
                await client.OnlinedCharacter.Notice(result);
        }
    }
}
