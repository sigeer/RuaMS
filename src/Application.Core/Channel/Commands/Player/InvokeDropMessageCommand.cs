namespace Application.Core.Channel.Commands
{
    internal record InvokeDropMessageCommand : IWorldChannelCommand
    {
        int playerId;
        int type;
        string message;
        string[] paramsValue;

        public InvokeDropMessageCommand(int playerId, int type, string message, params string[] paramsValue)
        {
            this.playerId = playerId;
            this.type = type;
            this.message = message;
            this.paramsValue = paramsValue;
        }

        public void Execute(ChannelCommandContext ctx)
        {
            var chr = ctx.WorldChannel.getPlayerStorage().getCharacterById(playerId);
            if (chr != null)
            {
                chr.TypedMessage(type, message, paramsValue);
            }
        }
    }

    internal record InvokeMultiDropMessageCommand : IWorldChannelCommand
    {
        IEnumerable<int> playerIds;
        int type;
        string message;
        string[] paramsValue;

        public InvokeMultiDropMessageCommand(IEnumerable<int> playerIds, int type, string message, params string[] paramsValue)
        {
            this.playerIds = playerIds;
            this.type = type;
            this.message = message;
            this.paramsValue = paramsValue;
        }

        public void Execute(ChannelCommandContext ctx)
        {
            if (playerIds.Contains(-1))
            {
                foreach (var player in ctx.WorldChannel.getPlayerStorage().getAllCharacters())
                {
                    player.TypedMessage(type, message, paramsValue);
                }
            }
            else
            {
                foreach (var id in playerIds)
                {
                    var player = ctx.WorldChannel.getPlayerStorage().getCharacterById(id);
                    if (player != null)
                    {
                        player.TypedMessage(type, message, paramsValue);
                    }
                }
            }
            return;
        }
    }
}
