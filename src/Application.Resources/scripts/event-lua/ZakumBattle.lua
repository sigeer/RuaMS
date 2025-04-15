local BaseEvent = require("scripts/event-lua/__BasePQ")

-- 配置事件参数
local config = {
    -- 注册的事件名
    instanceName = "Zakum",
    minPlayers = 6,
    maxPlayers = 30,
    minLevel = 50,
    maxLevel = 255,
    entryMap = 280030000,
    exitMap = 211042400,
    recruitMap = 211042400,
    clearMap = 211042400,
    minMapId = 280030000,
    maxMapId = 280030000,
    eventTime = 120,
    maxLobbies = 1,

    -- base.setup.resetMap 中调用
    resetConfig = {
        -- 重置地图
        resetPQMaps = {280030000},
        -- 打乱地图reactor顺序
        resetReactorMaps = {}
    }
}

-- 创建自定义事件
local Sample = BaseEvent:extend()

-- 没办法通过设置处理的就在这里进行重载
function Sample:setup(level, lobbyid)
    local eim = BaseEvent.setup(self, level, lobbyid)
    eim:setProperty("canJoin", 1)
    eim:setProperty("defeatedBoss", 0)
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

function Sample:playerUnregistered(eim, player)
    if (eim:isEventCleared()) then
        em:completeQuest(player, 100200, 2030010)
    end
end

function Sample:clearPQ(eim)
    BaseEvent.clearPQ(self, eim)
    em:getChannelServer():getMapFactory():getMap(211042300):getReactorById(2118002):forceHitReactor(0)
end

function Sample:dispose(eim)
    BaseEvent.clearPQ(self, eim)
    if (eim:isEventCleared()) then
        em:getChannelServer():getMapFactory():getMap(211042300):getReactorById(2118002):forceHitReactor(0)
    end
end

function Sample:monsterKilled(mob, eim)
    if mob:getId() == 8800002 then
        eim:setIntProperty("defeatedBoss", 1)
        eim:showClearEffect(mob:getMap():getId())
        eim:clearPQ()

        mob:getMap():broadcastZakumVictory()
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
