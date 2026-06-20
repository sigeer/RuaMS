namespace Application.Core.Channel.Commands
{
    internal record InvokeDropMessageCommand : IWorldChannelCommand
    {
        public string Name => nameof(InvokeDropMessageCommand);
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

        public void Execute(WorldChannel ctx)
        {
            ctx.getPlayerStorage().GetCharacterClientById(playerId)?.TypedMessage(type, message, paramsValue);
        }
    }

    internal record InvokeMultiDropMessageCommand : IWorldChannelAsyncCommand
    {
        public string Name => nameof(InvokeMultiDropMessageCommand);
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

        public async Task Execute(WorldChannel ctx)
        {
            if (playerIds.Contains(-1))
            {
                foreach (var player in ctx.getPlayerStorage().getAllCharacters())
                {
                    await player.TypedMessage(type, message, paramsValue);
                }
            }
            else
            {
                foreach (var id in playerIds)
                {
                    var player = ctx.getPlayerStorage().getCharacterById(id);
                    if (player != null)
                    {
                        await player.TypedMessage(type, message, paramsValue);
                    }
                }
            }
            return;
        }
    }

    internal record InvokeMultiDropMessageCommandPlus : IWorldChannelAsyncCommand
    {
        public string Name => nameof(InvokeMultiDropMessageCommandPlus);

        IEnumerable<int> playerIds;
        NoticeType type;
        Func<Player, string> _getMessage;
        public InvokeMultiDropMessageCommandPlus(IEnumerable<int> playerIds, NoticeType type, Func<Player, string> getMessage)
        {
            this.playerIds = playerIds;
            this.type = type;
            _getMessage = getMessage;
        }

        public async Task Execute(WorldChannel ctx)
        {
            if (playerIds.Contains(-1))
            {
                foreach (var player in ctx.getPlayerStorage().getAllCharacters())
                {
                    await player.TypedMessage((int)type, _getMessage(player));
                }
            }
            else
            {
                foreach (var id in playerIds)
                {
                    var player = ctx.getPlayerStorage().getCharacterById(id);
                    if (player != null)
                    {
                        await player.TypedMessage((int)type, _getMessage(player));
                    }
                }
            }
            return;
        }
    }
}
