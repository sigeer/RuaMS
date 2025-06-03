using Application.EF;
using Application.EF.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;

namespace Application.Core.Login.Datas
{
    public class WeddingManager
    {
        public MasterServer MasterServer { get; }
        readonly ILogger<WeddingManager> log;

        private Dictionary<int, int> relationships = new();
        private Dictionary<int, CoupleIdPair> relationshipCouples = new();

        private Dictionary<int, KeyValuePair<KeyValuePair<bool, bool>, CoupleIdPair>> queuedMarriages = new();
        private ConcurrentDictionary<int, HashSet<int>> marriageGuests = new();

        public WeddingManager(MasterServer server, ILogger<WeddingManager> logger)
        {
            MasterServer = server;
            log = logger;
        }

        public bool IsMarriageQueued(int marriageId)
        {
            return queuedMarriages.ContainsKey(marriageId);
        }

        public KeyValuePair<bool, bool>? GetMarriageQueuedLocation(int marriageId)
        {
            return queuedMarriages.TryGetValue(marriageId, out var qm) ? qm.Key : null;
        }

        public CoupleIdPair? GetMarriageQueuedCouple(int marriageId)
        {
            return queuedMarriages.TryGetValue(marriageId, out var qm) ? qm.Value : null;
        }

        public void PutMarriageQueued(int marriageId, bool cathedral, bool premium, int groomId, int brideId)
        {
            queuedMarriages[marriageId] = new(new(cathedral, premium), new(groomId, brideId));
            marriageGuests[marriageId] = new();
        }

        public KeyValuePair<bool, HashSet<int>> RemoveMarriageQueued(int marriageId)
        {
            queuedMarriages.Remove(marriageId, out var d);
            marriageGuests.Remove(marriageId, out var guests);

            return new(d.Key.Value, guests);
        }

        public bool AddMarriageGuest(int marriageId, int playerId)
        {
            HashSet<int>? guests = marriageGuests.GetValueOrDefault(marriageId);
            if (guests != null)
            {
                if (guests.Contains(playerId)) return false;

                guests.Add(playerId);
                return true;
            }

            return false;
        }

        public CoupleIdPair? GetWeddingCoupleForGuest(int guestId, bool cathedral)
        {
            var p = MasterServer.Transport.GetAllWeddingCoupleForGuest(guestId, cathedral);
            if (p != null)
            {
                return p;
            }

            List<int> possibleWeddings = new();
            foreach (var mg in marriageGuests)
            {
                if (mg.Value.Contains(guestId))
                {
                    var loc = GetMarriageQueuedLocation(mg.Key);
                    if (loc != null && cathedral.Equals(loc.Value.Key))
                    {
                        possibleWeddings.Add(mg.Key);
                    }
                }
            }

            if (possibleWeddings.Count == 0)
                return null;
            var m = MasterServer.Transport.GetAllWeddingReservationStatus(possibleWeddings, cathedral);
            if (m == -1)
                return null;

            return GetMarriageQueuedCouple(m);
        }

        public void DebugMarriageStatus()
        {
            log.LogDebug("Queued marriages: " + queuedMarriages);
            log.LogDebug("Guest list: " + marriageGuests);
        }

        private void PushRelationshipCouple(CoupleTotal couple)
        {
            int mid = couple.MarriageId, hid = couple.HusbandId, wid = couple.WifeId;
            relationshipCouples[mid] = new(hid, wid);
            relationships[hid] = mid;
            relationships[wid] = mid;
        }

        public CoupleIdPair? GetRelationshipCouple(int relationshipId)
        {
            if (!relationshipCouples.TryGetValue(relationshipId, out var rc))
            {
                var couple = GetRelationshipCoupleFromDb(relationshipId, true);
                if (couple == null)
                {
                    return null;
                }

                PushRelationshipCouple(couple);
                rc = new(couple.HusbandId, couple.WifeId);
            }

            return rc;
        }

        public int GetRelationshipId(int playerId)
        {
            if (!relationships.TryGetValue(playerId, out var value))
            {
                var couple = GetRelationshipCoupleFromDb(playerId, false);
                if (couple == null)
                {
                    return -1;
                }

                PushRelationshipCouple(couple);
                return couple.MarriageId;
            }

            return value;
        }

        private CoupleTotal? GetRelationshipCoupleFromDb(int id, bool usingMarriageId)
        {
            try
            {
                using var dbContext = new DBContext();
                DB_Marriage? model = null;
                if (usingMarriageId)
                {
                    model = dbContext.Marriages.FirstOrDefault(x => x.Marriageid == id);
                }
                else
                {
                    model = dbContext.Marriages.FirstOrDefault(x => x.Husbandid == id || x.Wifeid == id);
                }
                if (model == null)
                    return null;

                return new CoupleTotal(model.Marriageid, model.Husbandid, model.Wifeid);
            }
            catch (Exception se)
            {
                log.LogError(se.ToString());
                return null;
            }
        }

        public int CreateRelationship(int groomId, int brideId)
        {
            int ret = AddRelationshipToDb(groomId, brideId);

            PushRelationshipCouple(new(ret, groomId, brideId));
            return ret;
        }

        private int AddRelationshipToDb(int groomId, int brideId)
        {
            try
            {
                using var dbContext = new DBContext();
                var dbModel = new DB_Marriage
                {
                    Husbandid = groomId,
                    Wifeid = brideId
                };
                dbContext.Marriages.Add(dbModel);
                dbContext.SaveChanges();
                return dbModel.Marriageid;
            }
            catch (Exception e)
            {
                log.LogError(e.ToString());
                return -1;
            }
        }

        public void DeleteRelationship(int playerId, int partnerId)
        {
            int relationshipId = relationships.GetValueOrDefault(playerId);
            DeleteRelationshipFromDb(relationshipId);

            relationshipCouples.Remove(relationshipId);
            relationships.Remove(playerId);
            relationships.Remove(partnerId);
        }

        private void DeleteRelationshipFromDb(int playerId)
        {
            try
            {
                using var dbContext = new DBContext();
                dbContext.Marriages.Where(x => x.Marriageid == playerId).ExecuteDelete();
            }
            catch (Exception e)
            {
                log.LogError(e.ToString());
            }
        }

    }
}
