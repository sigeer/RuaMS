using Application.Core.Channel;
using Application.Core.Game.Players;
using Application.Core.Login.Client;
using Application.Core.Login.Models;
using Application.Utility;
using Application.Utility.Configs;
using Application.Utility.Exceptions;
using Application.Utility.Extensions;
using net.server;
using System.Net;

namespace Application.Core.Login.Net.Packets
{
    public static class LoginPacketCreator
    {
        public static Packet addNewCharEntry(ILoginClient client, CharacterViewObject chr)
        {
            OutPacket p = OutPacket.create(SendOpcode.ADD_NEW_CHAR_ENTRY);
            p.writeByte(0);
            AddCharEntry(p, client, chr, false);
            return p;
        }
        /// <summary>
        /// Gets a packet detailing a server status message.
        /// </summary>
        /// <param name="status">The server status.
        /// <para>Possible values for <paramref name="status"/>:</para>
        /// <para>0 - Normal</para>
        /// <para>1 - Highly populated</para>
        /// <para>2 - Full</para>
        /// </param>
        /// <returns>The server status packet.</returns>
        public static Packet getServerStatus(int status)
        {
            OutPacket p = OutPacket.create(SendOpcode.SERVERSTATUS);
            p.writeShort(status);
            return p;
        }
        public static Packet sendGuestTOS()
        {
            OutPacket p = OutPacket.create(SendOpcode.GUEST_ID_LOGIN);
            p.writeShort(0x100);
            p.writeInt(Randomizer.nextInt(999999));
            p.writeLong(0);
            p.writeLong(PacketCommon.getTime(-2));
            p.writeLong(PacketCommon.getTime(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()));
            p.writeInt(0);
            p.writeString("http://maplesolaxia.com");
            return p;
        }

        /**
         * Gets a packet detailing a PIN operation.
         * <p>
         * Possible values for <code>mode</code>:<br> 0 - PIN was accepted<br> 1 -
         * Register a new PIN<br> 2 - Invalid pin / Reenter<br> 3 - Connection
         * failed due to system error<br> 4 - Enter the pin
         *
         * @param mode The mode.
         * @return
         */
        private static Packet pinOperation(byte mode)
        {
            OutPacket p = OutPacket.create(SendOpcode.CHECK_PINCODE);
            p.writeByte(mode);
            return p;
        }
        public static Packet pinRegistered()
        {
            OutPacket p = OutPacket.create(SendOpcode.UPDATE_PINCODE);
            p.writeByte(0);
            return p;
        }

        public static Packet requestPin()
        {
            return pinOperation(4);
        }

        public static Packet requestPinAfterFailure()
        {
            return pinOperation(2);
        }

        public static Packet registerPin()
        {
            return pinOperation(1);
        }

        public static Packet pinAccepted()
        {
            return pinOperation(0);
        }

        public static Packet wrongPic()
        {
            OutPacket p = OutPacket.create(SendOpcode.CHECK_SPW_RESULT);
            p.writeByte(0);
            return p;
        }

        /**
 * Gets a server message packet.
 * <p>
 * Possible values for <code>type</code>:<br> 0: [Notice]<br> 1: Popup<br>
 * 2: Megaphone<br> 3: Super Megaphone<br> 4: Scrolling message at top<br>
 * 5: Pink Text<br> 6: Lightblue Text<br> 7: BroadCasting NPC
 *
 * @param type          The type of the notice.
 * @param channel       The channel this notice was sent on.
 * @param message       The message to convey.
 * @param servermessage Is this a scrolling ticker?
 * @return The server notice packet.
 */
        private static Packet serverMessage(int type, int channel, string message, bool servermessage, bool megaEar, int npc)
        {
            OutPacket p = OutPacket.create(SendOpcode.SERVERMESSAGE);
            p.writeByte(type);
            if (servermessage)
            {
                p.writeByte(1);
            }
            p.writeString(message);
            if (type == 3)
            {
                p.writeByte(channel - 1); // channel
                p.writeBool(megaEar);
            }
            else if (type == 6)
            {
                p.writeInt(0);
            }
            else if (type == 7)
            { // npc
                p.writeInt(npc);
            }
            return p;
        }

        /**
 * Gets a server message packet.
 *
 * @param message The message to convey.
 * @return The server message packet.
 */
        public static Packet serverMessage(string message)
        {
            return serverMessage(4, 0, message, true, false, 0);
        }
        /// <summary>
        /// Gets a packet telling the client the IP of the channel server.
        /// </summary>
        /// <param name="inetAddr">The InetAddress of the requested channel server.</param>
        /// <param name="port">The port the channel is on.</param>
        /// <param name="clientId">The ID of the client.</param>
        /// <returns>The server IP packet.</returns>
        public static Packet getServerIP(IPEndPoint iPEndPoint, int clientId)
        {
            OutPacket p = OutPacket.create(SendOpcode.SERVER_IP);
            p.writeShort(0);
            byte[] addr = iPEndPoint.Address.GetAddressBytes();
            p.writeBytes(addr);
            p.writeShort(iPEndPoint.Port);
            p.writeInt(clientId);
            p.writeBytes(new byte[] { 0, 0, 0, 0, 0 });
            return p;
        }

