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


using Application.Core.Client;
using Application.Core.Login.Datas;
using Application.Core.Login.Net.Packets;
using Application.Core.Servers;
using Application.Shared.Login;
using Application.Utility.Configs;
using Microsoft.Extensions.Logging;
using net.packet;
using net.server;
using net.server.coordinator.session;
using tools;

namespace Application.Core.Login.Net.Handlers;

public class LoginPasswordHandler : LoginHandlerBase
{
    public LoginPasswordHandler(IMasterServer server, AccountManager accountManager, ILogger<LoginHandlerBase> logger) 
        : base(server, accountManager, logger)
    {
    }

    public override bool ValidateState(ILoginClient c)
    {
        return !c.IsOnlined;
    }

    public override void HandlePacket(InPacket p, ILoginClient c)
    {
        string remoteHost = c.RemoteAddress;
        if (string.IsNullOrEmpty(remoteHost) || remoteHost == "null")
        {
            c.sendPacket(LoginPacketCreator.GetLoginFailed(14));          // thanks Alchemist for noting remoteHost could be null
            return;
        }

        string login = p.readString();
        string pwd = p.readString();

        p.skip(6);   // localhost masked the initial part with zeroes...
        byte[] hwidNibbles = p.readBytes(4);
        Hwid hwid = new Hwid(hwidNibbles.ToHexString());
        var loginok = c.Login(login, pwd, hwid);


        if (YamlConfig.config.server.AUTOMATIC_REGISTER && loginok == LoginResultCode.Fail_AccountNotExsited)
        {
            try
            {
                //Jayd: Added birthday, tempban
                _accountManager.CreateAccount(login, pwd);
                loginok = c.Login(login, pwd, hwid);
            }
            catch (Exception e)
            {
                _logger.LogError(e.ToString());
            }
        }

        if (c.HasBannedIP() || c.HasBannedMac())
        {
            c.sendPacket(LoginPacketCreator.GetLoginFailed(3));
            return;
        }
        if (c.AccountEntity!.Tempban != null && c.AccountEntity.Tempban > DateTimeOffset.UtcNow)
        {
            c.sendPacket(LoginPacketCreator.GetTempBan(c.AccountEntity!.Tempban.Value.ToUnixTimeMilliseconds(), (byte)c.AccountEntity!.Greason));
            return;
        }

        if (loginok == LoginResultCode.Fail_Banned)
        {
            c.sendPacket(LoginPacketCreator.GetPermBan((byte)c.AccountEntity!.Greason));//crashes but idc :D
            return;
        }
        else if (loginok != 0)
        {
            c.sendPacket(LoginPacketCreator.GetLoginFailed((int)loginok));
            return;
        }
        if (c.FinishLogin() == LoginResultCode.Success && c.CheckChar(c.AccountEntity!.Id))
        {
            c.sendPacket(LoginPacketCreator.GetAuthSuccess(c));//why the fk did I do c.getAccountName()?
            _server.RegisterLoginState(c);
        }
        else
        {
            c.sendPacket(LoginPacketCreator.GetLoginFailed(7));
        }
    }
}
