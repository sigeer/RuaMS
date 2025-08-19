namespace Application.Shared.Models
{
    public record ObjectName(int Id, string Name);

    public record NpcObjectName(int Id, string Name, string DefaultTalk);

    public record MapName(int Id, string PlaceName, string StreetName);
}
