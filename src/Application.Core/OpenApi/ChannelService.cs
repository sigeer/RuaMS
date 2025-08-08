using Application.Shared.Events;
using net.server;

namespace Application.Core.OpenApi
{
    public class ChannelService
    {
        public List<TravelScheduleItem> GetTravelSchedule()
        {
            //var worldServer = Server.getInstance().getWorld(0);
            //if (worldServer == null)
            //    return new List<TravelScheduleItem>();

            //var traveTypes = Enum.GetValues<TraveType>();
            //return worldServer.getChannels().SelectMany(ch =>
            //{
            //    var emMgn = ch.getEventSM();
            //    return traveTypes.Select(type =>
            //    {
            //        var em = emMgn.getEventManager(type.ToString());
            //        DateTimeOffset? next = em == null ? null : (long.TryParse(em.getProperty("next"), out var p) ? DateTimeOffset.FromUnixTimeMilliseconds(p) : null);
            //        return new TravelScheduleItem()
            //        {
            //            Channel = ch.getId(),
            //            World = 0,
            //            Type = type,
            //            TypeName = type.GetDescription(),
            //            NextTime = next
            //        };
            //    }).ToList();
            //}).ToList();
            return [];
        }
    }
}
