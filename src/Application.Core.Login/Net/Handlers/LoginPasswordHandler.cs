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


using Application.Core.Login.Client;
using Application.Core.Login.Net.Packets;
using Application.Core.Login.ServerData;
using Application.Shared.Login;
using Application.Utility;
using Application.Utility.Configs;
using Microsoft.Extensions.Logging;

namespace Application.Core.Login.Net.Handlers;

public class LoginPasswordHandler : LoginHandlerBase
{
    public LoginPasswordHandler(MasterServer server, ILogger<LoginHandlerBase> logger)
        : base(server, logger)
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

        var unknown = p.readBytes(6);
        byte[] hwidNibbles = p.readBytes(4); // 这里获得的hwid，和其他handler里的hostString一样吗？
        Hwid hwid = new Hwid(hwidNibbles.ToHexString());


        var loginok = c.Login(login, pwd, hwid);


        if (YamlConfig.config.server.AUTOMATIC_REGISTER && loginok == LoginResultCode.Fail_AccountNotExsited)
        {
            try
            {
                //Jayd: Added birthday, tempban
                _server.AccountManager.CreateAccount(login, pwd);
                loginok = c.Login(login, pwd, hwid);
            }
            catch (Exception e)
            {
                _logger.LogError(e.ToString());
            }
        }

        var banInfo = _server.AccountBanManager.GetAccountBanInfo(c.AccountId);
        if (banInfo != null)
        {
            c.sendPacket(LoginPacketCreator.GetTempBan(banInfo.EndTime.ToUnixTimeMilliseconds(), (byte)banInfo.Reason));
            return;
        }

        if (_server.AccountBanManager.IsIPBlocked(remoteHost) || _server.AccountBanManager.IsHWIDBlocked(hwid.hwid))
        {
            c.sendPacket(LoginPacketCreator.GetLoginFailed(3));
            return;
        }

        if (loginok != 0)
        {
            c.sendPacket(LoginPacketCreator.GetLoginFailed((int)loginok));
            return;
        }
        if (c.FinishLogin() == LoginResultCode.Success && c.CheckChar(c.AccountEntity!.Id))
        {
            c.sendPacket(LoginPacketCreator.GetAuthSuccess(c));//why the fk did I do c.getAccountName()?
            _server.RegisterLoginState(c);
            c.CurrentHistoryId = _server.AccountHistoryManager.InsertAccountLoginHistory(c.AccountId, c.RemoteAddress, hwid.hwid).Id;

        }
        else
        {
            c.sendPacket(LoginPacketCreator.GetLoginFailed(7));
        }
    }
}
