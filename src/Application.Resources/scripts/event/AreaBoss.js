const dataSource = [
    // 仙人掌爸爸沙漠 大宇
    { name: "Deo", mapId: 260010201, mobId: 3220001, pos: [{ x: 645, y: 275 }], mobTime: 3 * 60 * 60, msg: "Deo slowly appeared out of the sand dust." },
    //青竹武士
    { name: "Bamboo", mapId: 800020120, mobId: 6090002, pos: [{ x: 560, y: 50 }], mobTime: 3 * 60 * 60, msg: "From amongst the ruins shrouded by the mists, Bamboo Warrior appears." },
    //八十年药草地 巨型蜈蚣
    { name: "Centipede", mapId: 251010102, mobId: 5220004, pos: [{ x: 560, y: 50 }], mobTime: 3 * 60 * 60, msg: "From the mists surrounding the herb garden, the gargantuous Giant Centipede appears." },
    //研究所地下秘密通道 吉米拉
    { name: "Kimera", mapId: 261030000, mobId: 8220002, pos: [{ minX: -900, maxX: 0, y: 180 }], mobTime: 3 * 60 * 60, msg: "吉米拉从地下的黑暗中出现，眼中闪烁着微光。" },
    //阳光沙滩 巨居蟹
    { name: "KingClang", mapId: 110040000, mobId: 5220001, pos: [{ minX: -1600, maxX: 800, y: 140 }], mobTime: 3 * 60 * 60, msg: "一个奇怪的海螺出现在了沙滩上。" },
    //巫婆森林Ⅰ 浮士德
    { name: "Faust1", mapId: 100040105, mobId: 5220002, pos: [{ x: 456, y: 278 }], mobTime: 3 * 60 * 60, msg: "浮士德出现在蓝色迷雾中。" },
    //巫婆森林Ⅱ 浮士德
    { name: "Faust2", mapId: 100040106, mobId: 5220002, pos: [{ x: 474, y: 278 }], mobTime: 3 * 60 * 60, msg: "浮士德出现在蓝色迷雾中。" },
    //天空楼梯Ⅱ 艾利杰
    { name: "Eliza", mapId: 200010300, mobId: 8220000, pos: [{ x: 208, y: 83 }], mobTime: 3 * 60 * 60, msg: "Eliza has appeared with a black whirlwind." },
    //鳄鱼潭Ⅰ 多尔
    { name: "Dyle", mapId: 107000300, mobId: 6220000, pos: [{ x: 90, y: 119 }], mobTime: 3 * 60 * 60, msg: "The huge crocodile Dyle has come out from the swamp." },
    //海岸草丛Ⅲ 红蜗牛王
    { name: "Mano", mapId: 104000400, mobId: 2220000, pos: [{ x: 279, y: -496 }], mobTime: 3 * 60 * 60, msg: "A cool breeze was felt when Mano appeared." },
    //哥雷草原 朱诺
    { name: "Zeno", mapId: 221040301, mobId: 6220001, pos: [{ x: -4224, y: 776 }], mobTime: 3 * 60 * 60, msg: "Zeno has appeared with a heavy sound of machinery." },
    //流浪熊的地盘 肯德熊
    { name: "TaeRoon", mapId: 250010304, mobId: 7220000, pos: [{ minX: -800, maxX: -100, y: 390 }], mobTime: 3 * 60 * 60, msg: "Tae Roon has appeared with a soft whistling sound." },
    //东部岩山Ⅴ 树妖王
    { name: "Stumpy", mapId: 101030404, mobId: 3220000, pos: [{ minX: 400, maxX: 1200, y: 1280 }], mobTime: 3 * 60 * 60, msg: "Stumpy has appeared with a stumping sound that rings the Stone Mountain." },
    //妖怪森林2 妖怪禅师
    { name: "KingSageCat", mapId: 250010504, mobId: 7220002, pos: [{ minX: -500, maxX: 800, y: 540 }], mobTime: 3 * 60 * 60, msg: "周围的鬼气更加浓重了。传来一阵令人不快的猫叫声。" },
    //月岭 九尾狐
    { name: "NineTailedFox", mapId: 222010310, mobId: 7220001, pos: [{ minX: -800, maxX: 500, y: 33 }], mobTime: 3 * 60 * 60, msg: "As the moon light dims, a long fox cry can be heard and the presence of the old fox can be felt" },
    //海草之塔 歇尔夫
    { name: "Seruf", mapId: 230020100, mobId: 4220001, pos: [{ minX: -1500, maxX: 800, y: 520 }], mobTime: 3 * 60 * 60, msg: "A strange shell has appeared from a grove of seaweed" },
    //大海兽 峡谷 大海兽
    { name: "Leviathan", mapId: 240040401, mobId: 8220003, pos: [{ minX: -300, maxX: 300, y: 1125 }], mobTime: 3 * 60 * 60, msg: "Leviathan emerges from the canyon and the cold icy wind blows." },
    //未知的小吃店
    { name: "SnackBar", mapId: 105090310, mobId: 8220008, pos: [{ x: -626, y: -604 }, { x: 735, y: -600 }], mobTime: 3 * 60 * 60, msg: "一个可疑的小吃摊在一个奇怪的偏僻地方慢慢开张了。" },
    ////黑暗独角兽
    //{ name : "Door1", mapId : 677000003, mobId : 9400610, pos : [ { x : 467, y : 0 } ], mobTime : 3 * 60 * 60, msg : "Amdusias has appeared!"},
    ////印第安老斑鸠
    //{ name : "Door2", mapId : 677000005, mobId : 9400609, pos : [ { x : 201, y : 80 } ], mobTime : 3 * 60 * 60, msg : "Andras has appeared!"},
    ////沃勒福
    //{ name : "Door3", mapId : 677000009, mobId : 9400613, pos : [ { x : 251, y : -841 } ], mobTime : 3 * 60 * 60, msg : "Valefor has appeared!"},
    ////地狱大公
    //{ name : "Door4", mapId : 677000012, mobId : 9400633, pos : [ { x : 842, y : 0 } ], mobTime : 3 * 60 * 60, msg : "Astaroth has appeared!"},
    ////牛魔王
    //{ name : "Door5", mapId : 677000001, mobId : 9400612, pos : [ { x : 461, y : 61 } ], mobTime : 3 * 60 * 60, msg : "Marbas has appeared!"},
    //雪之猫女
    //{ name : "Door6", mapId : 677000007, mobId : 9400611, pos : [ { x : 171, y : 50 } ], mobTime : 3 * 60 * 60, msg : "Crocell has appeared!"},
    //时间漩涡 提莫
    { name: "Timer1", mapId: 220050100, mobId: 5220003, pos: [{ minX: -770, maxX: 0, y: 1030 }], mobTime: 3 * 60 * 60, msg: "Tick-Tock Tick-Tock! Timer makes it's presence known." },
    //丢失的时间1 提莫
    { name: "Timer2", mapId: 220050000, mobId: 5220003, pos: [{ minX: -1000, maxX: 400, y: 1030 }], mobTime: 3 * 60 * 60, msg: "Tick-Tock Tick-Tock! Timer makes it's presence known." },
    //丢失的时间2 提莫
    { name: "Timer3", mapId: 220050200, mobId: 5220003, pos: [{ minX: -700, maxX: 700, y: 1030 }], mobTime: 3 * 60 * 60, msg: "Tick-Tock Tick-Tock! Timer makes it's presence known." },
];

function init() {
    for (const item of dataSource) {
        const map = em.GetMap(item.mapId);
        map.SetupAreaBoss(item.name, item.mobId, item.mobTime, item.pos, "Boss_" + item.name);
    }
}

// ////////// FILLER FUNCTIONS //////////

function dispose() { }

function setup(eim, leaderid) { }

function monsterValue(eim, mobid) { return 0; }

function disbandParty(eim, player) { }

function playerDisconnected(eim, player) { }

function playerEntry(eim, player) { }

function monsterKilled(mob, eim) { }

function scheduledTimeout(eim) { }

function afterSetup(eim) { }

function changedLeader(eim, leader) { }

function playerExit(eim, player) { }

function leftParty(eim, player) { }

function clearPQ(eim) { }

function allMonstersDead(eim) { }

function playerUnregistered(eim, player) { }

function cancelSchedule() { }
