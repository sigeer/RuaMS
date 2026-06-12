using Application.Core.Channel;
using Application.Core.scripting.Events.Instances;
using Application.Core.Scripting.Events;
using Application.Resources.Messages;
using Application.Templates.Map;
using server.maps;
using tools;

namespace Application.Core.scripting.Events.Templates
{
    public abstract class AbstractMonsterCarnivalEventTemplate : AbstractPartyQuestEventTemplate
    {
        public int RegistrationTime { get; init; }
        public int PrepareTime { get; init; }
        public MapMonsterCarnivalTemplate MapMonsterCarnivalTemplate { get; }

        public AbstractMonsterCarnivalEventTemplate(string name, int eventMap) : base(name)
        {
            PartyLeaderRequired = true;

            MapMonsterCarnivalTemplate = MapFactory.Instance.GetMapTemplate(eventMap).MonsterCarnival!;

            RegistrationTime = 180;
            EventTime = MapMonsterCarnivalTemplate.TimeDefault - 10;
        }

        public override AbstractEventManager GenerateEventManager(WorldChannel worldChannel)
        {
            return new MonsterCarnivalEventManager(worldChannel, this);
        }

        public virtual void OnBattlePrepare(MonsterCarnivalEventInstanceManager eim) { }
        public virtual void OnBattleStarted(MonsterCarnivalEventInstanceManager pEim)
        {
            pEim.EventMap.allowSummonState(true);
            foreach (var mc in pEim.getPlayers())
            {
                var playerData = pEim.GetPlayerData(mc.Id)!;

                mc.setTeam(playerData.TeamFlag);
                mc.changeMap(pEim.EventMap, pEim.EventMap.GetInitPortal(playerData.TeamFlag));
                mc.sendPacket(PacketCreator.startMonsterCarnival(playerData, pEim.GetPlayerTeamData(mc.Id)!, pEim.GetPlayerEnemyTeamData(mc.Id)!));
                mc.LightBlue(nameof(ClientMessage.CPQ_Entry));
            }
        }
    }
}
