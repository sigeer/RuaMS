/*
    This file is part of the HeavenMS MapleStory Server
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
 * @Author Ronan
 * Player NPC Ranking System */

var status;

function start() {
    status = -1;
    action(1, 0, 0);
}

function action(mode, type, selection) {
    if (mode == -1) {
        cm.dispose();
    } else {
        if (mode == 0 && type > 0) {
            cm.dispose();
            return;
        }
        if (mode == 1) {
            status++;
        } else {
            status--;
        }

        if (status == 0) {
            var pnpc = cm.getPlayerNPCByScriptid(cm.getNpc());

            if (pnpc != null) {
                var branchJobName = GameConstants.getJobName(pnpc.job);

                var rankStr = "Hi, I am #b" + pnpc.getName() + "#k, #r" + GameConstants.ordinal(pnpc.JobRank) + "#k in the #r" + branchJobName + "#k class to reach the max level.\r\n";
                rankStr += "\r\n    World rank: #e#b" + GameConstants.ordinal(pnpc.OverallRank) + "#k#n";

                cm.sendOk(rankStr);
            } else {
                cm.sendOk("你好，你好吗？");
            }

            cm.dispose();
        }
    }
}