namespace Application.Shared.Characters
{
    public class SavedLocationDto
    {
        public int Id { get; set; }
        public int Characterid { get; set; }
        public string Locationtype { get; set; } = null!;

        public int Map { get; set; }

        public int Portal { get; set; }
    }
}
