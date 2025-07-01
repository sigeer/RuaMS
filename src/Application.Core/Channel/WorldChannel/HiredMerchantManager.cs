using Application.Core.Game.Controllers;
using Application.Core.Game.Trades;

namespace Application.Core.Channel
{
    public class HiredMerchantManager : TimelyControllerBase
    {
        private object activeMerchantsLock = new object();
        private Dictionary<int, MerchantOperation> activeMerchants = new();
        private DateTime merchantUpdate;

        readonly WorldChannel worldChannel;

        public HiredMerchantManager(WorldChannel worldChannel) : base($"HiredMerchantController_{worldChannel.ServerName}", TimeSpan.FromMinutes(10), TimeSpan.FromMinutes(10))
        {
            this.worldChannel = worldChannel;
        }

        public void registerHiredMerchant(HiredMerchant hm)
        {
            Monitor.Enter(activeMerchantsLock);
            try
            {
                int initProc;
                if (DateTime.UtcNow - merchantUpdate > TimeSpan.FromMinutes(5))
                {
                    initProc = 1;
                }
                else
                {
                    initProc = 0;
                }

                activeMerchants.AddOrUpdate(hm.getOwnerId(), new(hm, initProc));
            }
            finally
            {
                Monitor.Exit(activeMerchantsLock);
            }
        }

        public void unregisterHiredMerchant(int ownerId)
        {
            Monitor.Enter(activeMerchantsLock);
            try
            {
                activeMerchants.Remove(ownerId);
            }
            finally
            {
                Monitor.Exit(activeMerchantsLock);
            }
        }

        public void unregisterHiredMerchant(HiredMerchant hm)
        {
            unregisterHiredMerchant(hm.getOwnerId());
        }

        protected override void HandleRun()
        {
            Dictionary<int, MerchantOperation> deployedMerchants;
            Monitor.Enter(activeMerchantsLock);
            try
            {
                merchantUpdate = DateTime.UtcNow;
                deployedMerchants = new(activeMerchants);

                foreach (var dm in deployedMerchants)
                {
                    int timeOn = dm.Value.TimeWorked;
                    HiredMerchant hm = dm.Value.HiredMerchant;

                    if (timeOn <= 144)
                    {
                        // 1440 minutes == 24hrs
                        activeMerchants.AddOrUpdate(hm.getOwnerId(), new(hm, timeOn + 1));
                    }
                    else
                    {
                        hm.forceClose();

                        unregisterHiredMerchant(dm.Key);
                    }
                }
            }
            finally
            {
                Monitor.Exit(activeMerchantsLock);
            }
        }

        public List<HiredMerchant> getActiveMerchants()
        {
            Monitor.Enter(activeMerchantsLock);
            try
            {
                return activeMerchants.Where(x => x.Value.HiredMerchant.isOpen()).Select(x => x.Value.HiredMerchant).ToList();
            }
            finally
            {
                Monitor.Exit(activeMerchantsLock);
            }
        }

        public HiredMerchant? getHiredMerchant(int ownerid)
        {
            Monitor.Enter(activeMerchantsLock);
            try
            {
                return activeMerchants.TryGetValue(ownerid, out var value) ? value.HiredMerchant : null;
            }
            finally
            {
                Monitor.Exit(activeMerchantsLock);
            }
        }

        //private ReaderWriterLockSlim merchLock = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);
        //private Dictionary<int, HiredMerchant> hiredMerchants = new();
        //public Dictionary<int, HiredMerchant> getHiredMerchants()
        //{
        //    merchLock.EnterReadLock();
        //    try
        //    {
        //        return new Dictionary<int, HiredMerchant>(hiredMerchants);
        //    }
        //    finally
        //    {
        //        merchLock.ExitReadLock();
        //    }
        //}

        //public void addHiredMerchant(int chrid, HiredMerchant hm)
        //{
        //    merchLock.EnterWriteLock();
        //    try
        //    {
        //        hiredMerchants[chrid] = hm;
        //    }
        //    finally
        //    {
        //        merchLock.ExitWriteLock();
        //    }
        //}

        //public void removeHiredMerchant(int chrid)
        //{
        //    merchLock.EnterWriteLock();
        //    try
        //    {
        //        hiredMerchants.Remove(chrid);
        //    }
        //    finally
        //    {
        //        merchLock.ExitWriteLock();
        //    }
        //}
    }
}
