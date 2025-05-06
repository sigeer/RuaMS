using Application.Core.Game.TheWorld;

namespace net.server.task;

public class PetFullnessController : TimelyControllerBase
{

    private object activePetsLock = new object();
    private Dictionary<int, int> activePets = new();
    private ScheduledFuture? petsSchedule;
    private DateTime petUpdate;

    readonly IWorldChannel worldChannel;

    public PetFullnessController(IWorldChannel worldChannel) : base("PetFullnessController", TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(1))
    {
        petUpdate = DateTime.UtcNow;
        this.worldChannel = worldChannel;
    }

    private int getPetKey(IPlayer chr, sbyte petSlot)
    {
        // assuming max 3 pets
        return (chr.getId() << 2) + petSlot;
    }

    public void registerPetHunger(IPlayer chr, sbyte petSlot)
    {
        if (chr.isGM() && YamlConfig.config.server.GM_PETS_NEVER_HUNGRY || YamlConfig.config.server.PETS_NEVER_HUNGRY)
        {
            return;
        }

        int key = getPetKey(chr, petSlot);

        Monitor.Enter(activePetsLock);
        try
        {
            int initProc;
            if (DateTime.UtcNow - petUpdate > TimeSpan.FromSeconds(55))
            {
                initProc = YamlConfig.config.server.PET_EXHAUST_COUNT - 2;
            }
            else
            {
                initProc = YamlConfig.config.server.PET_EXHAUST_COUNT - 1;
            }

            activePets.AddOrUpdate(key, initProc);
        }
        finally
        {
            Monitor.Exit(activePetsLock);
        }
    }

    public void unregisterPetHunger(IPlayer chr, sbyte petSlot)
    {
        int key = getPetKey(chr, petSlot);

        Monitor.Enter(activePetsLock);
        try
        {
            activePets.Remove(key);
        }
        finally
        {
            Monitor.Exit(activePetsLock);
        }
    }

    protected override void HandleRun()
    {
        Dictionary<int, int> deployedPets;

        Monitor.Enter(activePetsLock);
        try
        {
            petUpdate = DateTime.UtcNow;
            deployedPets = new(activePets);   // exception here found thanks to MedicOP
        }
        finally
        {
            Monitor.Exit(activePetsLock);
        }

        foreach (var dp in deployedPets)
        {
            var chr = worldChannel.getPlayerStorage().getCharacterById(dp.Key / 4);
            if (chr == null || !chr.isLoggedinWorld())
            {
                continue;
            }

            int dpVal = dp.Value + 1;
            if (dpVal == YamlConfig.config.server.PET_EXHAUST_COUNT)
            {
                chr.runFullnessSchedule(dp.Key % 4);
                dpVal = 0;
            }

            Monitor.Enter(activePetsLock);
            try
            {
                activePets.AddOrUpdate(dp.Key, dpVal);
            }
            finally
            {
                Monitor.Exit(activePetsLock);
            }
        }
    }


}
