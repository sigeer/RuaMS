using Application.Core.Game.TheWorld;
using Application.Core.Game.Trades;

namespace net.server.task;


public class HiredMerchantController : TimelyControllerBase
{
    private object activeMerchantsLock = new object();
    private Dictionary<int, MerchantOperation> activeMerchants = new();
    private DateTime merchantUpdate;

    readonly IWorldChannel worldChannel;

    public HiredMerchantController(IWorldChannel worldChannel) : base("HiredMerchantController", TimeSpan.FromMinutes(10), TimeSpan.FromMinutes(10))
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

    public void unregisterHiredMerchant(HiredMerchant hm)
    {
        Monitor.Enter(activeMerchantsLock);
        try
        {
            activeMerchants.Remove(hm.getOwnerId());
        }
        finally
        {
            Monitor.Exit(activeMerchantsLock);
        }
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
                    worldChannel.removeHiredMerchant(hm.getOwnerId());

                    activeMerchants.Remove(dm.Key);
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
}
