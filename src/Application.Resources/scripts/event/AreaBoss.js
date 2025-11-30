const dataSource = [
    // 仙人掌爸爸沙漠 大宇
    { name: "Deo", task: null, mapId: 260010201, mobId: 3220001, pos: [{ x: 645, y: 275 }], interval: 3 * 60 * 60 * 1000, msg: "Deo slowly appeared out of the sand dust." },
    //青竹武士
    { name: "Bamboo", task: null, mapId: 800020120, mobId: 6090002, pos: [{ x: 560, y: 50 }], interval: 3 * 60 * 60 * 1000, msg: "From amongst the ruins shrouded by the mists, Bamboo Warrior appears." },
    //八十年药草地 巨型蜈蚣
    { name: "Centipede", task: null, mapId: 251010102, mobId: 5220004, pos: [{ x: 560, y: 50 }], interval: 3 * 60 * 60 * 1000, msg: "From the mists surrounding the herb garden, the gargantuous Giant Centipede appears." },
    //研究所地下秘密通道 吉米拉
    { name: "Kimera", task: null, mapId: 261030000, mobId: 8220002, pos: [{ minX: -900, maxX: 0, y: 180 }], interval: 3 * 60 * 60 * 1000, msg: "吉米拉从地下的黑暗中出现，眼中闪烁着微光。" },
    //阳光沙滩 巨居蟹
    { name: "KingClang", task: null, mapId: 110040000, mobId: 5220001, pos: [{ minX: -1600, maxX: 800, y: 140 }], interval: 3 * 60 * 60 * 1000, msg: "一个奇怪的海螺出现在了沙滩上。" },
    //巫婆森林Ⅰ 浮士德
    { name: "Faust1", task: null, mapId: 100040105, mobId: 5220002, pos: [{ x: 456, y: 278 }], interval: 3 * 60 * 60 * 1000, msg: "浮士德出现在蓝色迷雾中。" },
    //巫婆森林Ⅱ 浮士德
    { name: "Faust2", task: null, mapId: 100040106, mobId: 5220002, pos: [{ x: 474, y: 278 }], interval: 3 * 60 * 60 * 1000, msg: "浮士德出现在蓝色迷雾中。" },
    //天空楼梯Ⅱ 艾利杰
    { name: "Eliza", task: null, mapId: 200010300, mobId: 8220000, pos: [{ x: 208, y: 83 }], interval: 3 * 60 * 60 * 1000, msg: "Eliza has appeared with a black whirlwind." },
    //鳄鱼潭Ⅰ 多尔
    { name: "Dyle", task: null, mapId: 107000300, mobId: 6220000, pos: [{ x: 90, y: 119 }], interval: 3 * 60 * 60 * 1000, msg: "The huge crocodile Dyle has come out from the swamp." },
    //海岸草丛Ⅲ 红蜗牛王
    { name: "Mano", task: null, mapId: 104000400, mobId: 2220000, pos: [{ x: 279, y: -496 }], interval: 3 * 60 * 60 * 1000, msg: "A cool breeze was felt when Mano appeared." },
    //哥雷草原 朱诺
    { name: "Zeno", task: null, mapId: 221040301, mobId: 6220001, pos: [{ x: -4224, y: 776 }], interval: 3 * 60 * 60 * 1000, msg: "Zeno has appeared with a heavy sound of machinery." },
    //流浪熊的地盘 肯德熊
    { name: "TaeRoon", task: null, mapId: 250010304, mobId: 7220000, pos: [{ minX: -800, maxX: -100, y: 390 }], interval: 3 * 60 * 60 * 1000, msg: "Tae Roon has appeared with a soft whistling sound." },
    //东部岩山Ⅴ 树妖王
    { name: "Stumpy", task: null, mapId: 101030404, mobId: 3220000, pos: [{ minX: 400, maxX: 1200, y: 1280 }], interval: 3 * 60 * 60 * 1000, msg: "Stumpy has appeared with a stumping sound that rings the Stone Mountain." },
    //妖怪森林2 妖怪禅师
    { name: "KingSageCat", task: null, mapId: 250010504, mobId: 7220002, pos: [{ minX: -500, maxX: 800, y: 540 }], interval: 3 * 60 * 60 * 1000, msg: "周围的鬼气更加浓重了。传来一阵令人不快的猫叫声。" },
    //月岭 九尾狐
    { name: "NineTailedFox", task: null, mapId: 222010310, mobId: 7220001, pos: [{ minX: -800, maxX: 500, y: 33 }], interval: 3 * 60 * 60 * 1000, msg: "As the moon light dims, a long fox cry can be heard and the presence of the old fox can be felt" },
    //海草之塔 歇尔夫
    { name: "Seruf", task: null, mapId: 230020100, mobId: 4220001, pos: [{ minX: -1500, maxX: 800, y: 520 }], interval: 3 * 60 * 60 * 1000, msg: "A strange shell has appeared from a grove of seaweed" },
    //大海兽 峡谷 大海兽
    { name: "Leviathan", task: null, mapId: 240040401, mobId: 8220003, pos: [{ minX: -300, maxX: 300, y: 1125 }], interval: 3 * 60 * 60 * 1000, msg: "Leviathan emerges from the canyon and the cold icy wind blows." },
    //未知的小吃店
    { name: "SnackBar", task: null, mapId: 105090310, mobId: 8220008, checkMobs: [8220008, 8220009], pos: [{ x: -626, y: -604 }, { x: 735, y: -600 }], interval: 3 * 60 * 60 * 1000, msg: "一个可疑的小吃摊在一个奇怪的偏僻地方慢慢开张了。" },
    ////黑暗独角兽
    //{ name : "Door1", task : null,mapId : 677000003, mobId : 9400610, pos : [ { x : 467, y : 0 } ], interval : 3 * 60 * 60 * 1000, msg : "Amdusias has appeared!"},
    ////印第安老斑鸠
    //{ name : "Door2", task : null,mapId : 677000005, mobId : 9400609, pos : [ { x : 201, y : 80 } ], interval : 3 * 60 * 60 * 1000, msg : "Andras has appeared!"},
    ////沃勒福
    //{ name : "Door3", task : null,mapId : 677000009, mobId : 9400613, pos : [ { x : 251, y : -841 } ], interval : 3 * 60 * 60 * 1000, msg : "Valefor has appeared!"},
    ////地狱大公
    //{ name : "Door4", task : null,mapId : 677000012, mobId : 9400633, pos : [ { x : 842, y : 0 } ], interval : 3 * 60 * 60 * 1000, msg : "Astaroth has appeared!"},
    ////牛魔王
    //{ name : "Door5", task : null,mapId : 677000001, mobId : 9400612, pos : [ { x : 461, y : 61 } ], interval : 3 * 60 * 60 * 1000, msg : "Marbas has appeared!"},
    //雪之猫女
    //{ name : "Door6", task : null,mapId : 677000007, mobId : 9400611, pos : [ { x : 171, y : 50 } ], interval : 3 * 60 * 60 * 1000, msg : "Crocell has appeared!"},
    //时间漩涡 提莫
    { name: "Timer1", task: null, mapId: 220050100, mobId: 5220003, pos: [{ minX: -770, maxX: 0, y: 1030 }], interval: 3 * 60 * 60 * 1000, msg: "Tick-Tock Tick-Tock! Timer makes it's presence known." },
    //丢失的时间1 提莫
    { name: "Timer2", task: null, mapId: 220050000, mobId: 5220003, pos: [{ minX: -1000, maxX: 400, y: 1030 }], interval: 3 * 60 * 60 * 1000, msg: "Tick-Tock Tick-Tock! Timer makes it's presence known." },
    //丢失的时间2 提莫
    { name: "Timer3", task: null, mapId: 220050200, mobId: 5220003, pos: [{ minX: -700, maxX: 700, y: 1030 }], interval: 3 * 60 * 60 * 1000, msg: "Tick-Tock Tick-Tock! Timer makes it's presence known." },
];

function init() {
    for (const item of dataSource) {
        spawn(item);
        globalThis["spawn" + item.name] = () => {
            spawn(item);
        }
    }
}

function spawn(item) {
    const map = em.GetMap(item.mapId);

    const functionName = "spawn" + item.name;
    if (item.checkMobs) {
        for (const checkMob of item.checkMobs) {
            if (map.getMonsterById(checkMob) != null) {
                item.task = em.schedule(functionName, item.interval);
            }
        }
    }
    else {
        if (map.getMonsterById(item.mobId) != null) {
            item.task = em.schedule(functionName, item.interval);
        }
    }

    var index = parseInt(Math.random() * item.pos.length);
    var rndPos = item.pos[index];

    var posX = 0;
    if (rndPos.x)
        posX = rndPos.x;
    else
        posX = Math.floor(randInt(rndPos.minX, rndPos.maxX));

    var spawnpoint = new Point(posX, rndPos.y);

    var monster = LifeFactory.getMonster(item.mobId);
    map.spawnMonsterOnGroundBelow(monster, spawnpoint);
    map.LightBlue("Boss_" + item.name);
    item.task = em.schedule(functionName, item.interval);
}

function randInt(a, b) {
    return Math.floor(Math.random() * (b - a + 1)) + a;
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

