/*
 This file is part of the OdinMS Maple Story NewServer
 Copyright (C) 2008 Patrick Huy <patrick.huy@frz.cc>
 Matthias Butz <matze@odinms.de>
 Jan Christian Meyer <vimes@odinms.de>

 This program is free software: you can redistribute it and/or modify
 it under the terms of the GNU Affero General Public License as
 published by the Free Software Foundation version 3 as published by
 the Free Software Foundation. You may not use, modify or distribute
 this program under any other version of the GNU Affero General Public
 License.

 This program is distributed in the hope that it will be useful,
 but WITHOUT ANY WARRANTY; without even the implied warranty of
 MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 GNU Affero General Public License for more details.

 You should have received a copy of the GNU Affero General Public License
 along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */


using Application.Core.Managers;
using net.packet;
using net.server.coordinator.session;
using tools;

namespace net.server.handlers.login;

public class LoginPasswordHandler : AbstractPacketHandler
{

    public override bool ValidateState(IClient c)
    {
        return !c.isLoggedIn();
    }

    public override void HandlePacket(InPacket p, IClient c)
    {
        string remoteHost = c.getRemoteAddress();
        if (string.IsNullOrEmpty(remoteHost) || remoteHost == "null")
        {
            c.sendPacket(PacketCreator.getLoginFailed(14));          // thanks Alchemist for noting remoteHost could be null
            return;
        }

        string login = p.readString();
        string pwd = p.readString();
        c.setAccountName(login);

        p.skip(6);   // localhost masked the initial part with zeroes...
        byte[] hwidNibbles = p.readBytes(4);
        Hwid hwid = new Hwid(HexTool.toCompactHexString(hwidNibbles));
        int loginok = c.login(login, pwd, hwid);


        if (YamlConfig.config.server.AUTOMATIC_REGISTER && loginok == 5)
        {
            try
            { //Jayd: Added birthday, tempban
                var newAccountId = AccountManager.CreateAccount(login, pwd);
                c.setAccID(newAccountId);
            }
            catch (Exception e)
            {
                c.setAccID(-1);
                log.Error(e.ToString());
            }
            finally
            {
                loginok = c.login(login, pwd, hwid);
            }
        }

        if (YamlConfig.config.server.BCRYPT_MIGRATION && (loginok <= -10))
        { // -10 means migration to bcrypt, -23 means TOS wasn't accepted
            try
            {
                AccountManager.UpdatePasswordToBCrypt(login, pwd);
            }
            catch (Exception e)
            {
                log.Error(e.ToString());
            }
            finally
            {
                loginok = (loginok == -10) ? 0 : 23;
            }
        }

        if (c.hasBannedIP() || c.hasBannedMac())
        {
            c.sendPacket(PacketCreator.getLoginFailed(3));
            return;
        }
        var tempban = c.getTempBanCalendarFromDB();
        if (tempban != null)
        {
            if (tempban > DateTimeOffset.Now)
            {
                c.sendPacket(PacketCreator.getTempBan(tempban.Value.ToUnixTimeMilliseconds(), (byte)c.getGReason()));
                return;
            }
        }
        if (loginok == 3)
        {
            c.sendPacket(PacketCreator.getPermBan((byte)c.getGReason()));//crashes but idc :D
            return;
        }
        else if (loginok != 0)
        {
            c.sendPacket(PacketCreator.getLoginFailed(loginok));
            return;
        }
        if (c.finishLogin() == 0)
        {
            c.checkChar(c.getAccID());
            CompleteLogin(c);
        }
        else
        {
            c.sendPacket(PacketCreator.getLoginFailed(7));
        }
    }

    private static void CompleteLogin(IClient c)
    {
        c.sendPacket(PacketCreator.getAuthSuccess(c));//why the fk did I do c.getAccountName()?
        Server.getInstance().registerLoginState(c);
    }
}
