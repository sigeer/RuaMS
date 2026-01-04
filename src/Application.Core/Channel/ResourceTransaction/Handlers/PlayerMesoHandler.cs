namespace Application.Core.Channel.ResourceTransaction.Handlers
{
    public class PlayerCostMesoHandler : IResourceHandler
    {
        private Player _player;
        private int _frozenAmount;
        private int _deltaValue;

        public PlayerCostMesoHandler(Player player, int costAmount)
        {
            _player = player;
            _deltaValue = costAmount;
        }

        public bool TryFreeze()
        {
            lock (_player.ResourceLock)
            {
                if (_player.TryGainMeso(-_deltaValue, false))
                {
                    _frozenAmount = _deltaValue;
                    return true;
                }
                return false;
            }
        }

        public void Commit()
        {
            lock (_player.ResourceLock)
            {
                _frozenAmount = 0;
            }
        }

        public void Rollback()
        {
            lock (_player.ResourceLock)
            {
                _player.GainMeso(_frozenAmount);
                _frozenAmount = 0;
            }
        }
    }

    public class PlayerGainMesoHandler : IResourceHandler
    {
        private Player _player;
        private int _frozenAmount;

        private int _deltaValue;

        public PlayerGainMesoHandler(Player player, int costAmount)
        {
            _player = player;
            _deltaValue = costAmount;
        }

        public bool TryFreeze()
        {
            lock (_player.ResourceLock)
            {
                if (_player.TryGainMeso(_deltaValue, false))
                {
                    _frozenAmount = -_deltaValue;
                    return true;
                }
                return false;
            }
        }

        public void Commit()
        {
            lock (_player.ResourceLock)
            {
                _frozenAmount = 0;
            }
        }

        public void Rollback()
        {
            lock (_player.ResourceLock)
            {
                _player.GainMeso(_frozenAmount);
                _frozenAmount = 0;
            }
        }
    }
}
