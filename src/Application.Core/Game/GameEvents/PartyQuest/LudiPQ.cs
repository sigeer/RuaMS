namespace Application.Core.Game.GameEvents.PartyQuest
{
    public class LudiPQ : PlayerPartyQuestBase
    {
        public LudiPQ(Player player) : base("LudiPQ", "lpq", player)
        {
            EntryNpcId = 2040034;
        }

        public override int GetStageFromMap(int mapId) => ((mapId - FirstMapId) / 100) + 1;
        public override int ClearMapId => 922010900;
    }
}
