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
        public IMap StationAMap { get; protected set; } = null!;
        int _stationAMapId;
        public int StationAPortal { get; protected set; }
        int _stationBMapId;
        public IMap StationBMap { get; protected set; } = null!;
        public int StationBPortal { get; protected set; }

        int _waitingRoomAMapId;
        /// <summary>
        /// 等待室
        /// </summary>
        public IMap WaitingRoomAMap { get; protected set; } = null!;
        int _waitingRoomBMapId;
        public IMap WaitingRoomBMap { get; protected set; } = null!;

        int _transportAMapId;
        /// <summary>
        /// 传送中
        /// </summary>
        public IMap TransportAMap { get; protected set; } = null!;
        int _transportBMapId;
        public IMap TransportBMap { get; protected set; } = null!;

        int _dockAMapId;
        /// <summary>
        /// 码头，用于播放到站/离站动画
        /// </summary>
        public IMap? DockAMap { get; protected set; }
        int _dockBMapId;
        public IMap? DockBMap { get; protected set; }


        public TickableStatus Status => TickableStatus.Active;

        public long BeginTime { get; }
        public long CloseTime { get; }
        public long RideTime { get; }

        private int TicketAItemId;
        private int TicketAPrice;
        private int TicketBItemId;
        private int TicketBPrice;

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
            int ticketAItemId, int ticketAPrice,
            int ticketBItemId, int ticketBPrice)
        {
            ChannelServer = channelServer;

            _stationAMapId = stationAMap; 
            _stationBMapId = stationBMap;
            StationAPortal = stationAPortal;
            StationBPortal = stationBPortal;

            _transportAMapId = transportAMap;
            _transportBMapId = transportBMap;
            _dockAMapId = dockAMap;
            _dockBMapId = dockBMap;
            _waitingRoomAMapId = waitingRoomAMap;
            _waitingRoomBMapId = waitingRoomBMap;


            BeginTime = beginTime;
            CloseTime = closeTime;
            RideTime = rideTime;

            TicketAItemId = ticketAItemId;
            TicketAPrice = ticketAPrice;
            TicketBItemId = ticketBItemId;
            TicketBPrice = ticketBPrice;
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

        public virtual async Task Initialize()
        {
            await OnMapLoad();

            NewTask();
        }

        protected virtual async Task OnMapLoad()
        {
            var mapManager = ChannelServer.getMapFactory();
            StationAMap = await mapManager.getMap(_stationAMapId);
            StationBMap = await mapManager.getMap(_stationBMapId);

            WaitingRoomAMap = await mapManager.getMap(_waitingRoomAMapId);
            WaitingRoomBMap = await mapManager.getMap(_waitingRoomBMapId);
            TransportAMap = await mapManager.getMap(_transportAMapId);
            TransportBMap = await mapManager.getMap(_transportBMapId);
            DockAMap = _dockAMapId > 0 ? await mapManager.getMap(_dockAMapId) : null;
            DockBMap = _dockBMapId > 0 ? await mapManager.getMap(_dockBMapId) : null;
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

        public virtual async Task<bool> Enter(Player chr)
        {
            if (!CanEnter)
            {
                return false;
            }

            if (chr.MapModel == StationAMap)
            {
                await chr.changeMap(WaitingRoomAMap);
                return true;
            }
            if (chr.MapModel == StationBMap)
            {
                await chr.changeMap(WaitingRoomBMap);
                return true;
            }
            return false;
        }

        public string? GetDestinationMapName(Player chr)
        {
            if (chr.MapModel == StationAMap || chr.MapModel == WaitingRoomAMap || chr.MapModel == TransportAMap)
            {
                return chr.Client.CurrentCulture.GetMapStreetName(StationBMap.Id);
            }
            if (chr.MapModel == StationBMap || chr.MapModel == WaitingRoomBMap || chr.MapModel == TransportBMap)
            {
                return chr.Client.CurrentCulture.GetMapStreetName(StationAMap.Id);
            }
            return null;
        }

        public (int TicketItemId, int TicketPrice)? GetTicket(Player chr)
        {
            if (chr.MapModel == StationAMap)
            {
                return (TicketAItemId, TicketAPrice);
            }
            if (chr.MapModel == StationBMap)
            {
                return (TicketBItemId, TicketBPrice);
            }
            return null;
        }

        public virtual async Task OnStart()
        {
            await WaitingRoomAMap.warpEveryone(TransportAMap.getId());
            if (DockAMap != null)
                await DockAMap.broadcastShip(false);


            await WaitingRoomBMap.warpEveryone(TransportBMap.getId());
            if (DockBMap != null)
                await DockBMap.broadcastShip(false);

            ArriveAt = ChannelServer.Node.getCurrentTime() + _currentRideTime;
            _arriveFlag = true;
        }

        public virtual async Task OnArrived()
        {
            await TransportAMap.warpEveryone(StationBMap.getId(), StationBPortal);
            if (DockBMap != null)
                await DockBMap.broadcastShip(true);

            await TransportBMap.warpEveryone(StationAMap.getId(), StationAPortal);
            if (DockAMap != null)
                await DockAMap.broadcastShip(true);

            NewTask();
        }

        public bool CanEnter { get; private set; }

        public virtual async Task OnTick(long now)
        {
            if (_stopFlag && _stopEntryAt <= now)
            {
                CanEnter = false;
                _stopFlag = false;
            }

            if (_startFlag && _startAt <= now)
            {
                await OnStart();
                _startFlag = false;
            }

            if (_arriveFlag && ArriveAt <= now)
            {
                await OnArrived();
                _arriveFlag = false;
            }
        }

        public bool IsTransporting(IMap map)
        {
            return map == TransportAMap || map == TransportBMap;
        }

        public bool IsWaiting(IMap map)
        {
            return map == WaitingRoomAMap || map == WaitingRoomBMap;
        }
    }
}
