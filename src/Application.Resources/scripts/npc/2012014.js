﻿/*
	This file is part of the OdinMS Maple Story Server
    Copyright (C) 2008 Patrick Huy <patrick.huy@frz.cc> 
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

/**
 -- Odin JavaScript --------------------------------------------------------------------------------
 Orbis Magic Spot - Orbis Tower <1st Floor>(200082100)
 -- By ---------------------------------------------------------------------------------------------
 Unknown
 -- Version Info -----------------------------------------------------------------------------------
 1.2 - Now Official method (action(x,0,0) is weak) by Moogra
 1.1 - Official Text and Method [Information]
 1.0 - First Version by Unknown
 ---------------------------------------------------------------------------------------------------
 **/

function start() {
    if (cm.haveItem(4001019)) {
        cm.sendYesNo("你可以使用#b#t4001019##k来激活#b#p2012014##k。你要传送到#b#p2012015##k所在的地方吗？");
    } else {
        cm.sendOk("有一个#b#p2012014##k可以让你传送到#b#p2012015##k所在的地方，但如果没有卷轴就无法激活它。");
        cm.dispose();
    }
}

function action(mode, type, selection) {
    if (mode > 0) {
        cm.gainItem(4001019, -1);
        cm.warp(200082100, 0);
    }
    cm.dispose();
}