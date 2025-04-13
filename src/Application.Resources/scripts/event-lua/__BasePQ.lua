-- 基础事件类
local BaseEvent = {}
BaseEvent.__index = BaseEvent

-- 构造函数
function BaseEvent:new(config)
    local instance = {}

    -- 基础配置
    instance.name = config.name
    instance.minPlayers = config.minPlayers
    instance.maxPlayers = config.maxPlayers
    instance.minLevel = config.minLevel
    instance.maxLevel = config.maxLevel
    instance.entryMap = config.entryMap
    instance.exitMap = config.exitMap
    instance.recruitMap = config.recruitMap
    instance.clearMap = config.clearMap
    instance.minMapId = config.minMapId
    instance.maxMapId = config.maxMapId
    -- 是否广播消息
    instance.broadMessage = config.broadMessage or false
    -- 单位 分钟
    instance.eventTime = config.eventTime
    instance.maxLobbies = config.maxLobbies or 7

    instance.eventItems = config.eventItems or {}
    instance.rewardConfig = config.rewardConfig or {
        expStages = {},
        mesoStages = {},
        finalItem = {
            level = 1,
            list = {},
            quantity = {}
        }
    }
    -- 怪物重生
    instance.respawnConfig = config.respawnConfig or {
        maps = {},
        duration = 15000
    }

    instance.resetConfig = config.resetConfig or {
        -- 需要重置地图
        resetPQMaps = {},
        -- 需要打乱reactor
        resetReactorMaps = {}
    }

    instance.bossConfig = config.bossConfig or {
        id = nil,
        posX = 0,
        posY = 0,
        difficulty = false,
        drops = {
            items = {},
            counts = {},
            chances = {}
        }
    }

    -- 如果开启单人模式，则最小人数改为1
    if YamlConfig.config.server.USE_ENABLE_SOLO_EXPEDITIONS then
        self.minPlayers = 1
    end

    return setmetatable(instance, self)
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
    self:setEventRequirements()
end

function BaseEvent:getMaxLobbies()
    return self.maxLobbies
end

function BaseEvent:setEventRequirements()
    -- 设置在招募区域显示的关于事件的要求信息
    local reqStr = ""

    reqStr = reqStr .. "\r\n   组队人数: "
    if self.maxPlayers - self.minPlayers >= 1 then
        reqStr = reqStr .. self.minPlayers .. " ~ " .. self.maxPlayers
    else
        reqStr = reqStr .. self.minPlayers
    end

    reqStr = reqStr .. "\r\n   等级要求: "
    if self.maxLevel - self.minLevel >= 1 then
        reqStr = reqStr .. self.minLevel .. " ~ " .. self.maxLevel
    else
        reqStr = reqStr .. self.minLevel
    end

    reqStr = reqStr .. "\r\n   时间限制: "
    reqStr = reqStr .. self.eventTime .. " 分钟"

    em:setProperty("party", reqStr)
end

-- 设置仅在事件实例中存在的物品（将会在每次实例开始时检查玩家包裹并移除）
function BaseEvent:setEventExclusives(eim)
    eim:setExclusiveItems(LuaTableUtils.ToList(self.eventItems))
end

