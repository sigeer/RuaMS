using Application.Core.Client;
using Application.Core.Game.Life;
using Application.Core.Game.Players;
using Application.Core.scripting.Events.Abstraction;
using Application.Core.scripting.Events.Instances;
using Application.Core.scripting.Events.Templates;
using Application.Shared.Constants.Map;
using tools;

namespace Application.Plugin.Script.Features.Dojo
{
    internal sealed class EventSolo : AbstractSoloEventTemplate
    {
        public EventSolo() : base(DojoFeature.SoloEventName)
        {
            MinLevel = 25;
            MaxLevel = 255;

            MaxLobbys = 15;

            EntryMap = 925020100;
            ExitMap = DojoFeature.ExitMap;
            RecruitMap = DojoFeature.RecruitMap;
            ClearMap = DojoFeature.ClearMap;

            MinMap = 925020100;
            MaxMap = 925029999;

            // 首关（第 1 层）限时；后续每层进入时会按层数重新计时
            EventTime = 5 * 60;
        }

        public override string? HandleCreateInstanceResult(CreateInstanceResult r, IChannelClient c)
        {
            return DojoFeature.HandleCreateInstanceResult(r, c);
        }


        /// <summary>
        /// 从存档点续关。存档只在休息关由 NPC 写入，记录的是「上次打到的层」，
        /// 因此本次从它的下一层开始；0 表示没有记录，从第 1 层开始。
        ///
        /// 记录只对这一次挑战有效：读到后立即清零，失败重进时不会再从同一层重来。
        /// </summary>
        public override async Task OnPlayerEntry(AbstractEventInstanceManager eim, Player chr)
        {
            // 正常流程里存档只会是 6/12/…/36，这里只是防脏数据把玩家送进不存在的地图。
            var savedStage = Math.Clamp(chr.getDojoStage(), 0, DojoFeature.MaxStage - 1);
            chr.setDojoStage(0);
            chr.setDojoEnergy(0);

            var mapId = DojoFeature.GetStageMapId(MapId.DOJO_SOLO_BASE, savedStage + 1);

            var map = await eim.getMapInstance(mapId);
            await chr.changeMap(map, EntryPortal);
        }


        public override async Task OnMobRevive(AbstractEventInstanceManager eim, Monster mob)
        {
            await base.OnMobRevive(eim, mob);

            if (mob.getId() == DojoFeature.HiddenMob)
            {
                await OnMapCompleted(eim, mob.MapModel);

                if (DojoFeature.GetStage(mob.MapModel.Id) == DojoFeature.MaxStage)
                {
                    await eim.clearPQ();
                }
            }
        }
    }
}
