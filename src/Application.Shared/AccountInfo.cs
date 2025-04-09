namespace Application.Shared
{
    public struct AccountInfo
    {
        public AccountInfo(int id, int world, int accountId)
        {
            Id = id;
            World = world;
            AccountId = accountId;
        }

        public int Id { get; set; }
        public int World { get; set; }
        public int AccountId { get; set; }
    }
}
