/*
    This file is part of the HeavenMS MapleStory Server
    Copyleft (L) 2017 RonanLana

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


using Application.Core.Game.TheWorld;

namespace net.server.task;



/**
 * @author Ronan
 */
public class WeddingReservationTask : BaseTask
{

    public override void HandleRun()
    {
        foreach (var ch in wserv.getChannels())
        {
            KeyValuePair<bool, KeyValuePair<int, HashSet<int>>>? wedding;

            wedding = ch.getNextWeddingReservation(true);   // start cathedral
            if (wedding != null)
            {
                ch.setOngoingWedding(true, wedding.Value.Key, wedding.Value.Value.Key, wedding.Value.Value.Value);
            }
            else
            {
                ch.setOngoingWedding(true, null, null, null);
            }

            wedding = ch.getNextWeddingReservation(false);  // start chapel
            if (wedding != null)
            {
                ch.setOngoingWedding(false, wedding.Value.Key, wedding.Value.Value.Key, wedding.Value.Value.Value);
            }
            else
            {
                ch.setOngoingWedding(false, null, null, null);
            }
        }
    }

    public WeddingReservationTask(IWorld world) : base(world)
    {
    }
}
