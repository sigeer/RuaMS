using Application.Core.Channel;
using Application.Core.model;
using Application.Shared.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Concurrent;

namespace Application.Core.Gameplay.WorldEvents
{
    public class WeddingWorldInstance
    {
        public World WorldServer { get; }
        readonly ILogger log;

        private Dictionary<int, int> relationships = new();
        private Dictionary<int, CoupleIdPair> relationshipCouples = new();

        private Dictionary<int, KeyValuePair<KeyValuePair<bool, bool>, CoupleIdPair>> queuedMarriages = new();
        private ConcurrentDictionary<int, HashSet<int>> marriageGuests = new();

        public WeddingWorldInstance(World worldServer)
        {
            WorldServer = worldServer;
            log = LogFactory.GetLogger(LogType.Wedding);
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
            queuedMarriages.AddOrUpdate(marriageId, new(new(cathedral, premium), new(groomId, brideId)));
            marriageGuests.AddOrUpdate(marriageId, new());
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
            foreach (var ch in WorldServer.Channels)
            {
                var p = ch.WeddingInstance.GetWeddingCoupleForGuest(guestId, cathedral);
                if (p != null)
                {
                    return p;
                }
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

            int pwSize = possibleWeddings.Count;
            if (pwSize == 0)
            {
                return null;
            }
            else if (pwSize > 1)
            {
                int selectedPw = -1;
                int selectedPos = int.MaxValue;

                foreach (int pw in possibleWeddings)
                {
                    foreach (var ch in WorldServer.Channels)
                    {
                        int pos = ch.WeddingInstance.GetWeddingReservationStatus(pw, cathedral);
                        if (pos != -1)
                        {
                            if (pos < selectedPos)
                            {
                                selectedPos = pos;
                                selectedPw = pw;
                                break;
                            }
                        }
                    }
                }

                if (selectedPw == -1)
                {
                    return null;
                }

                possibleWeddings.Clear();
                possibleWeddings.Add(selectedPw);
            }

            return GetMarriageQueuedCouple(possibleWeddings[0]);
        }

        public void DebugMarriageStatus()
        {
            log.Debug("Queued marriages: " + queuedMarriages);
            log.Debug("Guest list: " + marriageGuests);
        }

        private void PushRelationshipCouple(CoupleTotal couple)
        {
            int mid = couple.MarriageId, hid = couple.HusbandId, wid = couple.WifeId;
            relationshipCouples.AddOrUpdate(mid, new(hid, wid));
            relationships.AddOrUpdate(hid, mid);
            relationships.AddOrUpdate(wid, mid);
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
                log.Error(se.ToString());
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
                log.Error(e.ToString());
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
                log.Error(e.ToString());
            }
        }

    }
}
