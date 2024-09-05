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


using Application.Core.Game.Life;

namespace client.command.commands.gm2;



public class WhereaMiCommand : Command
{
    public WhereaMiCommand()
    {
        setDescription("Show info about objects on current map.");
    }

    public override void execute(IClient c, string[] paramsValue)
    {
        var player = c.OnlinedCharacter;

        HashSet<IPlayer> chars = new();
        HashSet<NPC> npcs = new();
        HashSet<PlayerNPC> playernpcs = new();
        HashSet<Monster> mobs = new();

        foreach (var mmo in player.getMap().getMapObjects())
        {
            if (mmo is NPC npc)
            {
                npcs.Add(npc);
            }
            else if (mmo is IPlayer mc)
            {
                chars.Add(mc);
            }
            else if (mmo is Monster mob)
            {
                if (mob.isAlive())
                {
                    mobs.Add(mob);
                }
            }
            else if (mmo is PlayerNPC pnpc)
            {
                playernpcs.Add(pnpc);
            }
        }

        player.yellowMessage("Map ID: " + player.getMap().getId());

        player.yellowMessage("Players on this map:");
        foreach (var chr in chars)
        {
            player.dropMessage(5, ">> " + chr.getName() + " - " + chr.getId() + " - Oid: " + chr.getObjectId());
        }

        if (playernpcs.Count > 0)
        {
            player.yellowMessage("PlayerNPCs on this map:");
            foreach (PlayerNPC pnpc in playernpcs)
            {
                player.dropMessage(5, ">> " + pnpc.getName() + " - Scriptid: " + pnpc.getScriptId() + " - Oid: " + pnpc.getObjectId());
            }
        }

        if (npcs.Count > 0)
        {
            player.yellowMessage("NPCs on this map:");
            foreach (NPC npc in npcs)
            {
                player.dropMessage(5, ">> " + npc.getName() + " - " + npc.getId() + " - Oid: " + npc.getObjectId());
            }
        }

        if (mobs.Count > 0)
        {
            player.yellowMessage("Monsters on this map:");
            foreach (Monster mob in mobs)
            {
                if (mob.isAlive())
                {
                    player.dropMessage(5, ">> " + mob.getName() + " - " + mob.getId() + " - Oid: " + mob.getObjectId());
                }
            }
        }
    }
}
