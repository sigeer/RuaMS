using Application.Core.Channel;
using Application.Core.Game.Maps;

namespace Application.Core.Game.ContiMove
{
    /// <summary>
    /// A. 2层 B. 99层
    /// </summary>
    public class Elevator : ContiMoveBase
    {
        IMap _current = null!;

        public Elevator(WorldChannel channelServer)
            : base(channelServer,
                  222020100, 0, 222020200, 0,
                  222020110, 222020210,
                  222020111, 222020211,
                  -1, -1,
                  60 * 1000, 60 * 1000, 60 * 1000,
                  0, 0, 0, 0)
        {


        }

        public override async Task Initialize()
        {
            await base.Initialize();
            await StationBMap.setReactorState();
        }

        protected override async Task OnMapLoad()
        {
            await base.OnMapLoad();
            _current = StationAMap;
        }

        public override void NewTask()
        {
            base.NewTask();

            _current.resetReactors();
        }

        public override async Task OnStart()
        {
            if (_current == StationBMap)
            {
                await WaitingRoomBMap.warpEveryone(TransportBMap.Id);
            }
            else
            {
                await WaitingRoomAMap.warpEveryone(TransportAMap.Id);
            }

            await StationAMap.setReactorState();
            await StationBMap.setReactorState();

            ArriveAt = ChannelServer.Node.getCurrentTime() + _currentRideTime;
            _arriveFlag = true;
        }

        public override async Task OnArrived()
        {
            if (_current == StationBMap)
            {
                await TransportBMap.warpEveryone(StationAMap.Id, 4);
                _current = StationAMap;
            }
            else
            {
                await TransportAMap.warpEveryone(StationAMap.Id);
                _current = StationBMap;
            }

            NewTask();
        }
    }
}
