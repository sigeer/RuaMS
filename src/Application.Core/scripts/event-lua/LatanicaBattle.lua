local BaseEvent = require("scripts/event-lua/__BasePQ")

-- 配置事件参数
local config = {
    -- 注册的事件名
    instanceName = "Latanica",
    minPlayers = 1,
    maxPlayers = 6,
    minLevel = 1,
    maxLevel = 255,
    entryMap = 541010100,
    exitMap = 541010110,
    recruitMap = 541010060,
    clearMap = 541010110,
    warpTeamWhenClear = false,
    minMapId = 541010100,
    maxMapId = 541010100,
    eventTime = 10,
    maxLobbies = 1,

    -- base.setup.resetMap 中调用
    resetPQMaps = { 541010100 },
}

-- 创建自定义事件
local Sample = BaseEvent:extend()

-- 没办法通过设置处理的就在这里进行重载
function Sample:setup(level, lobbyid)
    local eim = BaseEvent.setup(self, level, lobbyid)
    -- xxx
    eim:setProperty("boss", "0");
    return eim
end

function Sample:monsterKilled(mob, eim)
    if mob:getId() == 9420513 then
        eim:showClearEffect(mob:getMap():getId())
        eim:clearPQ()
    end
end

Sample:new(config)