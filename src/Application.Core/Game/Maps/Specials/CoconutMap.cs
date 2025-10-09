using Application.Core.Channel;
using Application.Templates.Map;
using scripting.Event;
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

        public CoconutMap(MapTemplate template, WorldChannel worldChannel, EventInstanceManager? eim) : base(template, worldChannel, eim)
        {
            var mapData = template.Coconut!;
            CountFalling = mapData.CountFalling;
            CountBombing = mapData.CountBombing;
            CountStopped = mapData.CountStopped;
            CountHit = mapData.CountHit;

            TimeDefault = mapData.TimeDefault;
            TimeExpand = mapData.TimeExpand;
            TimeFinish = mapData.TimeFinish;

            SoundWin = mapData.SoundWin ?? GetDefaultSoundWin();
            SoundLose = mapData.SoundLose ?? GetDefaultSoundLose();
            EffectWin = mapData.EffectWin ?? GetDefaultEffectWin();
            EffectLose = mapData.EffectLose ?? GetDefaultEffectLose();
        }

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
