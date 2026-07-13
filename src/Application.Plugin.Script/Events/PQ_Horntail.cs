using Application.Core.scripting.Events.Instances;
using Application.Core.scripting.Events.Templates;

namespace Application.Plugin.Script.Events
{

    internal class PQ_Horntail : AbstractPartyQuestEventTemplate
    {
        public PQ_Horntail() : base(nameof(PQ_Horntail))
        {
            MinCount = 6;
            MaxCount = 6;

            MinLevel = 120;
            MaxLevel = 255;

            EntryMap = 240050100;
            ExitMap = 240050000;
            RecruitMap = 240050000;
            ClearMap = 240050400;

            MinMap = 240050100;
            MaxMap = 240050310;

            EventTime = 25 * 60;
        }

        public override async Task OnSetup(AbstractEventInstanceManager eim, int level, int lobbyId)
        {
            (await eim.getInstanceMap(240050101))?.getReactorByName("passKey1")?.setEventState(0);
            (await eim.getInstanceMap(240050102))?.getReactorByName("passKey2")?.setEventState(1);
            (await eim.getInstanceMap(240050103))?.getReactorByName("passKey3")?.setEventState(2);
            (await eim.getInstanceMap(240050104))?.getReactorByName("passKey4")?.setEventState(3);

            await base.OnSetup(eim, level, lobbyId);
        }
    }
}
