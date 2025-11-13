namespace Application.Core.Game.Commands.Gm0
{
    public class ExtraStorageCommand : CommandBase
    {
        public ExtraStorageCommand() : base(0, "storage")
        {
        }

        public override void Execute(IChannelClient client, string[] values)
        {
            if (client.NPCConversationManager != null)
                return;

            client.OnlinedCharacter.GachaponStorage.OpenStorage(NpcId.MAPLE_ADMINISTRATOR);
        }
    }
}
