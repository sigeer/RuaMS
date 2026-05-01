using Application.Core.Channel;
using Application.Core.Scripting.Events;

namespace Application.Plugin.Script.Events
{
    internal sealed class PQ_CPQ1 : MonsterCarnivalEventManager
    {
        public PQ_CPQ1(WorldChannel cserv) : base(cserv, nameof(PQ_CPQ1))
        {
            RecruitMap = 980000000;
            ExitMap = 980000010;

            MinMap = 980000100;
            MaxMap = 980000604;

            MinLevel = 30;
            MaxLevel = 50;

            _templates = new()
            {
                { "0", () => new MonsterCarnivalEventInstanceManager(ChannelServer, Name, "0", 2, 980000100, 980000101) },
                { "1", () => new MonsterCarnivalEventInstanceManager(ChannelServer, Name, "1", 2, 980000200, 980000201) },
                { "2", () => new MonsterCarnivalEventInstanceManager(ChannelServer, Name, "2", 3, 980000300, 980000301) }
            };
        }

        Dictionary<string, Func<MonsterCarnivalEventInstanceManager>> _templates;

        public override void Initialize()
        {
            foreach (var item in _templates)
            {
                registerEventInstance(item.Value(), -1);
            }
        }

        protected override void setEventRewards(AbstractEventInstanceManager eim)
        {
            List<object> expStages = [50000, 25500, 21000, 19505, 17500, 12000, 5000, 2500];    //bonus exp given on CLEAR stage signal
            eim.setEventClearStageExp(expStages);
        }

        protected override void DisposeInstanceInternal(string name)
        {
            base.DisposeInstanceInternal(name);

            registerEventInstance(_templates[name](), -1);
        }
    }
}
