using Application.Core.Channel.DataProviders;
using Application.Core.Game.Relation;
using Application.Core.model;
using client.inventory;
using client.inventory.manipulator;
using Microsoft.EntityFrameworkCore;
using tools.packets;

namespace Application.Core.Managers
{
    public class RingManager
    {
        public static RingPair? CreateRing(int itemid, IPlayer partner1, IPlayer partner2)
        {
            try
            {
                if (partner1 == null || partner2 == null)
                    return null;

                long[] ringID = new long[2];
                ringID[0] = Yitter.IdGenerator.YitIdHelper.NextId();
                ringID[1] = Yitter.IdGenerator.YitIdHelper.NextId();

                return new RingPair(
                    new Ring(ringID[0], ringID[1], partner2.getId(), itemid, partner2.getName()),
                    new Ring(ringID[1], ringID[0], partner1.getId(), itemid, partner1.getName()));
            }
            catch (Exception ex)
            {
                Log.Logger.Error(ex.ToString());
                return null;
            }
        }

        // js
        public static void GiveMarriageRings(IPlayer player, IPlayer partner, int marriageRingItemId)
        {
            var rings = CreateRing(marriageRingItemId, player, partner);
            var ii = ItemInformationProvider.getInstance();

            Item ringObj = ii.getEquipById(marriageRingItemId);
            Equip ringEqp = (Equip)ringObj;
            ringEqp.Ring = rings.MyRing;
            player.addMarriageRing(rings.MyRing);
            InventoryManipulator.addFromDrop(player.Client, ringEqp, false);
            player.broadcastMarriageMessage();

            ringObj = ii.getEquipById(marriageRingItemId);
            ringEqp = (Equip)ringObj;
            ringEqp.Ring = rings.PartnerRing;
            partner.addMarriageRing(rings.PartnerRing);
            InventoryManipulator.addFromDrop(partner.Client, ringEqp, false);
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

                chr.getChannelServer().Container.Transport.DeleteRelationship(chr.getId(), partnerid);
                var marriageRing = chr.getMarriageRing();

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

                chr.dropMessage(5, "You have successfully break the marriage with " + CharacterManager.getNameById(partnerid) + ".");

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
                eqp.Ring = null;
            }
        }

        static object breakEngagementLock = new object();
        private static void BreakEngagement(IPlayer chr)
        {
            lock (breakEngagementLock)
            {


                int partnerid = chr.getPartnerId();
                int marriageitemid = chr.getMarriageItemId();

                chr.getChannelServer().Container.Transport.DeleteRelationship(chr.getId(), partnerid);

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
                        InventoryManipulator.removeById(partner.Client, InventoryType.ETC, partnerMarriageitemid, 1, false, false);
                    }

                    //partner.sendPacket(Wedding.OnMarriageResult((byte) 0)); ok, how to gracefully unengage someone without the need to cc?
                    partner.sendPacket(WeddingPackets.OnNotifyWeddingPartnerTransfer(0, 0));
                    partner.setPartnerId(-1);
                    partner.setMarriageItemId(-1);
                }

                if (chr.haveItem(marriageitemid))
                {
                    InventoryManipulator.removeById(chr.Client, InventoryType.ETC, marriageitemid, 1, false, false);
                }
                chr.dropMessage(5, "You have successfully break the engagement with " + CharacterManager.getNameById(partnerid) + ".");

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
