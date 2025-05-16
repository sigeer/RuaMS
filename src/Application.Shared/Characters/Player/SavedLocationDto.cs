namespace Application.Shared.Characters
{
    public class SavedLocationDto
    {
        public string Locationtype { get; set; } = null!;

        public int Map { get; set; }

        public int Portal { get; set; }
    }
}
