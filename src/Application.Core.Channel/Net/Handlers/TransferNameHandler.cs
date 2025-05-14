/*
    This file is part of the HeavenMS MapleStory NewServer
    Copyleft (L) 2016 - 2019 RonanLana

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



using Application.EF;
using Application.Utility.Configs;
using Microsoft.Extensions.Logging;
using net.packet;
using tools;

namespace Application.Core.Channel.Net.Handlers;

/**
 * @author Ronan
 * @author Ubaware
 */
public class TransferNameHandler : ChannelHandlerBase
{
    readonly ILogger<TransferNameHandler> _logger;

    public TransferNameHandler(ILogger<TransferNameHandler> logger)
    {
        _logger = logger;
    }

    public override void HandlePacket(InPacket p, IChannelClient c)
    {
        p.readInt(); //cid
        int birthday = p.readInt();
        if (!c.CheckBirthday(birthday))
        {
            c.sendPacket(PacketCreator.showCashShopMessage(0xC4));
            c.sendPacket(PacketCreator.enableActions());
            return;
        }
        if (!YamlConfig.config.server.ALLOW_CASHSHOP_NAME_CHANGE)
        {
            c.sendPacket(PacketCreator.sendNameTransferRules(4));
            return;
        }
        var chr = c.OnlinedCharacter;
        if (chr.getLevel() < 10)
        {
            c.sendPacket(PacketCreator.sendNameTransferRules(4));
            return;
        }
        else if (c.AccountEntity?.Tempban != null && c.AccountEntity?.Tempban.Value.AddDays(30) < DateTimeOffset.Now)
        {
            c.sendPacket(PacketCreator.sendNameTransferRules(2));
            return;
        }
        //sql queries
        try
        { //double check, just in case
            using var dbContext = new DBContext();
            var dataList = dbContext.Namechanges.Where(x => x.Characterid == chr.getId()).Select(x => new { x.CompletionTime }).ToList();
            foreach (var rs in dataList)
            {
                if (rs.CompletionTime == null)
                { //has pending name request
                    c.sendPacket(PacketCreator.sendNameTransferRules(1));
                    return;
                }
                else if (rs.CompletionTime.Value.AddMilliseconds(YamlConfig.config.server.NAME_CHANGE_COOLDOWN) > DateTimeOffset.Now)
                {
                    c.sendPacket(PacketCreator.sendNameTransferRules(3));
                    return;
                }
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e.ToString());
            return;
        }
        c.sendPacket(PacketCreator.sendNameTransferRules(0));
    }
}
