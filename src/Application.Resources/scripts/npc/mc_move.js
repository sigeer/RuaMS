/**
 * 出现在
 * 1. 废弃都市
 * 2. 离开的地方
 * 3. 胜利/失败结算地图
 */
function start() {
    const talkMap = cm.getMapId();
    if (talkMap == 980000000) {
        // 1大厅
        startInstance("PQ_CPQ1");
    } else if (talkMap == 980030000) {
        startInstance("PQ_CPQ2");
    } else if (talkMap == 980000010) {
        cm.sendNextLevel("Exit1", "希望你在怪物嘉年华玩得开心！");
    } else if (talkMap == 980030010) {
        cm.sendNextLevel("Exit2", "希望你在怪物嘉年华玩得开心！");
    } else if (cm.getEventInstance() != null) {
        talkReward();
    }
    else {
        var talk = `你想做什么呢？ 如果你没有参加过怪物嘉年华, 在参加之前，你需要知道一些事情! \r\n#b
            #L0# 前往怪物嘉年华地图 1.#l \r\n
            #L2# 了解怪物嘉年华.#l`;

        cm.sendNextSelectLevel("Talk", talk);
    }

}

function levelExit1() {
    cm.warp(980000000, 0);
}

function levelExit2() {
    cm.warp(980030000, 0);
}

function levelTalk(index) {
    if (index == 0 || index == 1) {
        const targetEm = cm.getEventManager(index == 0 ? "PQ_CPQ1" : "PQ_CPQ2");
        if (cm.getLevel() < targetEm.GetMinLevel()) {
            cm.sendOkLevel(`你必须至少达到${targetEm.MinLevel}级才能参加怪物嘉年华。当你足够强大时，和我交谈。`);
        } else {
            cm.sendOkLevel(`很抱歉，只有等级在${targetEm.MinLevel}到${targetEm.MaxLevel}级之间的玩家才能参加怪物嘉年华活动。`);
        }
    } else if (index == 2) {
        level2();
    } else if (index == 3) {
        level3();
    }
}

function level2() {
    var msg = "你想做什么？\r\n#b#L0# 什么是怪物嘉年华？#l\r\n#L1# 怪物嘉年华概述。#l\r\n#L2# 怪物嘉年华的详细信息。#l\r\n#L3# 其实没什么，我改变主意了。#l";
    cm.sendSelectLevel("About", msg);
}

function levelAbout0() {
    cm.sendNextLevel("About00", "哈哈！我是斯皮格曼，这个怪物嘉年华的领袖。我在这里举办了第一届#b怪物嘉年华#k，等待像你这样的旅行者参加这场盛会！");
}

function levelAbout00() {
    cm.sendNextLevel("About000", "什么是#b怪物嘉年华#k？哈哈哈！可以说这是一次你永远不会忘记的经历！这是与其他像你一样的冒险者进行的战斗！#k");
}

function levelAbout000() {
    cm.sendNextLevel("About0000", "我知道你们用真正的武器互相战斗太危险了；我不会建议这样野蛮的行为。我的朋友，我所提供的是竞争的乐趣。战斗的激动和与如此强大和积极的人竞争的激动。我提议你们的团队和对立团队都召唤怪物，并打败对方团队召唤的怪物。这就是怪物嘉年华的精髓。此外，你可以使用在怪物嘉年华期间赚取的枫币来获得新物品和武器！");
}

function levelAbout0000() {
    cm.sendNextLevel("当然，事情并不那么简单。还有其他方法可以阻止另一组放置怪物，这取决于你如何解决。你觉得呢？对友好竞争感兴趣吗？");
}


function levelAbout1() {
    cm.sendNextLevel("About10", "#b怪物嘉年华#k 由两组进入战场，并释放对方召唤的怪物组成。 #b通过获得的嘉年华点数（CP）来确定胜利者的战斗团队#k。");
}

function levelAbout10() {
    cm.sendNextLevel("About100", "进入嘉年华领域时，你的任务是通过击败来自对立阵营的怪物来获得CP，并使用这些CP来分散对立阵营的注意，防止他们攻击怪物。");
}

function levelAbout100() {
    cm.sendNextLevel("About1000", "有三种方法可以分散对方的注意力：#b召唤怪物、能力和守护者#k。如果你想了解更多关于“详细说明”的内容，我可以给你更深入的了解！");
}

