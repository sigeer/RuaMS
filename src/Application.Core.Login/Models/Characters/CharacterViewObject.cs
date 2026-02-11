using Application.Utility.Extensions;

namespace Application.Core.Login.Models
{
    public class CharacterViewObject
    {
        public CharacterViewObject(CharacterModel character, ItemModel[] items)
        {
            Character = character;
            InventoryItems = items;
        }

        public CharacterModel Character { get; set; }
        public ItemModel[] InventoryItems { get; set; }

        /// <summary>
        /// AvatarLook::Decode
        /// </summary>
        /// <param name="p"></param>
        /// <param name="mega">不明</param>
        public virtual void EncodeAvatarLook(OutPacket p)
        {
            p.writeByte(Character.Gender);
            p.writeByte((int)Character.Skincolor); // skin color
            p.writeInt(Character.Face); // face
            p.writeBool(true);
            p.writeInt(Character.Hair); // hair

            Dictionary<short, int> myEquip = new();
            Dictionary<short, int> maskedEquip = new();
            int weaponItemId = 0;
            foreach (var item in InventoryItems.Where(x => x.Type == 1 && x.InventoryType == -1))
            {
                short pos = (short)(item.Position * -1);
                if (pos < 100 && !myEquip.ContainsKey(pos))
                {
                    myEquip.AddOrUpdate(pos, item.Itemid);
                }
                else if (pos > 100 && pos != 111)
                {
                    // don't ask. o.o
                    pos -= 100;
                    if (myEquip.TryGetValue(pos, out var d))
                    {
                        maskedEquip.AddOrUpdate(pos, d);
                    }
                    myEquip.AddOrUpdate(pos, item.Itemid);
                }
                else if (myEquip.ContainsKey(pos))
                {
                    maskedEquip.AddOrUpdate(pos, item.Itemid);
                }

                if (item.Position == -111)
                    weaponItemId = item.Itemid;
            }
            foreach (var entry in myEquip)
            {
                p.writeByte(entry.Key);
                p.writeInt(entry.Value);
            }

            p.writeByte(0xFF);
            foreach (var entry in maskedEquip)
            {
                p.writeByte(entry.Key);
                p.writeInt(entry.Value);
            }

            p.writeByte(0xFF);
            p.writeInt(weaponItemId);
            for (int i = 0; i < 3; i++)
            {
                p.writeInt(0);
            }
        }

        /// <summary>
        /// GW_CharacterStat::Decode
        /// </summary>
        /// <param name="p"></param>
        public virtual void EncodeStats(OutPacket p)
        {
            p.writeInt(Character.Id); // character id
            p.writeFixedString(Character.Name);
            p.writeByte(Character.Gender); // gender (0 = male, 1 = female)
            p.writeByte((byte)Character.Skincolor); // skin color
            p.writeInt(Character.Face); // face
            p.writeInt(Character.Hair); // hair

            for (int i = 0; i < 3; i++)
            {
                p.writeLong(0);
            }

            p.writeByte(Character.Level); // level
            p.writeShort(Character.JobId); // job
            p.writeShort(Character.Str); // str
            p.writeShort(Character.Dex); // dex
            p.writeShort(Character.Int); // int
            p.writeShort(Character.Luk); // luk
            p.writeShort(Character.Hp); // hp (?)
            p.writeShort(Character.Maxhp); // maxhp
            p.writeShort(Character.Mp); // mp (?)
            p.writeShort(Character.Maxmp); // maxmp
            p.writeShort(Character.Ap); // remaining ap
            p.writeShort(0); // remaining sp 只是在登录界面预览，应该不会影响游戏内容吧？
            p.writeInt(Character.Exp); // current exp
            p.writeShort(Character.Fame); // fame
            p.writeInt(Character.Gachaexp); //Gacha Exp
            p.writeInt(Character.Map); // current map id
            p.writeByte(Character.Spawnpoint); // spawnpoint
            p.writeInt(0);
        }

        public virtual OutPacket Encode(OutPacket p, AccountCtrl accountInfo, bool isViewAll, bool rankEnable)
        {
            EncodeStats(p);

            EncodeAvatarLook(p);

            if (!isViewAll)
                p.writeByte(0);

            EncodeRankInfo(p, !accountInfo.IsGmAccount() && !JobFactory.GetById(Character.JobId).IsGmJob() && rankEnable);
            return p;
        }

        protected void EncodeRankInfo(OutPacket p, bool enable)
        {
            p.writeBool(enable); // world rank enabled (next 4 ints are not sent if disabled) short??
            if (enable)
            {
                p.writeInt(Character.Rank); // world rank
                p.writeInt(Character.RankMove); // move (negative is downwards)
                p.writeInt(Character.JobRank); // job rank
                p.writeInt(Character.JobRankMove); // move (negative is downwards)
            }
        }
    }
}
