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


using server.life;
using tools;

namespace client.command.commands.gm3;

public class NpcCommand : Command
{
    public NpcCommand()
    {
        setDescription("Spawn an NPC on your location.");
    }

    public override void execute(IClient c, string[] paramsValue)
    {
        var player = c.OnlinedCharacter;
        if (paramsValue.Length < 1)
        {
            player.yellowMessage("Syntax: !npc <npcid>");
            return;
        }
        var npc = LifeFactory.getNPC(int.Parse(paramsValue[0]));
        if (npc != null)
        {
            npc.setPosition(player.getPosition());
            npc.setCy(player.getPosition().Y);
            npc.setRx0(player.getPosition().X + 50);
            npc.setRx1(player.getPosition().X - 50);
            npc.setFh(player.getMap().getFootholds().findBelow(c.OnlinedCharacter.getPosition()).getId());
            player.getMap().addMapObject(npc);
            player.getMap().broadcastMessage(PacketCreator.spawnNPC(npc));
        }
    }
}
