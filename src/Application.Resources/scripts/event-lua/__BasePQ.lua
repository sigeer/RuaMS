local BaseEvent = require("scripts/event-lua/__BaseEvent")

local BasePQ = BaseEvent:extend()

function BasePQ:new(config)
    local instance = {}

    -- 基础配置
    instance.name = config.name
    instance.instanceName = config.instanceName
    instance.minPlayers = config.minPlayers
    instance.maxPlayers = config.maxPlayers
    instance.minLevel = config.minLevel
    instance.maxLevel = config.maxLevel
    instance.entryMap = config.entryMap
    instance.entryPortal = config.entryPortal or 0
    instance.exitMap = config.exitMap
    instance.exitPortal = config.exitPortal or 0
    instance.recruitMap = config.recruitMap
    instance.clearMap = config.clearMap or 0
    -- 事件结束后是否传送到clearMap
    instance.warpTeamWhenClear = config.warpTeamWhenClear or false
    instance.minMapId = config.minMapId
    instance.maxMapId = config.maxMapId
    -- 单位 分钟
    instance.eventTime = config.eventTime
    -- 一个频道最多允许开启多少个事件
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
    instance.resetPQMaps = config.resetPQMaps 
    instance.resetReactorMaps = config.resetReactorMaps 

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
        instance.minPlayers = 1
    end

    return BaseEvent.new(self, instance)
end

-- 在ChannelServer加载后执行初始化操作
function BasePQ:init()
    self:setEventRequirements()
    return self.name
end

function BasePQ:setEventRequirements()
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
function BasePQ:setEventExclusives(eim)
    eim:setExclusiveItems(LuaTableUtils.ToList(self.eventItems))
end

-- 设置所有可能的奖励
function BasePQ:setEventRewards(eim)
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
function BasePQ:getEligibleParty(party)
    local eligible = {}
    local hasLeader = false

    if party.Length > 0 then
        for i = 0, party.Length - 1 do
            local player = party[i]
            if self:FilterTeam(player) then
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

function BasePQ:FilterTeam(player)
    return player:getMapId() == self.recruitMap
        and player:getLevel() >= self.minLevel
        and player:getLevel() <= self.maxLevel
end

-- 设置事件实例
function BasePQ:setup(level, lobbyId)
    local eim = em:newInstance(self.instanceName .. lobbyId)
    eim:setProperty("level", level)
    eim:setProperty("lobbyId", lobbyId)

    self:SetupProperty(eim, level, lobbyId)
    self:BeforeStartEvent(eim, level, lobbyId)
    self:ResetMap(eim, level)

    self:respawnStages(eim)
    self:setEventRewards(eim)
    self:setEventExclusives(eim)
    self:StartEvent(eim, level, lobbyId)
    return eim
end

function BasePQ:SetupProperty(eim, level, lobbyid)
end

function BasePQ:BeforeStartEvent(eim, level, lobbyid)
end

function BasePQ:StartEvent(eim, level, lobbyid)
    eim:startEventTimer(self.eventTime * 60000)
end

-- 定义事件内部允许重生的地图
-- 没有重载这个方法将会默认使用respawnConfig的设置
function BasePQ:respawnStages(eim)
    if (self.respawnConfig.maps and #self.respawnConfig.maps > 0) then
        for _, mapId in ipairs(self.respawnConfig.maps) do
            eim:getInstanceMap(mapId):instanceMapRespawn()
        end
        eim:schedule("respawnStages", self.respawnConfig.duration)
    end
end

-- 非后端调用代码
function BasePQ:ResetMap(eim, level)
    if (self.resetPQMaps) then
        for _, mapId in ipairs(self.resetPQMaps) do
            eim:getInstanceMap(mapId):resetPQ(level)
        end
    end

    if (self.resetReactorMaps) then
        for _, mapId in ipairs(self.resetReactorMaps) do
            eim:getInstanceMap(mapId):shuffleReactors(level)
        end
    end
end

-- 玩家进入事件
function BasePQ:playerEntry(eim, player)
    local map = eim:getMapInstance(self.entryMap)
    player:changeMap(map, map.getPortal(self.entryPortal))
    self:noticePlayerEnter(eim, player)
end

-- 玩家退出事件
function BasePQ:playerExit(eim, player)
    eim:unregisterPlayer(player)
    player:changeMap(self.exitMap, self.exitPortal)
end

-- 玩家离开队伍
function BasePQ:playerLeft(eim, player)
    if (not eim:isEventCleared()) then
        self:playerExit(eim, player)
    end
end

-- 玩家更换地图
function BasePQ:changedMap(eim, player, mapId)
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

-- 更换队长
function BasePQ:changedLeader(eim, leader)
    local mapid = leader:getMapId()
    if (not eim:isEventCleared() and (mapid < self.minMapId or mapid > self.maxMapId)) then
        self:endEvent(eim)
    end
end

-- 事件超时
function BasePQ:scheduledTimeout(eim)
    self:endEvent(eim)
end

function BasePQ:monsterValue(eim, mobId)
    return 1
end

-- 玩家复活
function BasePQ:playerRevive(eim, player)
    eim:unregisterPlayer(player)
    if eim:isEventTeamLackingNow(true, self.minPlayers, player) then
        self:noticePlayerLeft(eim, player)
        self:endEvent(eim)
    else
        self:noticeMemberCount(eim, player)
    end
end

function BasePQ:playerDisconnected(eim, player)
    eim:unregisterPlayer(player)
    if eim:isEventTeamLackingNow(true, self.minPlayers, player) then
        self:noticePlayerLeft(eim, player)
        self:endEvent(eim)
    else
        self:noticeMemberCount(eim, player)
    end
end

-- 给予随机奖励 似乎没有调用过
function BasePQ:giveRandomEventReward(eim, player)
    eim:giveEventReward(player)
end

-- 成功完成事件
function BasePQ:clearPQ(eim)
    eim:stopEventTimer()
    eim:setEventCleared()

    if self.warpTeamWhenClear and self.clearMap > 0 then
        eim:warpEventTeam(self.clearMap)
    end
end

-- 离开队伍
function BasePQ:leftParty(eim, player)
    if eim:isEventTeamLackingNow(false, self.minPlayers, player) then
        self:endEvent(eim)
    else
        self:playerLeft(eim, player)
    end
end

-- 解散队伍
function BasePQ:disbandParty(eim, player)
    if not eim:isEventCleared() then
        self:endEvent(eim)
    end
end

function BasePQ:noticePlayerLeft(eim, player)
end

-- 人数不足
function BasePQ:noticeMemberCount(eim, player)
end

function BasePQ:noticePlayerEnter(eim, player)
    -- body
end

return BasePQ
