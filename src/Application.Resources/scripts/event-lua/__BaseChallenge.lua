-- 单人挑战事件
local BasePQ = require("scripts/event-lua/__BasePQ")

local BaseChallenge = BasePQ:extend()

-- 在ChannelServer加载后执行初始化操作
function BaseChallenge:init()
    em:setProperty("noEntry", "false")
    return self.name
end

function BaseChallenge:setup(level, lobbyId)
    local eim = em:newInstance(self.instanceName .. lobbyId)
    eim:setProperty("level", level)
    eim:setProperty("lobbyId", lobbyId)
    eim:setProperty("boss", 0)
    self:SetupProperty(eim, level, lobbyId)
    self:setEventRewards(eim)
    self:setEventExclusives(eim)
    return eim
end

-- 不同于__BasePQ，地图初始化和计时在进入时触发而不是setup
function BaseChallenge:playerEntry(eim, player)
    local level = eim:getIntProperty("level")
    local lobbyId = eim:getIntProperty("lobbyId")

    self:BeforeStartEvent(eim, level, lobbyId)
    self:ResetMap(eim, level)

    self:respawnStages(eim)
    self:StartEvent(eim, level, lobbyId)
    BasePQ.playerEntry(self, eim, player)
    em:setProperty("noEntry", "true");
end

function BaseChallenge:endEvent(eim)
    BasePQ.endEvent(self, eim)
    em:setProperty("noEntry", "false")
end

return BaseChallenge