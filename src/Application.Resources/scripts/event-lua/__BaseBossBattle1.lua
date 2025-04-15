-- BOSS事件简单的战役事件
local BasePQ = require("scripts/event-lua/__BasePQ")

local BaseBossBattle = BasePQ:extend()

function BaseBossBattle:new(config)
    return BasePQ:new(config)
end

function BaseBossBattle:setupProperty(eim, level, lobbyId)
    eim:setProperty("boss", "0")
end

function BaseBossBattle:monsterKilled(mob, eim)
    if (mob:getId() == self.bossId) then
        self:onBossKilled(eim)
        eim:showClearEffect();
        eim:clearPQ();
    end
end

function BaseBossBattle:onBossKilled(eim)

end

return BaseBossBattle
