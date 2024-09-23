using Application.Core.model;
using client;
using client.inventory;
using client.inventory.manipulator;
using Microsoft.EntityFrameworkCore;
using server;

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
                dbContext.Rings.Where(x => x.Id == ring.getRingId() || x.Id == ring.getPartnerRingId()).ExecuteDelete();

                CashIdGenerator.freeCashId(ring.getRingId());
                CashIdGenerator.freeCashId(ring.getPartnerRingId());

                dbContext.Inventoryequipments.Where(x => x.RingId == ring.getRingId()).ExecuteUpdate(x => x.SetProperty(y => y.RingId, -1));
                dbContext.Inventoryequipments.Where(x => x.RingId == ring.getPartnerRingId()).ExecuteUpdate(x => x.SetProperty(y => y.RingId, -1));
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
    }
}
