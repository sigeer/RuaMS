using Application.Core.Channel;
using Application.Core.Game.Maps;
using Application.Utility.Tickables;

namespace Application.Core.Game.ContiMove
{

    public abstract class ContiMoveBase : ITickable
    {
        public WorldChannel ChannelServer { get; }
        /// <summary>
        /// 抵达目的地时的地图
        /// </summary>
        public IMap StationAMap { get; }
        public int StationAPortal { get; }
        public IMap StationBMap { get; }
        public int StationBPortal { get; }

        /// <summary>
        /// 等待室
        /// </summary>
        public IMap WaitingRoomAMap { get; }
        public IMap WaitingRoomBMap { get; }

        /// <summary>
        /// 传送中
        /// </summary>
        public IMap TransportAMap { get; }
        public IMap TransportBMap { get; }

        /// <summary>
        /// 码头，用于播放到站/离站动画
        /// </summary>
        public IMap? DockAMap { get; }
        public IMap? DockBMap { get; }


        public TickableStatus Status => TickableStatus.Active;

        public long BeginTime { get; }
        public long CloseTime { get; }
        public long RideTime { get; }

        /// <summary>
        /// 0. meso
        /// </summary>
        public int TicketItemId { get; }
        public int TicketPrice { get; }

        protected float _currentTransitionRate;
        protected long _currentBeginTime;
        protected long _currentCloseTime;
        protected long _currentRideTime;

        protected ContiMoveBase(WorldChannel channelServer,
            int stationAMap, int stationAPortal,
            int stationBMap, int stationBPortal,
            int waitingRoomAMap, int waitingRoomBMap,
            int transportAMap, int transportBMap,
            int dockAMap, int dockBMap,
            long beginTime, long closeTime, long rideTime,
            int ticketItemId, int ticketPrice)
        {
            ChannelServer = channelServer;
            var mapManager = ChannelServer.getMapFactory();
            StationAMap = mapManager.getMap(stationAMap);
            StationAPortal = stationAPortal;
            StationBMap = mapManager.getMap(stationBMap);
            StationBPortal = stationBPortal;
            WaitingRoomAMap = mapManager.getMap(waitingRoomAMap);
            WaitingRoomBMap = mapManager.getMap(waitingRoomBMap);
            TransportAMap = mapManager.getMap(transportAMap);
            TransportBMap = mapManager.getMap(transportBMap);
            DockAMap = dockAMap > 0 ? mapManager.getMap(dockAMap) : null;
            DockBMap = dockBMap > 0 ? mapManager.getMap(dockBMap) : null;

            BeginTime = beginTime;
            CloseTime = closeTime;
            RideTime = rideTime;

            TicketItemId = ticketItemId;
            TicketPrice = ticketPrice;

        }

        /// <summary>
        /// 何时停止检票
        /// </summary>
        protected long _stopEntryAt;
        protected bool _stopFlag;
        /// <summary>
        /// 何时出发
        /// </summary>
        protected long _startAt;
        protected bool _startFlag;
        /// <summary>
        /// 何时抵达
        /// </summary>
        public long ArriveAt { get; protected set; }
        protected bool _arriveFlag;

        public int GetTransportationTime(double travelTime)
        {
            return (int)Math.Ceiling(travelTime / _currentTransitionRate);
        }

        public virtual void Initialize()
        {
            NewTask();
        }

        public virtual void NewTask()
        {
            DockAMap?.setDocked(true);
            DockBMap?.setDocked(true);

            var now = ChannelServer.Node.getCurrentTime();

            _currentTransitionRate = ChannelServer.WorldTravelRate;
            _currentBeginTime = GetTransportationTime(BeginTime);
            _currentCloseTime = GetTransportationTime(CloseTime);
            _currentRideTime = GetTransportationTime(RideTime);

            _stopEntryAt = now + _currentCloseTime;
            _startAt = now + _currentBeginTime;

            _stopFlag = true;
            _startFlag = true;

            CanEnter = true;
        }

        public virtual bool Enter(Player chr)
        {
            if (!CanEnter)
            {
                return false;
            }

            if (chr.MapModel == StationAMap)
            {
                chr.changeMap(WaitingRoomAMap);
                return true;
            }
            if (chr.MapModel == StationBMap)
            {
                chr.changeMap(WaitingRoomBMap);
                return true;
            }
            return false;
        }

        public string GetDestinationMapName(Player chr)
        {
            if (chr.MapModel == StationAMap)
            {
                return chr.Client.CurrentCulture.GetMapStreetName(StationBMap.Id);
            }
            if (chr.MapModel == StationBMap)
            {
                return chr.Client.CurrentCulture.GetMapStreetName(StationAMap.Id);
            }
            return "";
        }

        public virtual void OnStart()
        {
            WaitingRoomAMap.warpEveryone(TransportAMap.getId());
            DockAMap?.broadcastShip(false);

            WaitingRoomBMap.warpEveryone(TransportBMap.getId());
            DockBMap?.broadcastShip(false);

            ArriveAt = ChannelServer.Node.getCurrentTime() + _currentRideTime;
            _arriveFlag = true;
        }

        public virtual void OnArrived()
        {
            TransportAMap.warpEveryone(StationBMap.getId(), StationBPortal);
            DockBMap?.broadcastShip(true);

            TransportBMap.warpEveryone(StationAMap.getId(), StationAPortal);
            DockAMap?.broadcastShip(true);

            NewTask();
        }

        public bool CanEnter { get; private set; }

        public virtual void OnTick(long now)
        {
            if (_stopFlag && _stopEntryAt <= now)
            {
                CanEnter = false;
                _stopFlag = false;
            }

            if (_startFlag && _startAt <= now)
            {
                OnStart();
                _startFlag = false;
            }

            if (_arriveFlag && ArriveAt <= now)
            {
                OnArrived();
                _arriveFlag = false;
            }
        }
    }
}
