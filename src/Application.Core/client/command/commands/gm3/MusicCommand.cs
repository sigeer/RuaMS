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


using constants.game;
using constants.id;
using tools;

namespace client.command.commands.gm3;

public class MusicCommand : Command
{
    public MusicCommand()
    {
        setDescription("Play a song.");
    }

    private static string getSongList()
    {
        string songList = "Song:\r\n";
        foreach (string s in GameConstants.GAME_SONGS)
        {
            songList += ("  " + s + "\r\n");
        }

        return songList;
    }

    public override void execute(IClient c, string[] paramsValue)
    {

        var player = c.OnlinedCharacter;
        if (paramsValue.Length < 1)
        {
            string sendMsgTemp = "";

            sendMsgTemp += "Syntax: #r!music <song>#k\r\n\r\n";
            sendMsgTemp += getSongList();

            c.sendPacket(PacketCreator.getNPCTalk(NpcId.BILLY, 0, sendMsgTemp, "00 00", 0));
            return;
        }

        string song = player.getLastCommandMessage();
        foreach (string s in GameConstants.GAME_SONGS)
        {
            if (s.Equals(song, StringComparison.OrdinalIgnoreCase))
            {    // thanks Masterrulax for finding an issue here
                player.getMap().broadcastMessage(PacketCreator.musicChange(s));
                player.yellowMessage("Now playing song " + s + ".");
                return;
            }
        }

        string sendMsg = "";
        sendMsg += "Song not found, please enter a song below.\r\n\r\n";
        sendMsg += getSongList();

        c.sendPacket(PacketCreator.getNPCTalk(NpcId.BILLY, 0, sendMsg, "00 00", 0));
    }
}