function levelAbout1000() {
    cm.sendNextLevel("About10000", "请记住，把你的CP留着从来都不是一个好主意。#b你使用的CP将决定怪物嘉年华的胜负。");
}

function levelAbout10000() {
    cm.sendNextLevel("哦，不用担心变成幽灵。在怪物嘉年华中，你死后不会损失经验。所以这真的是一种独特的体验！");
}

function levelAbout2() {
    cm.sendNextLevel("About20", "当你进入嘉年华场地时，你会看到怪物列表窗口出现。你只需要#b选择你想要使用的东西，然后按下确定#k。非常简单，对吧？");
}

function levelAbout20() {
    cm.sendNextLevel("About200", "一旦你习惯了这些指令，试着使用 #bTAB 和 F1 ~ F12#k。#bTAB 可以在召唤怪物/使用技能/保护者之间切换#k，而 #bF1 ~ F12 可以直接让你访问其中一个窗口#k。");
}

function levelAbout200() {
    cm.sendNextLevel("About2000", "召唤怪物会召唤一个攻击对方队伍的怪物，并将其控制在其下。使用CP召唤一个被召唤的怪物，它将出现在同一区域，攻击对方团队。");
}

function levelAbout2000() {
    cm.sendNextLevel("About20000", "#b能力#k 是一种选项，可以使用黑暗、虚弱等能力来阻止对方小队杀死其他怪物。不需要太多的CP，但是非常值得。唯一的问题是它们持续时间不长。明智地使用这种策略！");
}

function levelAbout20000() {
    cm.sendNextLevel("About200000", "#b守护者#k基本上是一个被召唤的物品，可以大幅增加你的小队召唤的怪物的能力。守护者会一直起作用，直到被对方小队摧毁，所以我希望你先召唤几个怪物，然后再带上守护者。");
}

function levelAbout200000() {
    cm.sendNextLevel("最后，在怪物嘉年华中，你不能使用随身携带的物品/恢复药水。与此同时，怪物会让这些物品掉落。当你拾取这些物品时，它们会立即激活。因此，知道何时获取这些物品非常重要。");
}

function level3() {
    cm.sendSelectLevel("Exchange", `记住，如果你有#t4001129#，你可以用它来兑换物品。选择你想要兑换的物品！\r\n#b
    #L0# #t1122007#（50 纪念币）#l\r\n
    #L1# #t2041211#（40 纪念币）#l\r\n`);
}

function levelExchange0() {
    if (cm.haveItem(4001129, 50) && cm.canHold(1122007)) {
        cm.gainItem(1122007, 1);
        cm.gainItem(4001129, -50);
        cm.dispose();
    } else {
        cm.sendOkLevel(cm.GetClientMessage("Exchange_Fail", 4001129, "EQP"));
        cm.dispose();
    }
}

function levelExchange1() {
    if (cm.haveItem(4001129, 40) && cm.canHold(1122007)) {
        cm.gainItem(1122007, 1);
        cm.gainItem(4001129, -40);
        cm.dispose();
    } else {
        cm.sendOkLevel(cm.GetClientMessage("Exchange_Fail", 4001129, "USE"));
        cm.dispose();
    }
}

var em;
function startInstance(cpqEvent) {
    em = cm.getEventManager(cpqEvent);
    if (em == null) {
        cm.sendOkLevel(cm.GetClientMessage("Event_FatelError"));
        return;
    }

    const msg = cm.GetClientMessage("CPQ_PickRoom") + "#b";
    let o = msg.length;
    const roomName = cm.GetClientMessage("CPQ_Room");
    const levelName = cm.GetClientMessage("Level");
    for (var i = 0; i < em.Rooms.Count; i++) {
        var room = em.Rooms[i];
        if (room.Instance == null) {
            msg += `#L${i}# ${roomName}${i + 1} （${room.MinCount}x${room.MinCount}）#l`;
        } else if (room.Instance.Team1 == null) {
            msg += `#L${i}# ${roomName}${i + 1} （${levelName}: ${room.Instance.GetAveLevel()} / ${room.Instance.GetRoomSize()}x${room.Instance.GetRoomSize()}）#l`;
        }
    }
    if (msg.length === o) {
        cm.sendOkLevel(cm.GetClientMessage("CPQ_NoEmptyRoom"));
        return;
    }
    cm.sendNextSelectLevel("SelectRoom", msg);
}

