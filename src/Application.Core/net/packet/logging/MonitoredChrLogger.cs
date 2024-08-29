/*
 This file is part of the OdinMS Maple Story Server
 Copyright (C) 2008 ~ 2010 Patrick Huy <patrick.huy@frz.cc>
 Matthias Butz <matze@odinms.de>
 Jan Christian Meyer <vimes@odinms.de>

 This program is free software: you can redistribute it and/or modify
 it under the terms of the GNU Affero General Public License version 3
 as published by the Free Software Foundation. You may not use, modify
 or distribute this program under any other version of the
 GNU Affero General Public License.

 This program is distributed in the hope that it will be useful,
 but WITHOUT ANY WARRANTY; without even the implied warranty of
 MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 GNU Affero General Public License for more details.

 You should have received a copy of the GNU Affero General Public License
 along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */


using net.opcodes;
using tools;

namespace net.packet.logging;






/**
 * Logs packets from monitored characters to a file.
 *
 * @author Alan (SharpAceX)
 */

public class MonitoredChrLogger
{
    private static ILogger log = LogFactory.GetLogger("MonitoredChrLogger");
    private static HashSet<int> monitoredChrIds = new();

    /**
     * Toggle monitored status for a character id
     *
     * @return new status. true if the chrId is now monitored, otherwise false.
     */
    public static bool toggleMonitored(int chrId)
    {
        if (monitoredChrIds.Contains(chrId))
        {
            monitoredChrIds.Remove(chrId);
            return false;
        }
        else
        {
            monitoredChrIds.Add(chrId);
            return true;
        }
    }

    public static ICollection<int> getMonitoredChrIds()
    {
        return monitoredChrIds;
    }

    public static void logPacketIfMonitored(IClient c, short packetId, byte[] packetContent)
    {
        var chr = c.OnlinedCharacter;
        if (chr == null)
        {
            return;
        }
        if (!monitoredChrIds.Contains(chr.getId()))
        {
            return;
        }
        RecvOpcode op = getOpcodeFromValue(packetId);
        if (isRecvBlocked(op))
        {
            return;
        }

        string packet = packetContent.Length > 0 ? HexTool.toHexString(packetContent) : "<empty>";
        log.Information("{AccountName}-{CharacterName} {PacketId}-{Packet}", c.getAccountName(), chr.getName(), packetId, packet);
    }

    private static bool isRecvBlocked(RecvOpcode op)
    {
        return (op) switch
        {
            RecvOpcode.MOVE_PLAYER or RecvOpcode.GENERAL_CHAT or RecvOpcode.TAKE_DAMAGE or RecvOpcode.MOVE_PET or RecvOpcode.MOVE_LIFE or RecvOpcode.NPC_ACTION or RecvOpcode.FACE_EXPRESSION => true,
            _ => false
        };
    }

    private static RecvOpcode getOpcodeFromValue(int value)
    {
        return (RecvOpcode)value;
    }
}
