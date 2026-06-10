using Application.Shared.Constants.Job;

namespace Application.Plugin.Script.Npc
{
    internal partial class NpcScript
    {
        async Task Desguise(Job baseJob)
        {
            if (!getJob().IsSameJobGroup(baseJob))
            {
                await SayOK("Hello there, #h0#. Are you helping us finding the intruder? He is not in this area, I've already searched here.");
                return;
            }

            await SayOK("Darn, you found me! Then, there's only one way out! Let's fight, like #rBlack Wings#k should!");
            var mapobj = getMap();
            var npcpos = mapobj.getMapObject(getNpcObjectId())!.getPosition();

            mapobj.spawnMonsterOnGroundBelow(9001009, npcpos.X, npcpos.Y);
            mapobj.destroyNPC(getNpc());
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
