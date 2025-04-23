local BaseEvent = require("scripts/event-lua/__BasePQ")

-- 配置事件参数
local config = {
    -- 注册的事件名
    instanceName = "MagatiaA",
    minPlayers = 4,
    maxPlayers = 4,
    minLevel = 71,
    maxLevel = 85,
    entryMap = 926110000,
    exitMap = 926110700,
    recruitMap = 261000021,
    clearMap = 926110700,
    warpTeamWhenClear = false,
    minMapId = 926110000,
    maxMapId = 926110600,
    eventTime = 45,
    maxLobbies = 1,

    resetPQMaps = { 926110000, 926110001, 926110100, 926110200, 926110201, 926110202, 926110203, 926110300,
        926110301, 926110302, 926110303, 926110304, 926110400, 926110401, 926110500, 926110600, 926110700 },
    -- base.setup.setEventExclusives 任务特有的道具，需要被清理
    eventItems = { 4001130, 4001131, 4001132, 4001133, 4001134, 4001135 },
    -- base.setup.setEventRewards 奖励设置
    rewardConfig = {
        -- 每一关的经验奖励
        expStages = { 0, 10000, 20000, 0, 20000, 20000, 0, 0 },
        -- 每一关的金钱奖励
        mesoStages = {},
        -- 最终关卡的物品奖励
        finalItem = {
            level = 1,
            list = { 2000003, 2000002, 2000004, 2000005, 2022003, 1032016, 1032015, 1032014, 2041212, 2041020, 2040502,
                2041016, 2044701, 2040301, 2043201, 2040501, 2040704, 2044001, 2043701, 2040803, 1102026, 1102028,
                1102029 },
            quantity = { 100, 100, 20, 10, 50, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 }
        }
    },
    -- base.setup.respawnStages调用 地图怪物重生设置
    respawnConfig = {
        maps = { 926110100, 926110200 },
        duration = 15000
    }
}

-- 创建自定义事件
local Sample = BaseEvent:extend()

-- 没办法通过设置处理的就在这里进行重载
function Sample:setup(level, lobbyid)
    local eim = BaseEvent.setup(self, level, lobbyid)
    eim:setIntProperty("isAlcadno", 1);

    eim:setIntProperty("escortFail", 0);
    eim:setIntProperty("yuleteTimeout", 0);
    eim:setIntProperty("yuleteTalked", 0);
    eim:setIntProperty("yuletePassed", 0);
    eim:setIntProperty("npcShocked", 0);
    eim:setIntProperty("normalClear", 0);

    eim:setIntProperty("statusStg1", 0);
    eim:setIntProperty("statusStg2", 0);
    eim:setIntProperty("statusStg3", 0);
    eim:setIntProperty("statusStg4", 0);
    eim:setIntProperty("statusStg5", 0);
    eim:setIntProperty("statusStg6", 0);
    eim:setIntProperty("statusStg7", 0);

    eim:getInstanceMap(926110201).shuffleReactors(2518000, 2612004);
    eim:getInstanceMap(926110202).shuffleReactors(2518000, 2612004);
    eim:spawnNpc(2112010, Point(252, 243), eim:getInstanceMap(926110203));
    eim:spawnNpc(2112010, Point(200, 100), eim:getInstanceMap(926110401));
    eim:spawnNpc(2112011, Point(200, 100), eim:getInstanceMap(926110500));
    eim:spawnNpc(2112018, Point(200, 100), eim:getInstanceMap(926110600));
    return eim
end

function Sample:afterSetup(eim)
    eim:setIntProperty("escortFail", 0);

    local books = { -1, -1, -1, -1, -1, 0, 0, 0, 0, 0, 0, 0, -1, -1, -1, -1, -1, 1, 1, 1, 1, 1, 1, 1, 2, 3 }
    shuffle(books);

    for i = 1, #books do
        eim:setIntProperty("stg1_b" .. (i - 1), books[i]);
    end
end

function Sample:respawnStages(eim)
    BaseEvent.respawnStages(self, eim)
    if (not eim:isEventCleared()) then
        local mapobj = eim:getMapInstance(926110401);
        local mobcount = mapobj:countMonster(9300150);
        if (mobcount == 0) then
            local mobobj = LifeFactory.getMonster(9300150);
            mapobj:spawnMonsterOnGroundBelow(mobobj, Point(-278, -126));

            mobobj = LifeFactory.getMonster(9300150);
            mapobj:spawnMonsterOnGroundBelow(mobobj, Point(-542, -126));
        elseif (mobcount == 1) then
            local mobobj = LifeFactory.getMonster(9300150);
            mapobj:spawnMonsterOnGroundBelow(mobobj, Point(-542, -126));
        end
    end
end

