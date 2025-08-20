﻿/* ===========================================================
			Ronan Lana
	NPC Name: 		Lita Lawless
	Description: 	Quest - Bounty Hunter - Fool's Gold
=============================================================
Version 1.0 - Script Done.(11/7/2017)
=============================================================
*/

var status = -1;

function start(mode, type, selection) {
    status++;
    if (mode != 1) {
        if (type == 1 && mode == 0) {
            status -= 2;
        } else {
            qm.sendOk("好的，再见。");
            qm.dispose();
            return;
        }
    }
    if (status == 0) {
        var target = "小矮人";
        qm.sendAcceptDecline("嘿，游客！我需要你的帮助。新叶城的居民面临新的威胁。我正在招募任何人，这次的目标是 #r" + target + "#k。你愿意加入吗？");
    } else if (status == 1) {
        var reqs = "#r30 个 #t4032031##k";
        qm.sendOk("非常好。尽快给我 #r" + reqs + "#k，新叶城正在指望你。");
        qm.forceStartQuest();
    } else if (status == 2) {
        qm.dispose();
    }
}
