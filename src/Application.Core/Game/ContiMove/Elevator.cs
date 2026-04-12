using Application.Core.Channel;
using Application.Core.Game.Maps;

namespace Application.Core.Game.ContiMove
{
    /// <summary>
    /// A. 2层 B. 99层
    /// </summary>
    public class Elevator : ContiMoveBase
    {
        IMap _current;

        public Elevator(WorldChannel channelServer)
            : base(channelServer,
                  222020100, 0, 222020200, 0,
                  222020110, 222020210,
                  222020111, 222020211,
                  -1, -1,
                  60 * 1000, 60 * 1000, 60 * 1000,
                  0, 0)
        {
            _current = StationAMap;

        }

        public override void Initialize()
        {
            base.Initialize();

            StationBMap.setReactorState();
        }

        public override void NewTask()
        {
            base.NewTask();

            _current.resetReactors();
        }

        public override void OnStart()
        {
            if (_current == StationBMap)
            {
                WaitingRoomBMap.warpEveryone(TransportBMap.Id);
            }
            else
            {
                WaitingRoomAMap.warpEveryone(TransportAMap.Id);
            }

            StationAMap.setReactorState();
            StationBMap.setReactorState();

            ArriveAt = ChannelServer.Node.getCurrentTime() + _currentRideTime;
            _arriveFlag = true;
        }

        public override void OnArrived()
        {
            if (_current == StationBMap)
            {
                TransportBMap.warpEveryone(StationAMap.Id, 4);
                _current = StationAMap;
            }
            else
            {
                TransportAMap.warpEveryone(StationAMap.Id);
                _current = StationBMap;
            }

            NewTask();
        }
    }
}
