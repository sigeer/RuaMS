/*
    This file is part of the HeavenMS MapleStory NewServer, commands OdinMS-based
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

/*
   @Author: Arthur L - Refactored command content into modules
*/


using DotNetty.Buffers;
using net;
using net.packet;
using System.Text.Json;
using tools;

namespace client.command.commands.gm3;



public class PeCommand : Command
{
    public PeCommand()
    {
        setDescription("Handle synthesized packets from file, and handle them as if sent from a client");
    }


    public override void execute(IClient c, string[] paramsValue)
    {
        var player = c.OnlinedCharacter;
        string packet = "";
        try
        {
            Dictionary<string, string>? packetProps = JsonSerializer.Deserialize<Dictionary<string, string>>(File.ReadAllText("pe.text"));
            packet = packetProps?.GetValueOrDefault("pe") ?? "";
        }
        catch (IOException ex)
        {
            log.Error(ex.ToString());
            player.yellowMessage("Failed to load pe.txt");
            return;

        }
        byte[] packetContent = HexTool.toBytes(packet);
        InPacket inPacket = new ByteBufInPacket(Unpooled.WrappedBuffer(packetContent));
        short packetId = inPacket.readShort();
        var packetHandler = PacketProcessor.getProcessor(0, c.getChannel()).getHandler(packetId);
        if (packetHandler != null && packetHandler.ValidateState(c))
        {
            try
            {
                player.yellowMessage("Receiving: " + packet);
                packetHandler.HandlePacket(inPacket, c);
            }
            catch (Exception t)
            {
                string chrInfo = player != null ? player.getName() + " on map " + player.getMapId() : "?";
                log.Warning(t, "Error in packet handler {HandlerName}. Chr {CharacterName}, account {AccountName}. Packet: {Packet}", packetHandler.GetType().Name,
                        chrInfo, c.getAccountName(), packet);
            }
        }
    }
}