-- 设置所有可能的奖励
function BaseEvent:setEventRewards(eim)
    if (self.rewardConfig.finalItem) then
        eim:setEventRewards(self.rewardConfig.finalItem.level, LuaTableUtils.ToList(self.rewardConfig.finalItem.list),
            LuaTableUtils.ToList(self.rewardConfig.finalItem.quantity))
    end

    if (self.rewardConfig.expStages and #self.rewardConfig.expStages > 0) then
        eim:setEventClearStageExp(LuaTableUtils.ToList(self.rewardConfig.expStages))
    end

    if (self.rewardConfig.mesoStages and #self.rewardConfig.mesoStages > 0) then
        eim:setEventClearStageMeso(LuaTableUtils.ToList(self.rewardConfig.mesoStages))
    end
end

-- 从给定的队伍中选择符合资格的团队
function BaseEvent:getEligibleParty(party)
    local eligible = {}
    local hasLeader = false

    if #party > 0 then
        for _, player in ipairs(party) do
            if player:getMapId() == self.recruitMap and player:getLevel() >= self.minLevel and player:getLevel() <=
                self.maxLevel then
                if player:isLeader() then
                    hasLeader = true
                end
                table.insert(eligible, player)
            end
        end
    end

    if not (hasLeader and #eligible >= self.minPlayers and #eligible <= self.maxPlayers) then
        eligible = {}
    end

    return eligible
end

-- 设置事件实例
function BaseEvent:setup(level, lobbyid)
    local eim = em:newInstance(self.name .. lobbyid)
    eim:setProperty("level", level)

    self:resetMap(eim, level)

    -- 生成Boss
    if self.bossConfig.id then
        self:spawnBoss(eim, level)
    end

    self:respawnStages(eim)
    eim:startEventTimer(self.eventTime * 60000)
    self:setEventRewards(eim)
    self:setEventExclusives(eim)
    return eim
end

-- 事件实例初始化完毕后触发
function BaseEvent:afterSetup(eim)

end

-- 定义事件内部允许重生的地图
-- 没有重载这个方法将会默认使用respawnConfig的设置
function BaseEvent:respawnStages(eim)
    if (#self.respawnConfig.maps == 0) then
        return
    end

    for _, mapId in ipairs(self.respawnConfig.maps) do
        eim:getInstanceMap(mapId):instanceMapRespawn()
    end
    eim:schedule("respawnStages", self.respawnConfig.duration)
end

-- 非后端调用代码
function BaseEvent:resetMap(eim, level)
    if (#self.resetConfig.resetPQMaps > 0) then
        for _, mapId in ipairs(self.resetConfig.resetPQMaps) do
            eim:getInstanceMap(mapId):resetPQ(level)
        end
    end

    if (#self.resetConfig.resetReactorMaps > 0) then
        for _, mapId in ipairs(self.resetConfig.resetReactorMaps) do
            eim:getInstanceMap(mapId):shuffleReactors(level)
        end
    end
end

-- 玩家进入事件
function BaseEvent:playerEntry(eim, player)
    self:noticePlayerEnter(eim, player)
    local map = eim:getMapInstance(entryMap)
    player:changeMap(map, map.getPortal(0))
end

-- 玩家注销前操作
function BaseEvent:playerUnregistered(eim, player)
end

-- 玩家退出事件
function BaseEvent:playerExit(eim, player)
    eim:unregisterPlayer(player)
    player:changeMap(exitMap, 0)
end

-- 玩家离开队伍
function BaseEvent:playerLeft(eim, player)
    if (not eim:isEventCleared()) then
        self:playerExit(eim, player)
    end
end

-- 玩家更换地图
function BaseEvent:changedMap(eim, player, mapId)
    if mapId < self.minMapId or mapId > self.maxMapId then
        if (eim:isEventTeamLackingNow(true, self.minPlayers, player)) then
            eim:unregisterPlayer(player)
            self:noticePlayerLeft(eim, player)
            self:endEvent(eim)
        else
            self:noticeMemberCount(eim, player)
            eim:unregisterPlayer(player)
        end
    end
end

-- 切换地图完成触发
function BaseEvent:afterChangedMap(eim, player, mapId)
end

-- 更换队长
function BaseEvent:changedLeader(eim, leader)
    local mapid = leader:getMapId()
    if (not eim:isEventCleared() and (mapid < self.minMapId or mapid > self.maxMapId)) then
        self:endEvent(eim)
    end
end

-- 事件超时
function BaseEvent:scheduledTimeout(eim)
    
end

-- 敌对怪物死亡
function BaseEvent:monsterKilled(mob, eim)
end

function BaseEvent:monsterValue(eim, mobId)
    return 1
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
    if eim:isEventTeamLackingNow(true, self.minPlayers, player) then
        eim:unregisterPlayer(player)
        self:noticePlayerLeft(eim, player)
        self:endEvent(eim)
    else
        self:noticeMemberCount(eim, player)
        eim:unregisterPlayer(player)
    end
end

function BaseEvent:playerDisconnected(eim, player)
    if eim:isEventTeamLackingNow(true, self.minPlayers, player) then
        eim:unregisterPlayer(player)
        self:noticePlayerLeft(eim, player)
        self:endEvent(eim)
    else
        self:noticeMemberCount(eim, player)
        eim:unregisterPlayer(player)
    end
end

-- 事件未完成结束
function BaseEvent:finish(eim)

end

-- 给予随机奖励 似乎没有调用过
function BaseEvent:giveRandomEventReward(eim, player)
    eim:giveEventReward(player)
end

-- 成功完成事件
function BaseEvent:clearPQ(eim)
    eim:stopEventTimer()
    eim:setEventCleared()

    -- if self.clearMap then
        -- eim:warpEventTeam(self.clearMap)
    -- end
end

-- 离开队伍
function BaseEvent:leftParty(eim, player)
    if eim:isEventTeamLackingNow(false, self.minPlayers, player) then
        self:endEvent(eim)
    else
        self:playerLeft(eim, player)
    end
end

-- 解散队伍
function BaseEvent:disbandParty(eim, player)
    if not eim:isEventCleared() then
        self:endEvent(eim)
    end
end

-- 移除玩家
function BaseEvent:removePlayer(eim, player)

end

-- 注册嘉年华队伍
function BaseEvent:registerCarnivalParty(eim, carnivalParty)

end

-- 地图加载
function BaseEvent:onMapLoad(eim, player)

end

-- 取消调度
function BaseEvent:cancelSchedule()

end

-- 结束事件实例
function BaseEvent:dispose()

end

function BaseEvent:endEvent(eim)
    local party = eim:getPlayers()
    for i = 0, party.Count - 1 do
        self:playerExit(eim, party[i])
    end
    eim:dispose()
end

function BaseEvent:noticePlayerLeft(eim, player)
end

function BaseEvent:noticeMemberCount(eim, player)
end

function BaseEvent:noticePlayerEnter(eim, player)
	-- body
end


function BaseEvent:spawnBoss(eim, level)
    local mob = LifeFactory.getMonster(self.bossConfig.id)
    if mob then
        local map = eim:getMapInstance(self.entryMap)
        map:killAllMonsters()

        if self.bossConfig.difficulty then
            level = math.max(1, level)
            local stats = mob:getStats()
            local hpMax = math.min(mob:getMaxHp() * level, 2147483647)
            local mpMax = math.min(mob:getMaxMp() * level, 2147483647)

            mob:setStartingHp(hpMax)
            mob:setMp(mpMax)

            stats:setPADamage(stats:getPADamage() * level)
            stats:setPDDamage(stats:getPDDamage() * level)
            stats:setMADamage(stats:getMADamage() * level)
            stats:setMDDamage(stats:getMDDamage() * level)
            mob:setStats(stats)
        end

        map:spawnMonsterOnGroundBelow(mob, Point(self.bossConfig.posX, self.bossConfig.posY))
    end
end

return BaseEvent
