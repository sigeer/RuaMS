namespace Application.Core.Channel.ResourceTransaction.Handlers
{
    public class PlayerCostCashHandler : IResourceHandler
    {
        private IPlayer _player;
        private int _cashType;
        private int _frozenAmount;

        private int _costAmount;

        public PlayerCostCashHandler(IPlayer player, int type, int costAmount)
        {
            _player = player;
            _cashType = type;
            _costAmount = costAmount;
        }

        public bool TryFreeze()
        {
            lock (_player.ResourceLock)
            {
                if (_player.CashShopModel.TryGainCash(_cashType, -_costAmount))
                {
                    _frozenAmount = _costAmount;
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
                if (_player.CashShopModel.TryGainCash(_cashType, _frozenAmount))
                {
                    _frozenAmount = 0;
                }
            }
        }
    }

    public class PlayerGainCashHandler : IResourceHandler
    {
        private IPlayer _player;
        private int _cashType;
        private int _frozenAmount;

        private int _deltaValue;

        public PlayerGainCashHandler(IPlayer player, int type, int gainAmount)
        {
            _player = player;
            _cashType = type;
            _deltaValue = gainAmount;
        }

        public bool TryFreeze()
        {
            lock (_player.ResourceLock)
            {
                if (_player.CashShopModel.TryGainCash(_cashType, _deltaValue))
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
                if (_player.CashShopModel.TryGainCash(_cashType, _frozenAmount))
                {
                    _frozenAmount = 0;
                }
            }
        }
    }
}
