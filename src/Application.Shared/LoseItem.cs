namespace Application.Shared
{
    public struct LoseItem
    {
        public int Id { get; set; }
        public byte Prob { get; set; }
        public byte X { get; set; }

        public LoseItem(int id, byte prob, byte x)
        {
            Id = id;
            Prob = prob;
            X = x;
        }
    }
}
