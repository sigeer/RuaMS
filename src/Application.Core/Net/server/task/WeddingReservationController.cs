using Application.Core.Game.TheWorld;

namespace net.server.task;


public class WeddingReservationController : TimelyControllerBase
{
    IWorldChannel ch;
    protected override void HandleRun()
    {
        KeyValuePair<bool, KeyValuePair<int, HashSet<int>>>? wedding;

        wedding = ch.WeddingInstance.GetNextWeddingReservation(true);   // start cathedral
        if (wedding != null)
        {
            ch.WeddingInstance.SetOngoingWedding(true, wedding.Value.Key, wedding.Value.Value.Key, wedding.Value.Value.Value);
        }
        else
        {
            ch.WeddingInstance.SetOngoingWedding(true, null, null, null);
        }

        wedding = ch.WeddingInstance.GetNextWeddingReservation(false);  // start chapel
        if (wedding != null)
        {
            ch.WeddingInstance.SetOngoingWedding(false, wedding.Value.Key, wedding.Value.Value.Key, wedding.Value.Value.Value);
        }
        else
        {
            ch.WeddingInstance.SetOngoingWedding(false, null, null, null);
        }
    }

    public WeddingReservationController(IWorldChannel world) : base("WeddingReservationController", 
        TimeSpan.FromMinutes(YamlConfig.config.server.WEDDING_RESERVATION_INTERVAL), 
        TimeSpan.FromMinutes(YamlConfig.config.server.WEDDING_RESERVATION_INTERVAL))
    {
        ch = world;
    }
}