function levelSelectRoom(roomIndex) {
    if (em == null) {
        cm.sendOkLevel(cm.GetClientMessage("Event_FatelError"));
        return;
    }

    const team = em.GetPreparationParty(roomIndex, cm.getParty());
    if (team.Count == 0) {
        cm.sendOkLevel("队伍不满足条件。");
        return;
    }

    const room = em.GetRoom(roomIndex);
    if (room == null) {
        cm.sendOkLevel(cm.GetClientMessage("CPQ_Error"));
        return;
    }

    if (room.Instance == null) {
        if (!em.StartInstance(cm.getPlayer(), cm.get, roomIndex)) {
            cm.sendOkLevel(cm.GetClientMessage("CPQ_Error"));
            return;
        } else {
            cm.sendOkLevel(cm.GetClientMessage("CPQ_EntryLobby"));
        }
    } else {
        const joinResult = em.JoinInstance(cm.getPlayer(), cm.get, roomIndex);
        if (joinResult == 0) {
            cm.sendOkLevel(cm.GetClientMessage("CPQ_ChallengeRoomSent"));
            return;
        } else if (joinResult == 2) {
            cm.sendOkLevel("队伍不满足条件。需要与被挑战的队伍人数一致！");
        } else if (joinResult == 3) {
            cm.sendOkLevel(cm.GetClientMessage("CPQ_FindError"));
        } else if (joinResult == 4) {
            cm.sendOkLevel(cm.GetClientMessage("CPQ_ChallengeRoomAnswer"));
        }
        else {
            cm.sendOkLevel(cm.GetClientMessage("CPQ_Error"));
            return;
        }
    }
}

function talkReward() {
    if (cm.getEventInstance() == null) {
        cm.sendOkLevel("");
        return;
    }

    if (cm.getEventInstance().IsWinner(cm.getPlayer())) {
        if (cm.getPlayer().TotalCP >= 300) {
            cm.SendParamedNextLevel("Reward", 0, "恭喜你的胜利！表现太棒了！对方队伍毫无还手之力！希望下次也能有同样出色的表现！\r\n\r\n#b你的成绩：#rA#k");
        } else if (cm.getPlayer().TotalCP >= 100) {
            cm.SendParamedNextLevel("Reward", 1, "恭喜你的胜利！太棒了！你对抗对方团队做得很好！再坚持一会儿，下次你肯定能拿到A！\r\n\r\n#b你的成绩：#rB#k");
        } else if (cm.getPlayer().TotalCP >= 50) {
            cm.SendParamedNextLevel("Reward", 2, "恭喜你的胜利。你做了一些事情，但这不能算是一个好的胜利。我期待你下次能做得更好。\r\n\r\n#b你的成绩：#rC#k");
        } else {
            cm.SendParamedNextLevel("Reward", 3, "恭喜你的胜利，尽管你的表现并没有完全体现出来。在下一次怪物嘉年华中更加积极参与吧！\r\n\r\n#b你的成绩：#rD#k");
        }
    } else {
        if (cm.getPlayer().TotalCP >= 300) {
            cm.SendParamedNextLevel("Reward", 4, "很遗憾，尽管你表现出色，但你要么平局要么输掉了这场战斗。下次胜利就属于你了！\r\n\r\n#b你的成绩：#rA#k");
        } else if (cm.getPlayer().TotalCP >= 100) {
            cm.SendParamedNextLevel("Reward", 5, "很遗憾，即使你表现出色，你要么平局要么失败了这场战斗。只差一点点，胜利就可能属于你了！\r\n\r\n#b你的成绩：#rB#k");
        } else if (cm.getPlayer().TotalCP >= 50) {
            cm.SendParamedNextLevel("Reward", 6, "很遗憾，你要么平局要么失败了。胜利属于那些努力奋斗的人。我看到了你的努力，所以胜利离你并不遥远。继续努力吧！\r\n##b你的成绩：#rC#k");
        } else {
            cm.SendParamedNextLevel("Reward", 7, "很遗憾，你要么打成了平局，要么输掉了战斗，你的表现清楚地反映了这一点。我希望你下次能做得更好。\r\n\r\n#b你的成绩：#rD#k");
        }
    }
}

function levelReward(value) {
    let eim = cm.getEventInstance();
    eim.giveEventReward(cm.getPlayer(), value);
    eim.unregisterPlayer(cm.getPlayer());
    cm.warp(eim.Room.RecruitMap);

}