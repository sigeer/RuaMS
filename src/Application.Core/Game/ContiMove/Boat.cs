using Application.Core.Channel;
using Application.Core.Game.Maps;
using tools;

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
        public IMap CabinAMap { get; }
        public IMap CabinBMap { get; }

        double _invasionARate = 0.42;
        double _invasionBRate = 0.42;
        public Boat(WorldChannel channelServer) : base(channelServer, 
                200000100, 0, 101000300, 0, 
                200000112, 101000301, 
                200090000, 200090010, 
                200000111, 101000300, 5 * 60 * 1000, 4 * 60 * 1000, 10 * 60 * 1000,
                4031045, 1500)
        {
            CabinAMap = channelServer.getMapFactory().getMap(200090001);
            CabinBMap = channelServer.getMapFactory().getMap(200090011);
        }

        public override void NewTask()
        {
            base.NewTask();
        }

        public override void OnStart()
        {
            base.OnStart();

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

        public override void OnArrived()
        {
            CabinAMap.warpEveryone(StationBMap.getId(), StationBPortal);
            CabinBMap.warpEveryone(StationAMap.getId(), StationAPortal);

            base.OnArrived();

            if (TransportAMap.IsPirateDocked)
            {
                TransportAMap.broadcastEnemyShip(false);
                TransportAMap.killAllMonsters();
            }

            if (TransportBMap.IsPirateDocked)
            {
                TransportBMap.broadcastEnemyShip(false);
                TransportBMap.killAllMonsters();
            }
        }

        public override void OnTick(long now)
        {
            base.OnTick(now);

            if (_approachAFlag && _approachA <= now)
            {
                ApproachA(now);
                _approachAFlag = false;
            }

            if (_summonAFlag && _summonADelay <= now)
            {
                SummonA();
                _summonAFlag = false;
            }

            if (_approachBFlag && _approachB <= now)
            {
                ApproachB(now);
                _approachBFlag = false;
            }

            if (_summonBFlag && _summonBDelay <= now)
            {
                SummonB();
                _summonBFlag = false;
            }
        }

        long _approachA;
        bool _approachAFlag;

        void ApproachA(long now)
        {
            TransportAMap.broadcastEnemyShip(true);
            // 更改背景音乐
            TransportAMap.broadcastMessage(PacketCreator.musicChange("Bgm04/ArabPirate"));

            _summonADelay = now + GetTransportationTime(5_000);
            _summonAFlag = true;
        }

        long _summonADelay;
        bool _summonAFlag;
        void SummonA()
        {
            TransportAMap.spawnMonsterOnGroundBelow(8150000, 339, 148);
            TransportAMap.spawnMonsterOnGroundBelow(8150000, 339, 148);
        }

        bool _approachBFlag;
        long _approachB;

        void ApproachB(long now)
        {
            TransportBMap.broadcastEnemyShip(true);
            // 更改背景音乐
            TransportBMap.broadcastMessage(PacketCreator.musicChange("Bgm04/ArabPirate"));

            _summonBDelay = now + GetTransportationTime(5_000);
            _summonBFlag = true;
        }

        bool _summonBFlag;
        long _summonBDelay;
        void SummonB()
        {
            TransportBMap.spawnMonsterOnGroundBelow(8150000, -538, 143);
            TransportBMap.spawnMonsterOnGroundBelow(8150000, -538, 143);
        }
    }
}
