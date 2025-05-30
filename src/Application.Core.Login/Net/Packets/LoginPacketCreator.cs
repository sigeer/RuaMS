using Application.Core.Login.Client;
using Application.Core.Login.Models;
using Application.Shared.Net;
using Application.Utility.Configs;
using Application.Utility.Exceptions;
using Application.Utility.Extensions;
using net.opcodes;
using net.packet;
using net.server;
using System.Net;
using tools;

namespace Application.Core.Login.Net.Packets
{
    public static class LoginPacketCreator
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="reason">
        /// The reason logging in failed.
        /// <para>Possible values for <paramref name="reason"/>:</para>
        /// <para>3: ID deleted or blocked</para>
        /// <para>4: Incorrect password</para>
        /// <para>5: Not a registered id</para>
        /// <para>6: System error</para>
        /// <para>7: Already logged in</para>
        /// <para>8: System error</para>
        /// <para>9: System error</para>
        /// <para>10: Cannot process so many connections</para>
        /// <para>11: Only users older than 20 can use this channel</para>
        /// <para>13: Unable to log on as master at this ip</para>
        /// <para>14: Wrong gateway or personal info and weird korean button</para>
        /// <para>15: Processing request with that korean button!</para>
        /// <para>16: Please verify your account through email...</para>
        /// <para>17: Wrong gateway or personal info</para>
        /// <para>21: Please verify your account through email...</para>
        /// <para>23: License agreement</para>
        /// <para>25: Maple Europe notice =[ FUCK YOU NEXON</para>
        /// <para>27: Some weird full client  notice, probably for trial versions</para>
        /// </param>
        /// <returns>The login failed packet.</returns>
        public static Packet GetLoginFailed(int reason)
        {
            OutPacket p = OutPacket.create(SendOpcode.LOGIN_STATUS);
            p.writeByte(reason);
            p.writeByte(0);
            p.writeInt(0);
            return p;
        }

        public static Packet GetPermBan(byte reason)
        {
            OutPacket p = OutPacket.create(SendOpcode.LOGIN_STATUS);
            p.writeByte(2); // Account is banned
            p.writeByte(0);
            p.writeInt(0);
            p.writeByte(reason);
            p.writeLong(PacketCreator.getTime(-1));
            return p;
        }

        public static Packet GetTempBan(long timestampTill, byte reason)
        {
            OutPacket p = OutPacket.create(SendOpcode.LOGIN_STATUS);
            p.writeByte(2);
            p.writeByte(0);
            p.writeInt(0);
            p.writeByte(reason);
            p.writeLong(PacketCreator.getTime(timestampTill)); // Tempban date is handled as a 64-bit long, number of 100NS intervals since 1/1/1601. Lulz.
            return p;
        }
        public static Packet GetAuthSuccess(ILoginClient c)
        {
            if (c.AccountEntity == null)
                throw new BusinessException("未获取到账户信息");

            OutPacket p = OutPacket.create(SendOpcode.LOGIN_STATUS);
            p.writeInt(0);
            p.writeShort(0);
            p.writeInt(c.AccountEntity.Id);
            p.writeByte(c.AccountEntity.Gender);

            bool canFly = Server.getInstance().canFly(c.AccountEntity.Id);
            p.writeBool((YamlConfig.config.server.USE_ENFORCE_ADMIN_ACCOUNT || canFly) && c.AccountEntity.GMLevel > 1);    // thanks Steve(kaito1410) for pointing the GM account bool here
            p.writeByte(((YamlConfig.config.server.USE_ENFORCE_ADMIN_ACCOUNT || canFly) && c.AccountEntity.GMLevel > 1) ? 0x80 : 0);  // Admin Byte. 0x80,0x40,0x20.. Rubbish.
            p.writeByte(0); // Country Code.

            p.writeString(c.AccountEntity.Name);
            p.writeByte(0);

            p.writeByte(0); // IsQuietBan
            p.writeLong(0);//IsQuietBanTimeStamp
            p.writeLong(0); //CreationTimeStamp

            p.writeInt(1); // 1: Remove the "Select the world you want to play in"

            p.writeByte(YamlConfig.config.server.ENABLE_PIN && !c.CanBypassPin() ? 0 : 1); // 0 = Pin-System Enabled, 1 = Disabled
            p.writeByte(YamlConfig.config.server.ENABLE_PIC && !c.CanBypassPin() ? (string.IsNullOrEmpty(c.AccountEntity.Pin) ? 0 : 1) : 2); // 0 = Register PIC, 1 = Ask for PIC, 2 = Disabled

            return p;
        }

        public static Packet WrongPic()
        {
            OutPacket p = OutPacket.create(SendOpcode.CHECK_SPW_RESULT);
            p.writeByte(0);
            return p;
        }

