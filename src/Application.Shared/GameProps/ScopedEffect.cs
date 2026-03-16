namespace Application.Shared.GameProps
{
    public record ScopedEffect(int StartMapId, int EndMapId, bool PartyEffect)
    {
        public bool IsActive(int mapId, bool hasParty)
        {
            if (PartyEffect && !hasParty)
                return false;

            if (StartMapId == 0 && EndMapId == 0)
                return true;

            return mapId >= StartMapId && mapId <= EndMapId;
        }
    }
}
