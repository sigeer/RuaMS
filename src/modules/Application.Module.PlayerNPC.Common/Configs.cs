namespace Application.Module.PlayerNPC.Common
{
    public class Configs
    {
        /// <summary>
        /// Map frame width for putting PlayerNPCs.
        /// </summary>
        public int PLAYERNPC_INITIAL_X { get; set; } = 262;
        /// <summary>
        /// Map frame height for putting PlayerNPCs.
        /// </summary>
        public int PLAYERNPC_INITIAL_Y { get; set; } = 262;
        /// <summary>
        /// Initial width gap between PlayerNPCs.
        /// </summary>
        public int PLAYERNPC_AREA_X { get; set; } = 320;
        /// <summary>
        /// Initial height gap between PlayerNPCs.
        /// </summary>
        public int PLAYERNPC_AREA_Y { get; set; } = 160;
        /// <summary>
        /// Max number of times gap is shortened to comport PlayerNPCs.
        /// </summary>
        public int PLAYERNPC_AREA_STEPS { get; set; } = 4;
        /// <summary>
        /// Automatically rearranges PlayerNPCs on the map if there is no space set the new NPC. Current distance gap between NPCs is decreased to solve this issue.
        /// </summary>
        public bool PLAYERNPC_ORGANIZE_AREA { get; set; } = true;
        /// <summary>
        /// Makes PlayerNPC automatically deployed on the Hall of Fame at the instant one reaches max level. If false, eligible players must talk to 1st job instructor to deploy a NPC.
        /// </summary>
        public bool PLAYERNPC_AUTODEPLOY { get; set; } = true;
    }
}
