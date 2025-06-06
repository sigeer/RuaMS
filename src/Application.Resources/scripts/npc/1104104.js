﻿/*
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

var npcid = 1104104;
var baseJob = 15;
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
            if (Math.floor(cm.getJobId() / 100) != baseJob) {
                cm.sendOk("你好，#h0#。你能帮我们找到入侵者吗？他不在这个区域，我已经在这里搜索过了。");
                cm.dispose();
                return;
            }

            cm.sendOk("该死，你找到我了！那么，只有一条出路！让我们战斗，就像#r黑之翼#k应该的那样！");
        } else if (status == 1) {
            var mapobj = cm.getMap();
            var npcpos = mapobj.getMapObject(cm.getNpcObjectId()).getPosition();

            spawnMob(npcpos.x, npcpos.y, 9001009, mapobj);
            mapobj.destroyNPC(npcid);
            cm.dispose();
        }
    }
}

function spawnMob(x, y, id, map) {
    if (map.getMonsterById(id) != null) {
        return;
    }
    var mob = LifeFactory.getMonster(id);
    map.spawnMonsterOnGroundBelow(mob, new Point(x, y));
}