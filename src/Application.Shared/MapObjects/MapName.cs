namespace Application.Shared.MapObjects
{
    public struct MapName
    {
        public string StreetName { get; set; }
        public string Name { get; set; }
        public MapName(string streetName, string name)
        {
            StreetName = streetName;
            Name = name;
        }
    }
}
