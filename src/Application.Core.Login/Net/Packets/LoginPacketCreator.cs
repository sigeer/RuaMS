using Application.Core.Login.Client;
using Application.Core.Login.Models;
using Application.Utility;
using Application.Utility.Configs;
using Application.Utility.Exceptions;
using Application.Utility.Extensions;
using System.Net;

namespace Application.Core.Login.Net.Packets
{
    public static class LoginPacketCreator
    {
        public static Packet AddNewCharEntry(NewCharacterPreview model)
        {
            OutPacket p = OutPacket.create(SendOpcode.ADD_NEW_CHAR_ENTRY);
            p.writeByte(0);
            model.Encode(p, model.Account, false, false);
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
            p.writeBool((YamlConfig.config.server.USE_ENFORCE_ADMIN_ACCOUNT || canFly) && c.AccountEntity.IsGmAccount());    // thanks Steve(kaito1410) for pointing the GM account bool here
            p.writeByte(((YamlConfig.config.server.USE_ENFORCE_ADMIN_ACCOUNT || canFly) && c.AccountEntity.IsGmAccount()) ? 0x80 : 0);  // Admin Byte. 0x80,0x40,0x20.. Rubbish.
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



        public static Packet SendRecommended(MasterServer server)
        {
            OutPacket p = OutPacket.create(SendOpcode.RECOMMENDED_WORLD_MESSAGE);
            p.writeByte(1);//size
            p.writeInt(server.Id);
            p.writeString(server.WhyAmIRecommended);
            return p;
        }

        public static Packet showAllCharacter(int totalWorlds, int totalChrs)
        {
            OutPacket p = OutPacket.create(SendOpcode.VIEW_ALL_CHAR);
            p.writeByte(totalChrs > 0 ? 1 : 5); // 2: already connected to server, 3 : unk error (view-all-characters), 5 : cannot find any
            p.writeInt(totalWorlds);
            p.writeInt(totalChrs);
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
                chr.Encode(p, client.AccountEntity!, true, true);
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
                chr.Encode(p, c.AccountEntity!, false, true);
            }

            p.writeByte(YamlConfig.config.server.ENABLE_PIC && !c.CanBypassPic() ? (string.IsNullOrEmpty(c.AccountEntity?.Pic) ? 0 : 1) : 2);
            p.writeInt(c.AccountEntity!.Characterslots);
            return p;
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
            p.writeShort(100); // event xp
            p.writeShort(100); // drop rate
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

        public static Packet charNameResponse(string charname, bool nameUsed)
        {
            OutPacket p = OutPacket.create(SendOpcode.CHAR_NAME_RESPONSE);
            p.writeString(charname);
            p.writeByte(nameUsed ? 1 : 0);
            return p;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cid"></param>
        /// <param name="state">
        /// <para>0x00 = success</para>
        /// <para>0x06 = Trouble logging into the game?</para>
        /// <para>0x09 = Unknown error</para>
        /// <para>0x0A = Could not be processed due to too many connection requests to the server.</para>
        /// <para>0x12 = invalid bday</para>
        /// <para>0x14 = incorrect pic</para>
        /// <para>0x16 = Cannot delete a guild master.</para>
        /// <para>0x18 = Cannot delete a character with a pending wedding.</para>
        /// <para>0x1A = Cannot delete a character with a pending world transfer.</para>
        /// <para>0x1D = Cannot delete a character that has a family.</para>
        /// </param>
        /// <returns></returns>
        public static Packet deleteCharResponse(int cid, int state)
        {
            OutPacket p = OutPacket.create(SendOpcode.DELETE_CHAR_RESPONSE);
            p.writeInt(cid);
            p.writeByte(state);
            return p;
        }

        /// <summary>
        /// Gets the response to a relog request.
        /// </summary>
        /// <returns></returns>
        public static Packet getRelogResponse()
        {
            OutPacket p = OutPacket.create(SendOpcode.RELOG_RESPONSE);
            p.writeByte(1);//1 O.O Must be more types ):
            return p;
        }

    }
}
