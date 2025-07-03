using Application.Core.Game.Trades;
using Microsoft.EntityFrameworkCore;

namespace Application.Core.Game.Players
{
    public partial class Player
    {
        /// <summary>
        /// 正在访问的雇佣商人，而不是自己的雇佣商人
        /// </summary>
        private HiredMerchant? hiredMerchant = null;
        public void setHasMerchant(bool set)
        {
            HasMerchant = set;
        }

        public void addMerchantMesos(int add)
        {
            int newAmount = (int)Math.Min((long)MerchantMesos + add, int.MaxValue);

            setMerchantMeso(newAmount);
        }

        public void setMerchantMeso(int set)
        {
            MerchantMesos = set;
        }

        object withDrawMerchantLock = new object();
        public void withdrawMerchantMesos()
        {
            lock (withDrawMerchantLock)
            {
                int merchantMeso = this.getMerchantNetMeso();
                int playerMeso = this.getMeso();

                if (merchantMeso > 0)
                {
                    int possible = int.MaxValue - playerMeso;

                    if (possible > 0)
                    {
                        if (possible < merchantMeso)
                        {
                            this.gainMeso(possible, false);
                            this.setMerchantMeso(merchantMeso - possible);
                        }
                        else
                        {
                            this.gainMeso(merchantMeso, false);
                            this.setMerchantMeso(0);
                        }
                    }
                }
                else
                {
                    int nextMeso = playerMeso + merchantMeso;

                    if (nextMeso < 0)
                    {
                        this.gainMeso(-playerMeso, false);
                        this.setMerchantMeso(merchantMeso + playerMeso);
                    }
                    else
                    {
                        this.gainMeso(merchantMeso, false);
                        this.setMerchantMeso(0);
                    }
                }
            }

        }

        public void setHiredMerchant(HiredMerchant? merchant)
        {
            this.hiredMerchant = merchant;
        }


        public int getMerchantMeso()
        {
            return MerchantMesos;
        }

        public int getMerchantNetMeso()
        {
            int elapsedDays = 0;

            using var dbContext = new DBContext();
            var dbModel = dbContext.Fredstorages.Where(x => x.Cid == getId()).Select(x => new { x.Timestamp }).FirstOrDefault();
            if (dbModel != null)
                elapsedDays = TimeUtils.DayDiff(dbModel.Timestamp, DateTimeOffset.UtcNow);

            if (elapsedDays > 100)
            {
                elapsedDays = 100;
            }

            long netMeso = MerchantMesos; // negative mesos issues found thanks to Flash, Vcoc
            netMeso = (netMeso * (100 - elapsedDays)) / 100;
            return (int)netMeso;
        }
        public HiredMerchant? getHiredMerchant()
        {
            return hiredMerchant;
        }
        public void closeHiredMerchant(bool closeMerchant)
        {
            HiredMerchant? merchant = this.getHiredMerchant();
            if (merchant == null)
            {
                return;
            }

            if (closeMerchant)
            {
                if (merchant.isOwner(this) && merchant.getItems().Count == 0)
                {
                    merchant.forceClose();
                }
                else
                {
                    merchant.removeVisitor(this);
                    this.setHiredMerchant(null);
                }
            }
            else
            {
                if (merchant.isOwner(this))
                {
                    merchant.setOpen(true);
                }
                else
                {
                    merchant.removeVisitor(this);
                }
                try
                {
                    merchant.saveItems(false);
                }
                catch (Exception e)
                {
                    Log.Error(e, "Error while saving {name}'s Hired Merchant items.", Name);
                }
            }
        }


    }
}
