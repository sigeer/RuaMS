-- 基础事件类，包含所有由C#调用的方法
local BaseEvent = {}
BaseEvent.__index = BaseEvent

-- 构造函数
function BaseEvent:new(config)
    local instance = {}

    config = config or {}
    instance.name = config.name
    return setmetatable(config, self)
end

-- 创建子类的辅助函数
function BaseEvent:extend(subclass)
    subclass = subclass or {}
    setmetatable(subclass, self)
    subclass.__index = subclass
    return subclass
end

-- 在ChannelServer加载后执行初始化操作
function BaseEvent:init()
    return self.eventName
end

-- 同时最多可执行的事件
function BaseEvent:getMaxLobbies()
    return 1
end

-- 从给定的队伍中选择符合资格的团队
function BaseEvent:getEligibleParty(party)
    return {}
end

-- 设置事件实例
function BaseEvent:setup(level, lobbyid)
end

-- 事件实例初始化完毕后触发
function BaseEvent:afterSetup(eim)
end

-- 玩家进入事件
function BaseEvent:playerEntry(eim, player)
end

-- 玩家注销前操作
function BaseEvent:playerUnregistered(eim, player)
end

-- 玩家退出事件
function BaseEvent:playerExit(eim, player)
end

-- 玩家离开队伍
function BaseEvent:playerLeft(eim, player)
end

-- 玩家更换地图
function BaseEvent:changedMap(eim, player, mapId)
end

-- 切换地图完成触发
function BaseEvent:afterChangedMap(eim, player, mapId)
end

-- 更换队长
function BaseEvent:changedLeader(eim, leader)
end

-- 事件超时
function BaseEvent:scheduledTimeout(eim)
    self:endEvent(eim)
end

-- 敌对怪物死亡
function BaseEvent:monsterKilled(mob, eim)
end

function BaseEvent:monsterValue(eim, mobId)
    return 0
end

-- 友好怪物死亡
function BaseEvent:friendlyKilled(mob, eim)

end

-- 所有怪物死亡
function BaseEvent:allMonstersDead(eim)

end

-- 玩家死亡
function BaseEvent:playerDead(eim, player)

end

-- 怪物复活
function BaseEvent:monsterRevive(mob, eim)

end

-- 玩家复活
function BaseEvent:playerRevive(eim, player)
end

function BaseEvent:playerDisconnected(eim, player)
end


-- 给予随机奖励 似乎没有调用过
function BaseEvent:giveRandomEventReward(eim, player)
end

-- 成功完成事件
function BaseEvent:clearPQ(eim)
end

-- 离开队伍
function BaseEvent:leftParty(eim, player)
end

-- 解散队伍
function BaseEvent:disbandParty(eim, player)
end

-- 取消调度
function BaseEvent:cancelSchedule()
end

-- 结束事件实例
function BaseEvent:dispose()
end

--- js版的end，由于lua中end是关键字，改为endEvent
--- 退出所有玩家，结束这个实例
---@param eim any
function BaseEvent:endEvent(eim)
    local party = eim:getPlayers()
    for i = 0, party.Count - 1 do
        self:playerExit(eim, party[i])
    end
    eim:dispose()
end

return BaseEvent
