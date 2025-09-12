namespace Application.Templates.Item.Pet
{
    public sealed class PetInterActData
    {
        [WZPath("interact/-/$name")]
        public int Id { get; set; }
        public int Prob { get; set; }
        public int Inc { get; set; }
    }
}
