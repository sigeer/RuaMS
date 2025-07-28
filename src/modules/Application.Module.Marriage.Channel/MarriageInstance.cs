using Application.Core.Client;
using Application.Core.Game.Players;
using client.inventory;
using scripting.Event;

namespace Application.Module.Marriage.Channel
{
    public class MarriageInstance : EventInstanceManager
    {
        public List<Item> GroomGiftList { get; set; } = new();
        public List<Item> BrideGiftList { get; set; } = new();

        readonly IChannelServerTransport _transport;
        public MarriageInstance(EventManager em, string name, IChannelServerTransport transport) : base(em, name)
        {
            _transport = transport;
        }

        public bool giftItemToSpouse(int cid)
        {
            return this.getIntProperty("wishlistSelection") == 0;
        }

        public List<string> getWishlistItems(bool groom)
        {
            var strItems = this.getProperty(groom ? "groomWishlist" : "brideWishlist");
            if (strItems != null)
            {
                return strItems.Split("\r\n").ToList();
            }

            return new();
        }

        public void initializeGiftItems()
        {
            GroomGiftList = new();

            BrideGiftList = new();
        }

        public List<Item> getGiftItems(IChannelClient c, bool groom)
        {
            List<Item> gifts = getGiftItemsList(groom);
            lock (gifts)
            {
                return new(gifts);
            }
        }

        private List<Item> getGiftItemsList(bool groom)
        {
            return groom ? GroomGiftList : BrideGiftList;
        }

        public Item? getGiftItem(IChannelClient c, bool groom, int idx)
        {
            try
            {
                return getGiftItems(c, groom).ElementAtOrDefault(idx);
            }
            catch (Exception e)
            {
                log.Error(e.ToString());
                return null;
            }
        }

        public void addGiftItem(bool groom, Item item)
        {
            List<Item> gifts = getGiftItemsList(groom);
            lock (gifts)
            {
                gifts.Add(item);
            }
        }

        public void removeGiftItem(bool groom, Item item)
        {
            List<Item> gifts = getGiftItemsList(groom);
            lock (gifts)
            {
                gifts.Remove(item);
            }
        }

        public bool? isMarriageGroom(IPlayer chr)
        {
            bool? groom = null;
            try
            {
                int groomid = this.getIntProperty("groomId"), brideid = this.getIntProperty("brideId");
                if (chr.getId() == groomid)
                {
                    groom = true;
                }
                else if (chr.getId() == brideid)
                {
                    groom = false;
                }
            }
            catch (Exception e)
            {
                log.Error(e.ToString());
            }

            return groom;
        }

        //public void saveGiftItemsToDb(IChannelClient c, bool groom, int cid)
        //{
        //    Marriage.saveGiftItemsToDb(c, getGiftItems(c, groom), cid);
        //}

        //public static void saveGiftItemsToDb(IChannelClient c, List<Item> giftItems, int cid)
        //{
        //    try
        //    {
        //        using var dbContext = new DBContext();
        //        using var dbTrans = dbContext.Database.BeginTransaction();
        //        ItemFactory.MARRIAGE_GIFTS.saveItems(giftItems.Select(x => new ItemInventoryType(x, x.getInventoryType())).ToList(), cid, dbContext);
        //        dbTrans.Commit();
        //    }
        //    catch (Exception sqle)
        //    {
        //        Log.Logger.Error(sqle.ToString());
        //    }
        //}

        public override void dispose(bool shutdown = false)
        {
            base.dispose(shutdown);
            _transport.CloseWedding(new MarriageProto.CloseWeddingRequest { MarriageId = getIntProperty("weddingId") });
        }
    }

}
