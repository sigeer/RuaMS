using Application.Shared.MapObjects;
using constants.id;
using server.events.gm;

namespace Application.Core.Game.Maps.Specials
{
    public interface ICoconutMap : IMap, IGroupEffectEventMap, ITimeLimitedEventMap, IGroupSoundEventMap
    {
        public int CountFalling { get; set; }
        public int CountBombing { get; set; }
        public int CountStopped { get; set; }
        public int CountHit { get; set; }

        public Coconut? Coconut { get; set; }
    }

    public class CoconutMap : MapleMap, ICoconutMap
    {
        public const string DefaultEffectWin = "event/coconut/victory";
        public const string DefaultEffectLose = "event/coconut/lose";
        public const string DefaultSoundWin = "Coconut/Victory";
        public const string DefaultSoundLose = "Coconut/Failed";

        public string EffectWin { get; set; } = DefaultEffectWin;
        public string EffectLose { get; set; } = DefaultEffectLose;
        public string SoundWin { get; set; } = DefaultSoundWin;
        public string SoundLose { get; set; } = DefaultSoundLose;
        public int TimeDefault { get; set; }
        public int TimeExpand { get; set; }
        public int TimeFinish { get; set; }

        public int CountFalling { get; set; }
        public int CountBombing { get; set; }
        public int CountStopped { get; set; }
        public int CountHit { get; set; }

        public Coconut? Coconut { get; set; }
        public CoconutMap(IMap map) : base(map.getId(), map.ChannelServer, map.getReturnMapId(), map.MonsterRate)
        {
        }

        public string GetDefaultSoundWin()
        {
            return DefaultSoundWin;
        }
        public string GetDefaultSoundLose()
        {
            return DefaultSoundLose;
        }

        public string GetDefaultEffectWin()
        {
            return DefaultEffectWin;
        }
        public string GetDefaultEffectLose()
        {
            return DefaultEffectLose;
        }

        public override void startEvent(IPlayer chr)
        {
            if (this.Id == MapId.EVENT_COCONUT_HARVEST && Coconut == null)
            {
                Coconut = new Coconut(this);
                Coconut.startEvent();
            }
        }
    }
}
