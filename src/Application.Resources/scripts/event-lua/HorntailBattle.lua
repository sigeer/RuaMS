local BaseEvent = require("scripts/event-lua/__BasePQ")

-- 配置事件参数
local config = {
    -- 注册的事件名
    instanceName = "Horntail",
    minPlayers = 6,
    maxPlayers = 30,
    minLevel = 100,
    maxLevel = 255,
    entryMap = 240060000,
    exitMap = 240050600,
    recruitMap = 240050400,
    clearMap = 240050600,
    minMapId = 240060000,
    maxMapId = 240060200,
    eventTime = 120,
    maxLobbies = 1,

    -- base.setup.resetMap 中调用
    resetConfig = {
        -- 重置地图
        resetPQMaps = { 240060000, 240060100, 240060200},
        -- 打乱地图reactor顺序
        resetReactorMaps = { }
    },
}

-- 创建自定义事件
local Sample = BaseEvent:extend()

-- 没办法通过设置处理的就在这里进行重载
function Sample:setup(level, lobbyid)
    local eim = BaseEvent.setup(self, level, lobbyid)
    eim:setProperty("canJoin", 1)

    eim:setProperty("defeatedBoss", 0);
    eim:setProperty("defeatedHead", 0);

    local map1 = eim:getInstanceMap(240060000);
    local mob1 = LifeFactory.getMonster(8810000);
    map1.spawnMonsterOnGroundBelow(mob1, Point(960, 120));

    local map2 = eim.getInstanceMap(240060100);
    local mob2 = LifeFactory.getMonster(8810001);
    map2.spawnMonsterOnGroundBelow(mob2, Point(-420, 120));
    return eim
end

function Sample:noticePlayerEnter(eim, player)
    eim:dropMessage(5, "[Expedition] " + player.Name + " has entered the map.")
end

function Sample:noticePlayerLeft(eim, player)
    eim:dropMessage(5, "[Expedition] " + player.Name + " has left the instance.")
end

function Sample:noticeMemberCount(eim, player)
    eim:dropMessage(5,
        "[Expedition] Either the leader has quit the expedition or there is no longer the minimum number of members required to continue it.")
end

function isHorntailHead(mob)
    local mobid = mob:getId();
    return (mobid == 8810000 or mobid == 8810001);
end

function isHorntail(mob)
    local mobid = mob:getId();
    return (mobid == 8810018);
end

function monsterKilled(mob, eim)
    if (isHorntail(mob)) then
        local map = mob:getMap()
        eim:setIntProperty("defeatedBoss", 1);
        eim:showClearEffect(map:getId());
        eim:clearPQ();

        eim:dispatchRaiseQuestMobCount(8810018, 240060200);
        map:broadcastHorntailVictory();
    elseif (isHorntailHead(mob)) then
        local killed = eim.getIntProperty("defeatedHead");
        eim:setIntProperty("defeatedHead", killed + 1);
        eim:showClearEffect(mob:getMap():getId());
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
                _ENV[k] = function(...) return v(event, ...) end
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