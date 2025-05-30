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

using Application.Shared.Net;

namespace Application.Shared.Constants;

/**
 * @author Ronan
 */
public class OpcodeConstants
{
    public static Dictionary<int, string> sendOpcodeNames = new();
    public static Dictionary<int, string> recvOpcodeNames = new();

    public static void generateOpcodeNames()
    {
        foreach (SendOpcode op in Enum.GetValues<SendOpcode>())
        {
            sendOpcodeNames[(int)op] = op.ToString();
        }
        // WEDDING_TALK 与 WEDDING_TALK_MORE 相同
        foreach (RecvOpcode op in Enum.GetValues<RecvOpcode>())
        {
            recvOpcodeNames[(int)op] = op.ToString();
        }
    }

}
