namespace Application.Core.Game.Commands.Gm0
{
    public class XiGuaiCommand : CommandBase
    {
        public XiGuaiCommand() : base(0, "xiguai")
        {
        }

        public override Task Execute(IChannelClient client, string[] values)
        {

            var map = client.OnlinedCharacter.getMap();
            if (values.Length == 0)
            {
                if (map.XiGuai != null && map.XiGuai.Controller != client.OnlinedCharacter)
                {
                    client.OnlinedCharacter.message("其他人在吸了");
                    return Task.CompletedTask;
                }

                if (map.XiGuai == null)
                    map.XiGuai = new Gameplay.XiGuai(map, client.OnlinedCharacter);
                else
                    map.XiGuai.RestPosition();

                map.XiGuai.Start();
            }

            else if (values[0] == "off")
            {
                if (map.XiGuai != null && map.XiGuai.Controller == client.OnlinedCharacter)
                    map.XiGuai = null;

                client.OnlinedCharacter.message("已停止吸怪");
            }
            return Task.CompletedTask;
        }
    }
}