        /// <summary>
        /// Gets a packet telling the client the IP of the channel server.
        /// </summary>
        /// <param name="inetAddr">The InetAddress of the requested channel server.</param>
        /// <param name="port">The port the channel is on.</param>
        /// <param name="characterId">The ID of the client.</param>
        /// <returns>The server IP packet.</returns>
        public static Packet GetServerIP(IPEndPoint iPEndPoint, int characterId)
        {
            OutPacket p = OutPacket.create(SendOpcode.SERVER_IP);
            p.writeShort(0);
            byte[] addr = iPEndPoint.Address.GetAddressBytes();
            p.writeBytes(addr);
            p.writeShort(iPEndPoint.Port);
            p.writeInt(characterId);
            p.writeBytes(new byte[] { 0, 0, 0, 0, 0 });
            return p;
        }

        public static Packet SendRecommended(MasterServer server)
        {
            OutPacket p = OutPacket.create(SendOpcode.RECOMMENDED_WORLD_MESSAGE);
            p.writeByte(1);//size
            p.writeInt(server.Id);
            p.writeString(server.WhyAmIRecommended);
            return p;
        }

        public static Packet showAllCharacterInfo(ILoginClient client, int worldid, List<CharacterViewObject> chars, bool usePic)
        {
            OutPacket p = OutPacket.create(SendOpcode.VIEW_ALL_CHAR);
            p.writeByte(0);
            p.writeByte(worldid);
            p.writeByte(chars.Count);
            foreach (var chr in chars)
            {
                AddCharEntry(p, client, chr, true);
            }
            p.writeByte(usePic ? 1 : 2);
            return p;
        }

        public static Packet GetCharListPacket(ILoginClient c)
        {
            OutPacket p = OutPacket.create(SendOpcode.CHARLIST);
            p.writeByte(0);
            List<CharacterViewObject> chars = c.LoadCharactersView();
            p.writeByte((byte)chars.Count);
            foreach (var chr in chars)
            {
                AddCharEntry(p, c, chr, false);
            }

            p.writeByte(YamlConfig.config.server.ENABLE_PIC && !c.CanBypassPic() ? (string.IsNullOrEmpty(c.AccountEntity?.Pic) ? 0 : 1) : 2);
            p.writeInt(c.AccountEntity!.Characterslots);
            return p;
        }

        private static void AddCharEntry(OutPacket p, ILoginClient playerClient, CharacterViewObject chr, bool viewall)
        {
            addCharStats(p, chr.Character);
            addCharLook(p, chr, false);
            if (!viewall)
            {
                p.writeByte(0);
            }
            if (playerClient.AccountEntity!.GMLevel > 1 || JobFactory.GetById(chr.Character.JobId).IsGmJob())
            {
                // thanks Daddy Egg (Ubaware), resinate for noticing GM jobs crashing on non-GM players account
                p.writeByte(0);
                return;
            }
            p.writeByte(1); // world rank enabled (next 4 ints are not sent if disabled) short??
            p.writeInt(chr.Character.Rank); // world rank
            p.writeInt(chr.Character.RankMove); // move (negative is downwards)
            p.writeInt(chr.Character.JobRank); // job rank
            p.writeInt(chr.Character.JobRankMove); // move (negative is downwards)
        }

        private static void addCharStats(OutPacket p, CharacterModel chr)
        {
            p.writeInt(chr.Id); // character id
            p.writeFixedString(chr.Name);
            p.writeByte(chr.Gender); // gender (0 = male, 1 = female)
            p.writeByte((byte)chr.Skincolor); // skin color
            p.writeInt(chr.Face); // face
            p.writeInt(chr.Hair); // hair

            for (int i = 0; i < 3; i++)
            {
                p.writeLong(0);
            }

            p.writeByte(chr.Level); // level
            p.writeShort(chr.JobId); // job
            p.writeShort(chr.Str); // str
            p.writeShort(chr.Dex); // dex
            p.writeShort(chr.Int); // int
            p.writeShort(chr.Luk); // luk
            p.writeShort(chr.Hp); // hp (?)
            p.writeShort(chr.Maxhp); // maxhp
            p.writeShort(chr.Mp); // mp (?)
            p.writeShort(chr.Maxmp); // maxmp
            p.writeShort(chr.Ap); // remaining ap
            p.writeShort(0); // remaining sp 只是在登录界面预览，应该不会影响游戏内容吧？
            p.writeInt(chr.Exp); // current exp
            p.writeShort(chr.Fame); // fame
            p.writeInt(chr.Gachaexp); //Gacha Exp
            p.writeInt(chr.Map); // current map id
            p.writeByte(chr.Spawnpoint); // spawnpoint
            p.writeInt(0);
        }

        public static void addCharLook(OutPacket p, CharacterViewObject chr, bool mega)
        {
            p.writeByte(chr.Character.Gender);
            p.writeByte((int)chr.Character.Skincolor); // skin color
            p.writeInt(chr.Character.Face); // face
            p.writeBool(!mega);
            p.writeInt(chr.Character.Hair); // hair
            addCharEquips(p, chr);
        }

        private static void addCharEquips(OutPacket p, CharacterViewObject chr)
        {
            Dictionary<short, int> myEquip = new();
            Dictionary<short, int> maskedEquip = new();
            int weaponItemId = 0;
            foreach (var item in chr.InventoryItems.Where(x => x.InventoryType == -1))
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


    }
}
