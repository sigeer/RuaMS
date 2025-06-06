﻿/*
 * Cygnus 1st Job advancement - Thunder Breaker
 */
/*
    Author:         Magical-H
    Description:    骑士团转职通用脚本
 */
var job = {
    1 : "DAWNWARRIOR",				// 魂骑士
    2 : "BLAZEWIZARD",				// 炎术士
    3 : "WINDARCHER",				// 风灵使者
    4 : "NIGHTWALKER",				// 夜行者
    5 : "THUNDERBREAKER"			// 奇袭者
};
var chrLevel = {//定义角色等级限制
    1 : 10,
    2 : 30,
    3 : 70,
    4 : 120
};
var InitialEquip = {    //一转才需要配置发放装备物资
    1 : [
        {1302077 : 1},      //新手战士之剑
    ],
    2 : [
        {1372043 : 1},      //初级魔法师的杖
    ],
    3 : [
        {1452051 : 1},      //初级弓手的弓
        {2060000 : 200}    //弓矢
    ],
    4 : [
        {1472061 : 1},      //初级飞侠拳套
        {2070000 : 200}    //海星镖
    ],
    5 : [
        {1482014 : 1},      //新手专用指套
    ]
}
var medalid = 1142066;             //定义初级勋章，根据转职次数自动发放对应的勋章
var completeQuestID = 29906;        //定义初级勋章任务，根据转职次数自动完成对应的勋章任务
var jobId = null;
var jobLevel = null;
var QuestID = null;
var checkItem = {itemid:[],quantity:[],InitialEquipMsg:[]};

var status = -1;
var jobType = 3;
var canTryFirstJob = true;

function end(mode, type, selection) {
    if(QuestID == null) {
        QuestID = qm.getQuest();
        jobId = String(QuestID).slice(-1);  //通过任务ID获取职业ID
        jobLevel = String(QuestID).substring(2, 3);  //通过任务ID获取几转
        chrLevel = chrLevel[jobLevel];               //通过转职次数绑定等级限制

         job = Job[job[jobId] + jobLevel];   //获取转职职业类
        medalid += (jobLevel - 1);    //绑定对应的转职勋章
        completeQuestID += (jobLevel - 1);    //绑定完成对应给予转职勋章的任务

        InitialEquip = InitialEquip[jobId];     //获取绑定的初始物品列表
        checkItem = InitialEquip.reduce((a, o) => (Object.entries(o).forEach(([i, q]) => (a.itemid.push(Number(i)), a.quantity.push(Number(q)), a.InitialEquipMsg.push(`#b#v${i}##t${i}##k * #r${q}#k`))), a), {itemid: [], quantity: [], InitialEquipMsg: []});  //初始化待检测的物品和数量列表

    }
    if (mode == 0) {
        if (status == 0) {
            qm.sendNext("这个决定..非常重要.");
            qm.dispose();
            return;
        }
        status--;
    } else {
        status++;
    }
    if (status == 0) {
        qm.sendYesNo(`你决定好了嘛? 这会是你最后的决定唷, 所以想清楚你要做什么. 你想要成为#b初级骑士 - ${job.getName()}#k吗?`);
    } else if (status == 1) {
        if (canTryFirstJob) {
            canTryFirstJob = false;
            if (qm.getPlayer().getJob().getId() != (1000+(jobId*100))) {
                if(!qm.canGetFirstJob(jobType)) {
                    qm.sendOk(`请先将等级提升到 #b10级, ${qm.getFirstJobStatRequirement(jobType)}#k 我会告诉你 #r${job.getName()}#k在哪。`);
                    qm.dispose();
                    return;
                }
                //检查是否可以持有足够数量的道具以及勋章
                if (!qm.canHoldAll(checkItem.itemid,checkItem.quantity) || !qm.canHold(medalid)) {
                    qm.sendOk(`请先给背包腾出一定量的空间用于接收初始装备物资。\r\n\r\n\r\n${checkItem.InitialEquipMsg.join("\r\n")}\r\n#b#v${medalid}##t${medalid}##k * #r1#k`);
                    qm.dispose();
                    return;
                } else {
                    qm.changeJob(job);
                    qm.getPlayer().resetStats();
                    qm.forceCompleteQuest();
                    InitialEquip.forEach(o => (Object.entries(o).forEach(([i,q]) => (console.log(i,q),qm.gainItem(Number(i), Number(q))))));
                    qm.gainItem(medalid, 1); //原始流程是女皇任务给的勋章，需要配合WZ给任务加入自动完成任务代码,比较麻烦，在这里给了。
                    qm.completeQuest(completeQuestID); //直接完成女皇的任务，这样女皇头顶不会一直顶着书本。
                    qm.sendNext(`从这一刻起，女皇任命你为#b初级骑士#k！\r\n带上我为你准备初始物资开始历练吧！\r\n\r\n${checkItem.InitialEquipMsg.join("\r\n")}\r\n#b#v${medalid}##t${medalid}##k * #r1#k`);
                }
            }
        } else {
            qm.sendNext("未知错误");
            qm.dispose();
            return;
        }
    } else if (status == 2) {
        qm.sendNextPrev("我还扩大了你的库存量.");
    } else if (status == 3) {
        qm.sendNextPrev("打开技能栏，看看获得的新技能.");
    } else if (status == 4) {
        qm.sendNextPrev("从现在开始，你死亡的时候会损失一部分经验值.");
    } else if (status == 5) {
        qm.sendNextPrev("现在。。。我要你出去向全世界展示皇家的骑士们是如何成长的.");
    } else if (status == 6) {
        qm.dispose();
    }
}