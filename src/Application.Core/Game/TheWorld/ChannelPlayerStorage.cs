namespace Application.Core.Game.TheWorld
{
    public class ChannelPlayerStorage
    {
        private Dictionary<int, IPlayer> storage = new();
        private Dictionary<string, IPlayer> nameStorage = new();
        private ReaderWriterLockSlim locks = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);

        public event EventHandler<IPlayer>? OnChannelAddPlayer;


        public void AddPlayer(IPlayer chr)
        {
            locks.EnterWriteLock();
            try
            {
                storage.AddOrUpdate(chr.Id, chr);
                nameStorage.AddOrUpdate(chr.Name.ToLower(), chr);
                OnChannelAddPlayer?.Invoke(this, chr);
            }
            finally
            {
                locks.ExitWriteLock();
            }
        }

        public IPlayer? RemovePlayer(int chr)
        {
            locks.EnterWriteLock();
            try
            {
                storage.Remove(chr, out var mc);
                if (mc != null)
                    nameStorage.Remove(mc.Name.ToLower());

                return mc;
            }
            finally
            {
                locks.ExitWriteLock();
            }
        }
        public IPlayer? this[string name] => getCharacterByName(name);
        public IPlayer? getCharacterByName(string name)
        {
            locks.EnterReadLock();
            try
            {
                return nameStorage.GetValueOrDefault(name.ToLower());
            }
            finally
            {
                locks.ExitReadLock();
            }
        }
        public IPlayer? this[int id] => getCharacterById(id);
        public IPlayer? getCharacterById(int id)
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

        public ICollection<IPlayer> getAllCharacters()
        {
            locks.EnterReadLock();
            try
            {
                return new List<IPlayer>(storage.Values);
            }
            finally
            {
                locks.ExitReadLock();
            }
        }

        public void disconnectAll()
        {
            List<IPlayer> chrList;
            locks.EnterReadLock();
            try
            {
                chrList = new(storage.Values);
            }
            finally
            {
                locks.ExitReadLock();
            }

            foreach (IPlayer mc in chrList)
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
