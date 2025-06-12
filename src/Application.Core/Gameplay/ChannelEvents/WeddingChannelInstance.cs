using Application.Core.Channel;
using Application.Core.Managers;
using server;
using System.Text;

namespace Application.Core.Gameplay.ChannelEvents
{
    public class WeddingChannelInstance
    {
        private List<int> chapelReservationQueue = new();
        private List<int> cathedralReservationQueue = new();
        private ScheduledFuture? chapelReservationTask;
        private ScheduledFuture? cathedralReservationTask;

        private int? ongoingChapel = null;
        private bool? ongoingChapelType = null;
        private HashSet<int>? ongoingChapelGuests = null;
        private int? ongoingCathedral = null;
        private bool? ongoingCathedralType = null;
        private HashSet<int>? ongoingCathedralGuests = null;

        private long ongoingStartTime;

        public WorldChannel ChannelServer { get; }
        readonly ILogger log;

        object lockObj = new object();

        public WeddingChannelInstance(WorldChannel channelServer)
        {
            ChannelServer = channelServer;
            log = LogFactory.GetLogger(LogType.Wedding);

            // rude approach to a world's last channel boot time, placeholder for the 1st wedding reservation ever
            this.ongoingStartTime = DateTimeOffset.UtcNow.AddSeconds(10).ToUnixTimeMilliseconds();
        }


        public KeyValuePair<bool, KeyValuePair<int, HashSet<int>>>? GetNextWeddingReservation(bool cathedral)
        {
            int? ret;

            Monitor.Enter(lockObj);
            try
            {
                List<int> weddingReservationQueue = (cathedral ? cathedralReservationQueue : chapelReservationQueue);
                if (weddingReservationQueue.Count == 0)
                {
                    return null;
                }

                ret = weddingReservationQueue.remove(0);
                if (ret == null)
                {
                    return null;
                }
            }
            finally
            {
                Monitor.Exit(lockObj);
            }


            var coupleId = ChannelServer.Container.Transport.GetMarriageQueuedCouple(ret.Value)!;
            var typeGuests = ChannelServer.Container.Transport.RemoveMarriageQueued(ret.Value);

            CoupleNamePair couple = new(CharacterManager.getNameById(coupleId.HusbandId), CharacterManager.getNameById(coupleId.WifeId));
            ChannelServer.Container.Transport.DropWorldMessage(6, couple.CharacterName1 + " and " + couple.CharacterName2 + "'s wedding is going to be started at " + (cathedral ? "Cathedral" : "Chapel") + " on Channel " + ChannelServer.getId() + ".");

            return new(typeGuests.Key, new(ret.Value, typeGuests.Value));
        }

        public bool IsWeddingReserved(int weddingId)
        {
            Monitor.Enter(lockObj);
            try
            {
                return ChannelServer.Container.Transport.IsMarriageQueued(weddingId) || weddingId.Equals(ongoingCathedral) || weddingId.Equals(ongoingChapel);
            }
            finally
            {
                Monitor.Exit(lockObj);
            }
        }

        public int GetWeddingReservationStatus(int? weddingId, bool cathedral)
        {
            if (weddingId == null)
            {
                return -1;
            }

            Monitor.Enter(lockObj);
            try
            {
                List<int> quene = cathedral ? cathedralReservationQueue : chapelReservationQueue;
                if (weddingId == ongoingCathedral)
                {
                    return 0;
                }

                var idx = quene.IndexOf(weddingId.Value);
                if (idx < 0)
                    return -1;
                return idx + 1;
            }
            finally
            {
                Monitor.Exit(lockObj);
            }
        }

        public int PushWeddingReservation(int? weddingId, bool cathedral, bool premium, int groomId, int brideId)
        {
            if (weddingId == null || IsWeddingReserved(weddingId.Value))
            {
                return -1;
            }

            ChannelServer.Container.Transport.PutMarriageQueued(weddingId.Value, cathedral, premium, groomId, brideId);

            Monitor.Enter(lockObj);
            try
            {
                List<int> weddingReservationQueue = (cathedral ? cathedralReservationQueue : chapelReservationQueue);

                int delay = YamlConfig.config.server.WEDDING_RESERVATION_DELAY - 1 - weddingReservationQueue.Count;
                for (int i = 0; i < delay; i++)
                {
                    weddingReservationQueue.Add(0);  // push empty slots to fill the waiting time
                }

                weddingReservationQueue.Add(weddingId.Value);
                return weddingReservationQueue.Count;
            }
            finally
            {
                Monitor.Exit(lockObj);
            }
        }

        public bool IsOngoingWeddingGuest(bool cathedral, int playerId)
        {
            Monitor.Enter(lockObj);
            try
            {
                if (cathedral)
                {
                    return ongoingCathedralGuests != null && ongoingCathedralGuests.Contains(playerId);
                }
                else
                {
                    return ongoingChapelGuests != null && ongoingChapelGuests.Contains(playerId);
                }
            }
            finally
            {
                Monitor.Exit(lockObj);
            }
        }

