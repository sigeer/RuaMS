using constants.id;
using server.events.gm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Core.Game.Maps.Specials
{
    public interface ISnowBallMap: IMap
    {
        public int DamageSnowBall { get; set; }
        public int DamageSnowMan0 { get; set; }
        public int DamageSnowMan1 { get; set; }
        public int RecoveryAmount { get; set; }
        public int SnowManHP { get; set; }
        public int SnowManWait { get; set; }
        Snowball? getSnowball(int team);
        void setSnowball(int team, Snowball? ball);
    }

    public class SnowBallMap: MapleMap, ISnowBallMap
    {
        public int DamageSnowBall { get; set; }
        public int DamageSnowMan0 { get; set; }
        public int DamageSnowMan1 { get; set; }
        public int RecoveryAmount { get; set; }
        public int SnowManHP { get; set; }
        public int SnowManWait { get; set; }

        private Snowball? snowball0 = null;
        private Snowball? snowball1 = null;

        public SnowBallMap(IMap map) : base(map.getId(), map.ChannelServer, map.getReturnMapId(), map.MonsterRate)
        {
        }

        public Snowball? getSnowball(int team)
        {
            switch (team)
            {
                case 0:
                    return snowball0;
                case 1:
                    return snowball1;
                default:
                    return null;
            }
        }

        public void setSnowball(int team, Snowball? ball)
        {
            switch (team)
            {
                case 0:
                    this.snowball0 = ball;
                    break;
                case 1:
                    this.snowball1 = ball;
                    break;
                default:
                    break;
            }
        }

        public override void startEvent(IPlayer chr)
        {
            if (this.Id == MapId.EVENT_SNOWBALL && getSnowball(chr.getTeam()) == null)
            {
                setSnowball(0, new Snowball(0, this));
                setSnowball(1, new Snowball(1, this));
                getSnowball(chr.getTeam())?.startEvent();
            }
        }
    }
}
