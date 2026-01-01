using Application.Core.Channel;
using Application.Resources.Messages;

namespace Application.Core.Game.Commands.Gm1
{
    public class LanguageCommand : CommandBase
    {
        public LanguageCommand() : base(1, "language", "lang")
        {
        }

        public override Task Execute(IChannelClient client, string[] values)
        {
            if (values.Length == 0)
            {
                client.OnlinedCharacter.Yellow(nameof(ClientMessage.Language_ShowCurrent), client.CurrentCulture.CultureInfo.DisplayName);
                return Task.CompletedTask;
            }

            var setValue = values[0];
            if (!int.TryParse(setValue, out var lang) || lang != 0 && lang != 1)
            {
                client.OnlinedCharacter.Yellow(nameof(ClientMessage.LanguageCommand_Syntax));
                return Task.CompletedTask;
            }

            client.Language = lang;
            client.CurrentCulture = new ClientCulture(lang);
            return Task.CompletedTask;
        }
    }
}
