using Application.Shared.Constants.Job;
using Application.Plugin.Script;
using Application.Resources.Messages;

namespace Application.Plugin.Script.Npc
{
    internal partial class NpcScript
    {
        async Task Desguise(Job baseJob)
        {
            if (!getJob().IsSameJobGroup(baseJob))
            {
                await SayOK(this.GetTalkMessage(ScriptMessage.Cygnus_NoIntruder));
                return;
            }

            await SayOK(this.GetTalkMessage(ScriptMessage.Cygnus_FoundMe));
            var mapobj = getMap();
            var npcpos = mapobj.getMapObject(getNpcObjectId())!.getPosition();

            await mapobj.spawnMonsterOnGroundBelow(9001009, npcpos.X, npcpos.Y);
            await mapobj.destroyNPC(getNpc());
        }
        // Npc: 1104100 
        public Task desguiseSoul()
        {
            return Desguise(Job.DAWNWARRIOR1);
        }

        // Npc: 1104101 
        public Task desguiseFlame()
        {
            return Desguise(Job.BLAZEWIZARD1);
        }


        // Npc: 1104102 
        public Task desguiseWind()
        {
            return Desguise(Job.WINDARCHER1);
        }


        // Npc: 1104103 
        public Task desguiseNight()
        {
            return Desguise(Job.NIGHTWALKER1);
        }


        // Npc: 1104104 
        public Task desguiseStrike()
        {
            return Desguise(Job.THUNDERBREAKER1);
        }

    }
}
