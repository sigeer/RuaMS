/*
    This file is part of the HeavenMS MapleStory Server, commands OdinMS-based
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
   @Author: Ronan
*/


using net.server.channel;
using server.life;
using server.maps;
using tools;

namespace client.command.commands.gm4;






public class PnpcCommand : Command
{
    public PnpcCommand()
    {
        setDescription("Spawn a permanent NPC on your location.");
    }

    public override void execute(Client c, string[] paramsValue)
    {
        Character player = c.getPlayer();
        if (paramsValue.Length < 1)
        {
            player.yellowMessage("Syntax: !pnpc <npcid>");
            return;
        }

        // command suggestion thanks to HighKey21, none, bibiko94 (TAYAMO), asafgb
        int mapId = player.getMapId();
        int npcId = int.Parse(paramsValue[0]);
        if (player.getMap().containsNPC(npcId))
        {
            player.dropMessage(5, "This map already contains the specified NPC.");
            return;
        }

        NPC npc = LifeFactory.getNPC(npcId);

        Point checkpos = player.getMap().getGroundBelow(player.getPosition());
        int xpos = checkpos.X;
        int ypos = checkpos.Y;
        int fh = player.getMap().getFootholds().findBelow(checkpos).getId();

        if (npc != null && !npc.getName().Equals("MISSINGNO"))
        {
            try
            {
                using var dbContext = new DBContext();
                var model = new Plife(player.getWorld(), mapId, npcId, -1, xpos, ypos, fh, "n");
                dbContext.Plives.Add(model);
                dbContext.SaveChanges();

                foreach (Channel ch in player.getWorldServer().getChannels())
                {
                    npc = LifeFactory.getNPC(npcId);
                    npc.setPosition(checkpos);
                    npc.setCy(ypos);
                    npc.setRx0(xpos + 50);
                    npc.setRx1(xpos - 50);
                    npc.setFh(fh);

                    MapleMap map = ch.getMapFactory().getMap(mapId);
                    map.addMapObject(npc);
                    map.broadcastMessage(PacketCreator.spawnNPC(npc));
                }

                player.yellowMessage("Pnpc created.");
            }
            catch (Exception e)
            {
                log.Error(e.ToString());
                player.dropMessage(5, "Failed to store pNPC in the database.");
            }
        }
        else
        {
            player.dropMessage(5, "You have entered an invalid NPC id.");
        }
    }
}