        public int GetOngoingWedding(bool cathedral)
        {
            Monitor.Enter(lockObj);
            try
            {
                return (cathedral ? ongoingCathedral : ongoingChapel) ?? 0;
            }
            finally
            {
                Monitor.Exit(lockObj);
            }
        }

        //public bool getOngoingWeddingType(bool cathedral)
        //{
        //    Monitor.Enter(lockObj);
        //    try
        //    {
        //        return (cathedral ? ongoingCathedralType : ongoingChapelType) ?? false;
        //    }
        //    finally
        //    {
        //        Monitor.Exit(lockObj);
        //    }
        //}

        public void CloseOngoingWedding(bool cathedral)
        {
            Monitor.Enter(lockObj);
            try
            {
                if (cathedral)
                {
                    ongoingCathedral = null;
                    ongoingCathedralType = null;
                    ongoingCathedralGuests = null;
                }
                else
                {
                    ongoingChapel = null;
                    ongoingChapelType = null;
                    ongoingChapelGuests = null;
                }
            }
            finally
            {
                Monitor.Exit(lockObj);
            }
        }

        public void SetOngoingWedding(bool cathedral, bool? premium, int? weddingId, HashSet<int>? guests)
        {
            Monitor.Enter(lockObj);
            try
            {
                if (cathedral)
                {
                    ongoingCathedral = weddingId;
                    ongoingCathedralType = premium;
                    ongoingCathedralGuests = guests;
                }
                else
                {
                    ongoingChapel = weddingId;
                    ongoingChapelType = premium;
                    ongoingChapelGuests = guests;
                }
            }
            finally
            {
                Monitor.Exit(lockObj);
            }

            ongoingStartTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            if (weddingId != null)
            {
                ScheduledFuture? weddingTask = TimerManager.getInstance().schedule(() => CloseOngoingWedding(cathedral), TimeSpan.FromMinutes(YamlConfig.config.server.WEDDING_RESERVATION_TIMEOUT));

                if (cathedral)
                {
                    cathedralReservationTask = weddingTask;
                }
                else
                {
                    chapelReservationTask = weddingTask;
                }
            }
        }


        object weddingLock = new object();
        public bool AcceptOngoingWedding(bool cathedral)
        {
            lock (weddingLock)
            {
                // couple succeeded to show up and started the ceremony
                if (cathedral)
                {
                    if (cathedralReservationTask == null)
                    {
                        return false;
                    }

                    cathedralReservationTask.cancel(false);
                    cathedralReservationTask = null;
                }
                else
                {
                    if (chapelReservationTask == null)
                    {
                        return false;
                    }

                    chapelReservationTask.cancel(false);
                    chapelReservationTask = null;
                }

                return true;
            }
        }

        private static string? GetTimeLeft(long futureTime)
        {
            StringBuilder str = new StringBuilder();
            long leftTime = futureTime - DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

            if (leftTime < 0)
            {
                return null;
            }

            byte mode = 0;
            if ((leftTime / 60 * 1000) > 0)
            {
                mode++;     //counts minutes

                if (leftTime / (3600 * 1000) > 0)
                {
                    mode++;     //counts hours
                }
            }

            switch (mode)
            {
                case 2:
                    int hours = (int)((leftTime / (3600 * 1000)));
                    str.Append(hours + " hours, ");
                    break;
                case 1:
                    int minutes = (int)((leftTime / (60 * 1000)) % 60);
                    str.Append(minutes + " minutes, ");
                    break;
                default:
                    int seconds = (int)(leftTime / 1000) % 60;
                    str.Append(seconds + " seconds");
                    break;
            }

            return str.ToString();
        }

        public long GetWeddingTicketExpireTime(int resSlot)
        {
            return ongoingStartTime + CommonManager.GetRelativeWeddingTicketExpireTime(resSlot);
        }


        public string? GetWeddingReservationTimeLeft(int? weddingId)
        {
            if (weddingId == null)
            {
                return null;
            }

            Monitor.Enter(lockObj);
            try
            {
                bool cathedral = true;

                int resStatus;
                resStatus = GetWeddingReservationStatus(weddingId, true);
                if (resStatus < 0)
                {
                    cathedral = false;
                    resStatus = GetWeddingReservationStatus(weddingId, false);

                    if (resStatus < 0)
                    {
                        return null;
                    }
                }

                string venue = (cathedral ? "Cathedral" : "Chapel");
                if (resStatus == 0)
                {
                    return venue + " - RIGHT NOW";
                }

                return venue + " - " + GetTimeLeft(ongoingStartTime + (long)resStatus * YamlConfig.config.server.WEDDING_RESERVATION_INTERVAL * 60 * 1000) + " from now";
            }
            finally
            {
                Monitor.Exit(lockObj);
            }
        }

        public CoupleIdPair? GetWeddingCoupleForGuest(int guestId, bool cathedral)
        {
            Monitor.Enter(lockObj);
            try
            {
                return (IsOngoingWeddingGuest(cathedral, guestId)) ? ChannelServer.Container.Transport.GetRelationshipCouple(GetOngoingWedding(cathedral)) : null;
            }
            finally
            {
                Monitor.Exit(lockObj);
            }
        }
    }
}
