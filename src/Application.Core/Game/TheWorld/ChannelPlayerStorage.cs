namespace Application.Core.Game.TheWorld
{
    public class ChannelPlayerStorage
    {
        private Dictionary<int, Player> storage = new();
        private Dictionary<string, Player> nameStorage = new();

        public event EventHandler<Player>? OnChannelAddPlayer;


        public void AddPlayer(Player chr)
        {
            storage.AddOrUpdate(chr.Id, chr);
            nameStorage.AddOrUpdate(chr.Name, chr);
            OnChannelAddPlayer?.Invoke(this, chr);
        }

        public Player? RemovePlayer(int chr)
        {
            storage.Remove(chr, out var mc);
            if (mc != null)
                nameStorage.Remove(mc.Name);

            return mc;

        }
        public Player? this[string name] => getCharacterByName(name);
        public Player? getCharacterByName(string name)
        {
            return nameStorage.GetValueOrDefault(name);
        }
        public Player? this[int id] => getCharacterById(id);
        public Player? getCharacterById(int id)
        {
            return storage.GetValueOrDefault(id);
        }

        public List<Player> getAllCharacters()
        {
            return new List<Player>(storage.Values);
        }
        public async Task disconnectAll(bool includeGM)
        {
            List<Player> chrList;

            chrList = new(storage.Values);
            foreach (Player mc in chrList)
            {
                if (mc.IsOnlined && (includeGM || !mc.isGM()))
                {
                    mc.Client.ForceDisconnect();
                }
            }

            storage.Clear();
        }

        public int Count()
        {
            return storage.Count;
        }
    }
}
