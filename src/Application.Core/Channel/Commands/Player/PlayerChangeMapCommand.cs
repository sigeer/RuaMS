namespace Application.Core.Channel.Commands
{
    internal class PlayerChangeMapCommand : IWorldChannelCommand
    {
        Player _chr;
        int _fromMapId;
        int _toMapId;

        public PlayerChangeMapCommand(Player chr, int fromId, int toMapId)
        {
            _chr = chr;
            _fromMapId = fromId;
            _toMapId = toMapId;
        }

        public void Execute(ChannelCommandContext ctx)
        {
            if (_chr.getMapId() == _fromMapId)
                _chr.changeMap(_toMapId, 0);
        }
    }
}
