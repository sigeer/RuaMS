namespace Application.Core.Game.TheWorld
{
    public class ChannelPlayerStorage
    {
        private Dictionary<int, Player> storage = new();
        private Dictionary<string, Player> nameStorage = new();
        private ReaderWriterLockSlim locks = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);

        public event EventHandler<Player>? OnChannelAddPlayer;


        public void AddPlayer(Player chr)
        {
            locks.EnterWriteLock();
            try
            {
                storage.AddOrUpdate(chr.Id, chr);
                nameStorage.AddOrUpdate(chr.Name, chr);
                OnChannelAddPlayer?.Invoke(this, chr);
            }
            finally
            {
                locks.ExitWriteLock();
            }
        }

        public Player? RemovePlayer(int chr)
        {
            locks.EnterWriteLock();
            try
            {
                storage.Remove(chr, out var mc);
                if (mc != null)
                    nameStorage.Remove(mc.Name);

                return mc;
            }
            finally
            {
                locks.ExitWriteLock();
            }
        }
        public Player? this[string name] => getCharacterByName(name);
        public Player? getCharacterByName(string name)
        {
            locks.EnterReadLock();
            try
            {
                return nameStorage.GetValueOrDefault(name);
            }
            finally
            {
                locks.ExitReadLock();
            }
        }
        public Player? this[int id] => getCharacterById(id);
        public Player? getCharacterById(int id)
        {
            locks.EnterReadLock();
            try
            {
                return storage.GetValueOrDefault(id);
            }
            finally
            {
                locks.ExitReadLock();
            }
        }

        public List<Player> getAllCharacters()
        {
            locks.EnterReadLock();
            try
            {
                return new List<Player>(storage.Values);
            }
            finally
            {
                locks.ExitReadLock();
            }
        }
        public void disconnectAll()
        {
            List<Player> chrList;
            locks.EnterReadLock();
            try
            {
                chrList = new(storage.Values);
            }
            finally
            {
                locks.ExitReadLock();
            }

            foreach (Player mc in chrList)
            {
                if (mc.IsOnlined)
                {
                    mc.Client.ForceDisconnect();
                }
            }

            locks.EnterWriteLock();
            try
            {
                storage.Clear();
            }
            finally
            {
                locks.ExitWriteLock();
            }
        }

        public int Count()
        {
            locks.EnterReadLock();
            try
            {
                return storage.Count;
            }
            finally
            {
                locks.ExitReadLock();
            }
        }
    }
}
