using Microsoft.EntityFrameworkCore;

namespace Application.Core.Managers
{
    public class MTSManager
    {
        public static void CancelMtsSale(int itemId, IPlayer seller)
        {
            try
            {
                using var dbContext = new DBContext();
                using var dbTrans = dbContext.Database.BeginTransaction();
                dbContext.MtsItems.Where(x => x.Id == itemId && x.Seller == seller.Id).ExecuteUpdate(x => x.SetProperty(y => y.Transfer, 1));
                dbContext.MtsCarts.Where(x => x.Itemid == itemId).ExecuteDelete();
                dbTrans.Commit();
            }
            catch (Exception ex)
            {
                Log.Logger.Error(ex.ToString());
            }
        }

        public static void AddToCart(int itemId, IPlayer player)
        {
            try
            {
                using var dbContext = new DBContext();
                var hasItem = dbContext.MtsItems.Any(x => x.Id == itemId && x.Seller != player.Id);
                if (hasItem)
                {
                    var hasCart = dbContext.MtsCarts.Any(x => x.Cid == player.Id && x.Itemid == itemId);
                    if (!hasCart)
                    {
                        dbContext.MtsCarts.Add(new MtsCart { Cid = player.Id, Itemid = itemId });
                        dbContext.SaveChanges();
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Logger.Error(ex.ToString());
            }
        }

        public static void DeleteCart(int itemId, IPlayer player)
        {
            try
            {
                using var dbContext = new DBContext();
                dbContext.MtsCarts.Where(x => x.Cid == player.Id && x.Itemid == itemId).ExecuteDelete();
            }
            catch (Exception ex)
            {
                Log.Logger.Error(ex.ToString());
            }
        }
    }
}
