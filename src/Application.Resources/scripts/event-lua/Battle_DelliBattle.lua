local BaseEvent = require("scripts/event-lua/__BasePQ")

-- 配置事件参数
local config = {
    -- 注册的事件名
    name = "DelliBattle",
    instanceName = "Delli",
    minPlayers = 1,
    maxPlayers = 2,
    minLevel = 120,
    maxLevel = 255,
    entryMap = 925010300,
    exitMap = 925010200,
    recruitMap = 925010200,
    clearMap = 0,
    minMapId = 925010300,
    maxMapId = 925010300,
    eventTime = 6,
    maxLobbies = 7,

    -- base.setup.respawnStages调用 地图怪物重生设置
    respawnConfig = {
        maps = { 925010300 },
        duration = 15000
    },
}

-- 创建自定义事件
local Sample = BaseEvent:extend()

-- 没办法通过设置处理的就在这里进行重载
function Sample:BeforeStartEvent(eim, level, lobbyid)
    -- toggle？
    eim:getMapInstance(self.entryMap):toggleDrops();
end

function Sample:scheduledTimeout(eim)
    eim:getMapInstance(self.entryMap):killAllMonstersNotFriendly();
    eim:showClearEffect();
    BaseEvent.clearPQ(self, eim);
end

function Sample:friendlyKilled(mob, eim)
    if (mob:getId() == 9300162) then
        BaseEvent.endEvent(self, eim);
    end
end

Sample:new(config)