function Sample:changedMap(eim, player, mapId)
    BaseEvent.changedMap(self, eim, player, mapId)
    if mapId == 926110203 and eim:getIntProperty("yuleteTimeout") == 0 then
        eim:setIntProperty("yuleteTimeout", 1);
        eim:schedule("yuleteAction", 10 * 1000);
    end
end

function Sample:yuleteAction(eim)
    if (eim:getIntProperty("yuleteTalked") == 1) then
        eim:setIntProperty("yuletePassed", 1);

        eim:dropMessage(5,
            "Yulete: Ugh, you guys disgust me. All I desired was to make this nation the greatest alchemy powerhouse of the entire world. If they won't accept this, I will make it true by myself, at any costs!!!");
    else
        eim:dropMessage(5,
            "Yulete: Hahaha... Did you really think I was going to be so disprepared knowing that the Magatia societies' dogs would be coming in my pursuit after my actions? Fools!");
    end
    eim:setIntProperty("yuleteTalked", -1);

    local mapobj = eim:getMapInstance(926110203);
    local mob1 = 9300143
    local mob2 = 9300144

    mapobj:destroyNPC(2112010);

    for i = 1, 5 do
        local mobobj1 = LifeFactory.getMonster(mob1);
        local mobobj2 = LifeFactory.getMonster(mob2);

        mapobj:spawnMonsterOnGroundBelow(mobobj1, Point(-455, 135));
        mapobj:spawnMonsterOnGroundBelow(mobobj2, Point(-455, 135));
    end


    for i = 1, 5 do
        local mobobj1 = LifeFactory.getMonster(mob1);
        local mobobj2 = LifeFactory.getMonster(mob2);

        mapobj.spawnMonsterOnGroundBelow(mobobj1, Point(0, 135));
        mapobj.spawnMonsterOnGroundBelow(mobobj2, Point(0, 135));
    end

    for i = 1, 5 do
        local mobobj1 = LifeFactory.getMonster(mob1);
        local mobobj2 = LifeFactory.getMonster(mob2);

        mapobj.spawnMonsterOnGroundBelow(mobobj1, Point(360, 135));
        mapobj.spawnMonsterOnGroundBelow(mobobj2, Point(360, 135));
    end
end

function monsterKilled(mob, eim)
    local map = mob.getMap();

    if (map:getId() == 926110001 and eim:getIntProperty("statusStg1") == 1) then
        if (map:countMonsters() == 0) then
            eim:showClearEffect();
            eim:giveEventPlayersStageReward(2);
            eim:setIntProperty("statusStg2", 1);
        end
    elseif (map.getId() == 926110203 and eim:getIntProperty("statusStg1") == 1) then
        if (map:countMonsters() == 0) then
            eim:showClearEffect();
            eim:giveEventPlayersStageReward(5);

            generateStg6Combo(eim);
            map.getReactorByName("jnr6_out").forceHitReactor(1);
        end
    elseif (mob:getId() == 9300151 or mob:getId() == 9300152) then
        eim:showClearEffect();
        eim:giveEventPlayersStageReward(7);

        eim:spawnNpc(2112005, Point(-370, -150), map);

        local gain = (eim:getIntProperty("escortFail") == 1) and 90000 or ((mob.getId() == 9300139) and 105000 or 140000);
        eim:giveEventPlayersExp(gain);

        map:killAllMonstersNotFriendly();

        if (mob:getId() == 9300139) then
            eim:setIntProperty("normalClear", 1);
        end

        eim:clearPQ();
    end
end

function friendlyKilled(mob, eim)
    eim:setIntProperty("escortFail", 1);
end

-- thanks Chloek3, seth1 for stating generated sequences are supposed to be linked
function generateStg6Combo(eim)
    local matrix = {};

    for i = 1, 4 do
        table.insert(matrix, {})
    end

    for i = 1, 10 do
        local array = { 0, 1, 2, 3 };
        array = shuffle(array);

        for i = 1, 4 do
            matrix[i].push(array[i]);
        end
    end

    for i = 1, 4 do
        local comb = "";
        for i = 1, 10 do
            local r = matrix[i][j];
            comb = comb .. r;
        end

        eim.setProperty("stage6_comb" + (i + 1), comb);
    end
end

-- 创建事件实例
local event = Sample:new(config)

-- 导出所有方法到全局环境（包括继承的方法）
local function exportMethods(obj)
    local exported = {}
    local current = obj
    while current do
        for k, v in pairs(current) do
            if type(v) == "function" and not exported[k] then
                _ENV[k] = function(...)
                    return v(event, ...)
                end
                exported[k] = true
            end
        end
        current = getmetatable(current)
        if current then
            current = current.__index
        end
    end
end

exportMethods(event)
