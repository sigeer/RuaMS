using Application.Core.Game.Life;
using Application.Core.Game.Maps;
using Application.Core.scripting.Events.Instances;
using Application.Core.scripting.Events.Templates;
using server.life;
using System.Drawing;

namespace Application.Plugin.Script.Events
{
    internal class Battle_Balrog : AbstractExpeditionEventTemplate
    {
        public Battle_Balrog() : base(nameof(Battle_Balrog), 8830003)
        {
            MinCount = 6;
            MaxCount = 30;

            MinLevel = 50;
            MaxLevel = 255;

            EntryMap = 105100300;
            ExitMap = 105100100;
            ClearMap = 105100301;

            MinMap = 105100300;
            MaxMap = 105100301;

            EventTime = 60 * 60;
            RegistrationTime = 5 * 60;
        }

        public override void AfterSeup(AbstractEventInstanceManager eim)
        {
            base.AfterSeup(eim);

            eim.Schedule(releaseLeftClaw, 1 * 60000);

            var mapObj = eim.getInstanceMap(EntryMap)!;

            var mob0 = LifeFactory.Instance.GetMonsterTrust(8830000);
            mapObj.spawnFakeMonsterOnGroundBelow(mob0, new Point(412, 258));

            // 8830002 -> 8830005
            var mob2 = LifeFactory.Instance.GetMonsterTrust(8830002);
            mapObj.spawnMonsterOnGroundBelow(mob2, new Point(412, 258));

            // 8830006 -> 8830001 -> 8830004
            var mob6 = LifeFactory.Instance.GetMonsterTrust(8830006);
            mapObj.spawnMonsterOnGroundBelow(mob6, new Point(412, 258));
        }

        void releaseLeftClaw(AbstractEventInstanceManager eim)
        {
            eim.getInstanceMap(EntryMap)?.killMonster(8830006);
        }

        public override void OnMobKilled(AbstractEventInstanceManager eim, Monster mob, ICombatantObject? killer)
        {
            var mapObj = eim.getInstanceMap(EntryMap)!;
            if (mob.getId() == 8830001 || mob.getId() == 8830002)
            {
                var count = eim.getIntProperty("boss");
                count++;

                if (count == 2)
                {
                    mapObj.makeMonsterReal(mapObj.getMonsterById(8830000));
                }
                eim.setIntProperty("boss", count);
            }


            if (mob.getId() == 8830000)
            {
                eim.showClearEffect();
                eim.clearPQ();

                eim.dispatchRaiseQuestMobCount(BossId, EntryMap);
                eim.dispatchRaiseQuestMobCount(9101003, EntryMap); // thanks Atoot for noticing quest not getting updated after boss kill

                eim.EventManager.ChannelServer.NodeActor.Send(s =>
                {
                    s.SendDropMessage(6, "[Victory] " + eim.getLeader().getName() + "'s party has successfully defeated the Balrog! Praise to them, they finished with " + mapObj.countAlivePlayers() + " players alive.", false);
                });

                mapObj.spawnMonsterOnGroundBelow(LifeFactory.Instance.GetMonsterTrust(BossId), new Point(412, 258));
            }
        }


    }
}