        /// <summary>
        /// Gets a login failed packet.
        /// </summary>
        /// <param name="reason">
        /// The reason logging in failed.
        /// <para>Possible values for <paramref name="reason"/>:</para>
        /// <para>3: ID deleted or blocked</para>
        /// <para>4: Incorrect password</para>
        /// <para>5: Not a registered id</para>
        /// <para>6: Trouble logging into the game?</para>
        /// <para>7: Already logged in</para>
        /// <para>8: Trouble logging into the game?</para>
        /// <para>9: Trouble logging into the game?</para>
        /// <para>10: Cannot process so many connections</para>
        /// <para>11: Only users older than 20 can use this channel</para>
        /// <para>12: Trouble logging into the game?</para>
        /// <para>13: Unable to log on as master at this ip</para>
        /// <para>14: Wrong gateway or personal info and weird korean button</para>
        /// <para>15: Processing request with that korean button!</para>
        /// <para>16: Please verify your account through email...</para>
        /// <para>17: Wrong gateway or personal info</para>
        /// <para>21: Please verify your account through email...</para>
        /// <para>23: Crashes</para>
        /// <para>25: Maple Europe notice =[ FUCK YOU NEXON</para>
        /// <para>27: Some weird full client  notice, probably for trial versions</para>
        /// </param>
        /// <returns>The login failed packet.</returns>
        public static Packet getAfterLoginError(int reason)
        {//same as above o.o
            OutPacket p = OutPacket.create(SendOpcode.SELECT_CHARACTER_BY_VAC);
            p.writeShort(reason);//using other types than stated above = CRASH
            return p;
        }

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

        public static Packet GetTempBan(long timestampTill, byte reason)
        {
            OutPacket p = OutPacket.create(SendOpcode.LOGIN_STATUS);
            p.writeByte(2);
            p.writeByte(0);
            p.writeInt(0);
            p.writeByte(reason);
            p.writeLong(PacketCommon.getTime(timestampTill)); // Tempban date is handled as a 64-bit long, number of 100NS intervals since 1/1/1601. Lulz.
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

            bool canFly = c.AccountEntity.CanFly;
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

        /// <summary>
        /// Gets a packet detailing a server and its channels.
        /// </summary>
        /// <param name="serverId"></param>
        /// <param name="serverName">The name of the server.</param>
        /// <param name="flag"></param>
        /// <param name="eventmsg"></param>
        /// <param name="channelLoad">Load of the channel - 1200 seems to be max.</param>
        /// <returns>The server info packet.</returns>
        public static Packet GetServerList(MasterServer server)
        {
            OutPacket p = OutPacket.create(SendOpcode.SERVERLIST);
            p.writeByte(server.Id);
            p.writeString(server.Name);
            p.writeByte(server.Flag);
            p.writeString(server.EventMessage);
            p.writeByte(100); // rate modifier, don't ask O.O!
            p.writeByte(0); // event xp * 2.6 O.O!
            p.writeByte(100); // rate modifier, don't ask O.O!
            p.writeByte(0); // drop rate * 2.6
            p.writeByte(0);
            p.writeByte(server.Channels.Count);
            for (int i = 0; i < server.Channels.Count; i++)
            {
                var chId = (i + 1);
                var ch = server.Channels[i];
                p.writeString(server.Name + "-" + chId);
                p.writeInt(server.GetChannelCapacity(chId));

                // thanks GabrielSin for this channel packet structure part
                p.writeByte(1);// nWorldID
                p.writeByte(i);// nChannelID
                p.writeBool(false);// bAdultChannel
            }
            p.writeShort(0);
            return p;
        }

        /// <summary>
        /// Gets a packet saying that the server list is over.
        /// </summary>
        /// <returns>The end of server list packet.</returns>
        public static Packet GetEndOfServerList()
        {
            OutPacket p = OutPacket.create(SendOpcode.SERVERLIST);
            p.writeByte(0xFF);
            return p;
        }

        public static Packet SelectWorld(int world)
        {
            OutPacket p = OutPacket.create(SendOpcode.LAST_CONNECTED_WORLD);
            p.writeInt(world);//According to GMS, it should be the world that contains the most characters (most active)
            return p;
        }
    }
}
