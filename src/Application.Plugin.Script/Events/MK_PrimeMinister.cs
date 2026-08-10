using Application.Core.Channel.Net.Packets;
using Application.Core.Game.Life;
using Application.Core.Game.Maps;
using Application.Core.Game.Players;
using Application.Core.scripting.Events.Instances;
using Application.Core.scripting.Events.Templates;
using CommunityToolkit.HighPerformance.Helpers;
using server.life;
using System.Drawing;

namespace Application.Plugin.Script.Events
{
    internal class MK_PrimeMinister : AbstractPartyQuestEventTemplate
    {
        public MK_PrimeMinister() : base(nameof(MK_PrimeMinister))
        {
            EventTime = 10 * 60;
            MinCount = 1;
            MaxCount = 3;
            MinLevel = 30;
            MaxLevel = 255;

            EntryMap = 106021600;
            EntryPortal = 1;
            ExitMap = 106021402;
            MinMap = 106021600;
            MaxMap = 106021600;
            RecruitMap = 106021402;

        }

        public override async Task OnSetup(AbstractEventInstanceManager eim, int level, int lobbyId)
        {
            await base.OnSetup(eim, level, lobbyId);

            var weddinghall = await eim.getMapInstance(EntryMap);
            weddinghall.getPortal(1)?.setPortalState(false);
        }

        public override async Task OnPlayerEntry(AbstractEventInstanceManager eim, Player chr)
        {
            await base.OnPlayerEntry(eim, chr);

            if (chr.Id == eim.getLeaderId())
            {
                var questStatus = chr.GetOrAddQuest(2333);
                if (questStatus.getStatus() != client.QuestStatus.Status.NOT_STARTED)
                {
                    await SpawnBoss(eim);
                }
            }
        }

        int mobId = 3300008;
        public override async Task OnMobKilled(AbstractEventInstanceManager eim, Monster mob, ICombatantObject? killer)
        {
            if (mob.getId() == mobId)
            {
                var map = await eim.getMapInstance(EntryMap);
                map.getPortal(1)?.setPortalState(true);

                await eim.showClearEffect();
                await eim.clearPQ();
            }
        }

        public async Task SpawnBoss(AbstractEventInstanceManager eim)
        {
            var weddinghall = await eim.getMapInstance(EntryMap);
            var pos = new Point(292, 143);
            await weddinghall.spawnMonsterOnGroundBelow(LifeFactory.Instance.getMonster(mobId), pos);
            await weddinghall.broadcastMessage(FieldEffectPacket.Summon(23, pos.X, pos.Y)); // name=23一样？
        }
    }
}
