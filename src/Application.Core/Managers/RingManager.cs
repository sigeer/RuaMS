using Application.Core.Game.Relation;
using Application.Core.model;
using client.inventory;
using client.inventory.manipulator;
using Microsoft.EntityFrameworkCore;
using server;
using tools.packets;

namespace Application.Core.Managers
{
    public class RingManager
    {
        public static Ring? LoadFromDb(int ringId)
        {
            using var dbContext = new DBContext();
            return dbContext.Rings.Where(x => x.Id == ringId).ToList().Select(x => new Ring(x.Id, x.PartnerRingId, x.PartnerChrId, x.ItemId, x.PartnerName)).FirstOrDefault();
        }


        public static void RemoveRing(Ring? ring)
        {
            if (ring == null)
                return;

            try
            {
                using var dbContext = new DBContext();
                using var dbTrans = dbContext.Database.BeginTransaction();
                dbContext.Rings.Where(x => x.Id == ring.getRingId() || x.Id == ring.getPartnerRingId()).ExecuteDelete();

                CashIdGenerator.freeCashId(ring.getRingId());
                CashIdGenerator.freeCashId(ring.getPartnerRingId());

                dbContext.Inventoryequipments.Where(x => x.RingId == ring.getRingId()).ExecuteUpdate(x => x.SetProperty(y => y.RingId, -1));
                dbContext.Inventoryequipments.Where(x => x.RingId == ring.getPartnerRingId()).ExecuteUpdate(x => x.SetProperty(y => y.RingId, -1));
                dbTrans.Commit();
            }
            catch (Exception ex)
            {
                Log.Logger.Error(ex.ToString());
            }
        }

        public static RingPair CreateRing(int itemid, IPlayer partner1, IPlayer partner2)
        {
            try
            {
                if (partner1 == null)
                    return new(-3, -3);

                if (partner2 == null)
                    return new(-2, -2);

                int[] ringID = new int[2];
                ringID[0] = CashIdGenerator.generateCashId();
                ringID[1] = CashIdGenerator.generateCashId();

                using var dbContext = new DBContext();
                var dbModel = new Ring_Entity(ringID[0], itemid, ringID[1], partner2.getId(), partner2.getName());

                dbContext.Rings.Add(dbModel);

                var dbRelatedModel = new Ring_Entity(ringID[1], itemid, ringID[0], partner1.getId(), partner1.getName());
                dbContext.Rings.Add(dbRelatedModel);

                dbContext.SaveChanges();
                return new(ringID[0], ringID[1]);
            }
            catch (Exception ex)
            {
                Log.Logger.Error(ex.ToString());
                return new(-1, -1);
            }
        }

        // js
        public static void GiveMarriageRings(IPlayer player, IPlayer partner, int marriageRingId)
        {
            var rings = CreateRing(marriageRingId, player, partner);
            var ii = ItemInformationProvider.getInstance();

            Item ringObj = ii.getEquipById(marriageRingId);
            Equip ringEqp = (Equip)ringObj;
            ringEqp.setRingId(rings.MyRingId);
            player.addMarriageRing(LoadFromDb(rings.MyRingId));
            InventoryManipulator.addFromDrop(player.getClient(), ringEqp, false, -1);
            player.broadcastMarriageMessage();

            ringObj = ii.getEquipById(marriageRingId);
            ringEqp = (Equip)ringObj;
            ringEqp.setRingId(rings.PartnerRingId);
            partner.addMarriageRing(LoadFromDb(rings.PartnerRingId));
            InventoryManipulator.addFromDrop(partner.getClient(), ringEqp, false, -1);
            partner.broadcastMarriageMessage();
        }

        public static void BreakMarriageRing(IPlayer chr, int wItemId)
        {
            InventoryType type = InventoryTypeUtils.getByType((sbyte)(wItemId / 1000000));
            var wItem = chr.getInventory(type).findById(wItemId);
            bool weddingToken = (wItem != null && type == InventoryType.ETC && wItemId / 10000 == 403);
            bool weddingRing = (wItem != null && wItemId / 10 == 111280);

            if (weddingRing)
            {
                if (chr.getPartnerId() > 0)
                {
                    BreakMarriage(chr);
                }

                chr.getMap().disappearingItemDrop(chr, chr, wItem!, chr.getPosition());
            }
            else if (weddingToken)
            {
                if (chr.getPartnerId() > 0)
                {
                    BreakEngagement(chr);
                }

                chr.getMap().disappearingItemDrop(chr, chr, wItem!, chr.getPosition());
            }
        }

