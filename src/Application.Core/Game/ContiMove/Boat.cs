using Application.Core.Channel;
using Application.Core.Game.Maps;
using System.Threading.Tasks;
using tools;
using static Application.Core.Channel.Internal.Handlers.PlayerFieldHandlers;

namespace Application.Core.Game.ContiMove
{
    /// <summary>
    /// A. 天空之城，B. 魔法密林
    /// </summary>
    public class Boat : ContiMoveBase
    {
        /// <summary>
        /// 船舱
        /// </summary>
        public IMap CabinAMap { get; private set; } = null!;
        public IMap CabinBMap { get; private set; } = null!;

        double _invasionARate = 0.42;
        double _invasionBRate = 0.42;
        public Boat(WorldChannel channelServer) : base(channelServer,
                200000100, 0, 101000300, 1,
                200000112, 101000301,
                200090000, 200090010,
                200000111, 101000300, 5 * 60 * 1000, 4 * 60 * 1000, 10 * 60 * 1000,
                4031047, 5000,
                4031045, 5000)
        {

        }

        protected override async Task OnMapLoad()
        {
            await base.OnMapLoad();
            CabinAMap = await ChannelServer.getMapFactory().getMap(200090001);
            CabinBMap = await ChannelServer.getMapFactory().getMap(200090011);
        }

        public override void NewTask()
        {
            base.NewTask();


        }

        public override async Task OnStart()
        {
            await base.OnStart();

            if (Random.Shared.NextDouble() < _invasionARate)
            {
                _approachA = ChannelServer.Node.getCurrentTime() + GetTransportationTime(3 * 60 * 1000);
                _approachAFlag = true;
            }
            if (Random.Shared.NextDouble() < _invasionBRate)
            {
                _approachB = ChannelServer.Node.getCurrentTime() + GetTransportationTime(3 * 60 * 1000);
                _approachBFlag = true;
            }
        }

        public override async Task OnArrived()
        {
            await CabinAMap.warpEveryone(StationBMap.getId(), StationBPortal);
            await CabinBMap.warpEveryone(StationAMap.getId(), StationAPortal);

            await base.OnArrived();

            if (TransportAMap.IsPirateDocked)
            {
                await TransportAMap.broadcastEnemyShip(false);
                await TransportAMap.killAllMonsters();
            }

            if (TransportBMap.IsPirateDocked)
            {
                await TransportBMap.broadcastEnemyShip(false);
                await TransportBMap.killAllMonsters();
            }
        }

        public override async Task OnTick(long now)
        {
            await base.OnTick(now);

            if (_approachAFlag && _approachA <= now)
            {
                await ApproachA(now);
                _approachAFlag = false;
            }

            if (_summonAFlag && _summonADelay <= now)
            {
                await SummonA();
                _summonAFlag = false;
            }

            if (_approachBFlag && _approachB <= now)
            {
                await ApproachB(now);
                _approachBFlag = false;
            }

            if (_summonBFlag && _summonBDelay <= now)
            {
                await SummonB();
                _summonBFlag = false;
            }
        }

        long _approachA;
        bool _approachAFlag;

        async Task ApproachA(long now)
        {
            await TransportAMap.broadcastEnemyShip(true);
            // 更改背景音乐
            await TransportAMap.broadcastMessage(PacketCreator.musicChange("Bgm04/ArabPirate"));

            _summonADelay = now + GetTransportationTime(5_000);
            _summonAFlag = true;
        }

        long _summonADelay;
        bool _summonAFlag;
        async Task SummonA()
        {
            await TransportAMap.spawnMonsterOnGroundBelow(8150000, -538, 143);
            await TransportAMap.spawnMonsterOnGroundBelow(8150000, -538, 143);
        }

        bool _approachBFlag;
        long _approachB;

        async Task ApproachB(long now)
        {
            await TransportBMap.broadcastEnemyShip(true);
            // 更改背景音乐
            await TransportBMap.broadcastMessage(PacketCreator.musicChange("Bgm04/ArabPirate"));

            _summonBDelay = now + GetTransportationTime(5_000);
            _summonBFlag = true;
        }

        bool _summonBFlag;
        long _summonBDelay;
        async Task SummonB()
        {
            await TransportBMap.spawnMonsterOnGroundBelow(8150000, 339, 148);
            await TransportBMap.spawnMonsterOnGroundBelow(8150000, 339, 148);
        }
    }
}
