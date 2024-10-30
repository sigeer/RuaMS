using server.maps;

namespace Application.EF.Entities;

public class SavedLocationEntity
{
    private SavedLocationEntity()
    {
    }

    public SavedLocationEntity(int map, int portal, int charId, string locationType)
    {
        Map = map;
        Portal = portal;
        Characterid = charId;
        Locationtype = locationType;
    }

    public SavedLocationEntity(int charId, SavedLocation data, SavedLocationType type) : this(data.getMapId(), data.getPortal(), charId, type.ToString())
    {
    }

    public int Id { get; set; }

    public int Characterid { get; set; }

    public string Locationtype { get; set; } = null!;

    public int Map { get; set; }

    public int Portal { get; set; }
}