        static object breakMarriageLock = new object();
        private static void BreakMarriage(IPlayer chr)
        {
            lock (breakMarriageLock)
            {
                int partnerid = chr.getPartnerId();
                if (partnerid <= 0)
                {
                    return;
                }

                chr.getChannelServer().Transport.DeleteRelationship(chr.getId(), partnerid);
                RingManager.RemoveRing(chr.getMarriageRing());

                var partner = chr.getWorldServer().getPlayerStorage().getCharacterById(partnerid);
                if (partner == null || !partner.IsOnlined)
                {
                    EraseEngagementOffline(partnerid);
                }
                else
                {
                    partner.dropMessage(5, chr.getName() + " has decided to break up the marriage.");

                    //partner.sendPacket(Wedding.OnMarriageResult((byte) 0)); ok, how to gracefully unengage someone without the need to cc?
                    partner.sendPacket(WeddingPackets.OnNotifyWeddingPartnerTransfer(0, 0));
                    ResetRingId(partner);
                    partner.setPartnerId(-1);
                    partner.setMarriageItemId(-1);
                    partner.addMarriageRing(null);
                }

                chr.dropMessage(5, "You have successfully break the marriage with " + partner?.Name + ".");

                //chr.sendPacket(Wedding.OnMarriageResult((byte) 0));
                chr.sendPacket(WeddingPackets.OnNotifyWeddingPartnerTransfer(0, 0));
                ResetRingId(chr);
                chr.setPartnerId(-1);
                chr.setMarriageItemId(-1);
                chr.addMarriageRing(null);
            }
        }

        private static void ResetRingId(IPlayer player)
        {
            int ringitemid = player.getMarriageRing()!.getItemId();

            var it = player.getInventory(InventoryType.EQUIP).findById(ringitemid) ?? player.getInventory(InventoryType.EQUIPPED).findById(ringitemid);
            if (it != null)
            {
                Equip eqp = (Equip)it;
                eqp.setRingId(-1);
            }
        }

        static object breakEngagementLock = new object();
        private static void BreakEngagement(IPlayer chr)
        {
            lock (breakEngagementLock)
            {


                int partnerid = chr.getPartnerId();
                int marriageitemid = chr.getMarriageItemId();

                chr.getChannelServer().Transport.DeleteRelationship(chr.getId(), partnerid);

                var partner = chr.getWorldServer().getPlayerStorage().getCharacterById(partnerid);
                if (partner == null || !partner.IsOnlined)
                {
                    breakEngagementOffline(partnerid);
                }
                else
                {
                    partner.dropMessage(5, chr.getName() + " has decided to break up the engagement.");

                    int partnerMarriageitemid = marriageitemid + ((chr.getGender() == 0) ? 1 : -1);
                    if (partner.haveItem(partnerMarriageitemid))
                    {
                        InventoryManipulator.removeById(partner.getClient(), InventoryType.ETC, partnerMarriageitemid, 1, false, false);
                    }

                    //partner.sendPacket(Wedding.OnMarriageResult((byte) 0)); ok, how to gracefully unengage someone without the need to cc?
                    partner.sendPacket(WeddingPackets.OnNotifyWeddingPartnerTransfer(0, 0));
                    partner.setPartnerId(-1);
                    partner.setMarriageItemId(-1);
                }

                if (chr.haveItem(marriageitemid))
                {
                    InventoryManipulator.removeById(chr.getClient(), InventoryType.ETC, marriageitemid, 1, false, false);
                }
                chr.dropMessage(5, "You have successfully break the engagement with " + partner?.Name + ".");

                //chr.sendPacket(Wedding.OnMarriageResult((byte) 0));
                chr.sendPacket(WeddingPackets.OnNotifyWeddingPartnerTransfer(0, 0));
                chr.setPartnerId(-1);
                chr.setMarriageItemId(-1);
            }
        }

        private static void EraseEngagementOffline(int characterId)
        {
            try
            {
                using var dbContext = new DBContext();
                EraseEngagementOffline(characterId, dbContext);
            }
            catch (Exception e)
            {
                Log.Logger.Error(e.ToString());
            }
        }

        private static void EraseEngagementOffline(int characterId, DBContext dbContext)
        {
            dbContext.Characters.Where(x => x.Id == characterId).ExecuteUpdate(x => x.SetProperty(y => y.MarriageItemId, -1).SetProperty(y => y.PartnerId, -1));
        }

        private static void breakEngagementOffline(int characterId)
        {
            try
            {
                using var dbContext = new DBContext();
                var dataItem = dbContext.Characters.Where(x => x.Id == characterId).Select(x => new { x.MarriageItemId }).FirstOrDefault();
                if (dataItem != null)
                {
                    int marriageItemId = dataItem.MarriageItemId;

                    if (marriageItemId > 0)
                    {
                        dbContext.Inventoryitems.Where(x => x.Itemid == marriageItemId && x.Characterid == characterId)
                                .ExecuteUpdate(x => x.SetProperty(y => y.Expiration, 0));
                    }
                }

                EraseEngagementOffline(characterId, dbContext);
            }
            catch (Exception ex)
            {
                Log.Logger.Error("Error updating offline breakup " + ex.Message);
            }
        }
    }
}
