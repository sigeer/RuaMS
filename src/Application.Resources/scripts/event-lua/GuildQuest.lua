local BasePQ = require("scripts/event-lua/__BasePQ")

-- 配置事件参数
local config = {
    instanceName = "Guild",
    minPlayers = 6,
    maxPlayers = 30,
    minLevel = 1,
    maxLevel = 255,
    entryMap = 990000000,
    exitMap = 990001100,
    recruitMap = 101030104,
    clearMap = 990001000,
    minMapId = 990000000,
    maxMapId = 990001101,
    eventTime = 90,    -- 90分钟
    maxLobbies = 1,
    
    -- 等待时间（分钟）
    waitTime = 3,
    -- 奖励时间（分钟）
    bonusTime = 0.5,
    
    -- 事件专属道具
    eventItems = {
        1032033, 4001024, 4001025, 4001026, 4001027, 4001028, 4001029, 
        4001030, 4001031, 4001032, 4001033, 4001034, 4001035, 4001037
    },
    
    resetPQMaps = {
        990000000, 990000100, 990000200, 990000300, 990000301,
        990000400, 990000401, 990000410, 990000420, 990000430,
        990000431, 990000440, 990000500, 990000501, 990000502,
        990000600, 990000610, 990000611, 990000620, 990000630,
        990000631, 990000640, 990000641, 990000700, 990000800,
        990000900, 990001000, 990001100, 990001101
    }
}

-- 创建自定义PQ
local GuildQuest = BasePQ:extend()

function GuildQuest:BeforeStartEvent(eim, level, lobbyid)
    eim:setProperty("guild", "0")
    eim:setProperty("canJoin", "1")
    eim:setProperty("canRevive", "0")
    
    -- 设置入场时间戳
    local ts = os.time() * 1000 + (60000 * self.waitTime)
    eim:setProperty("entryTimestamp", tostring(ts))
end

function GuildQuest:getEligibleParty(party)
    local eligible = {}
    local hasLeader = false
    local guildId = 0
    
    if party.Length > 0 then
        -- 获取队长的公会ID
        for _, player in ipairs(party) do
            if player:isLeader() then
                guildId = player:getGuildId()
                break
            end
        end
        
        -- 检查所有成员
        for _, player in ipairs(party) do
            if player:getMapId() == self.recruitMap 
                and player:getLevel() >= self.minLevel 
                and player:getLevel() <= self.maxLevel 
                and player:getGuildId() == guildId then
                
                if player:isLeader() then
                    hasLeader = true
                end
                table.insert(eligible, player)
            end
        end
    end
    
    if not hasLeader then
        eligible = {}
    end
    
    return eligible
end

function GuildQuest:afterSetup(eim)
    local leader = em:getChannelServer():getPlayerStorage():getCharacterById(eim:getLeaderId())
    if leader then
        eim:setProperty("guild", tostring(leader:getGuildId()))
    end
end

function GuildQuest:isTeamAllJobs(eim)
    local eventJobs = eim:getEventPlayersJobs()
    local rangeJobs = tonumber('111110', 2)
    return bit.band(eventJobs, rangeJobs) == rangeJobs
end

function GuildQuest:scheduledTimeout(eim)
    if eim:isEventCleared() then
        eim:warpEventTeam(990001100)
    else
        if eim:getIntProperty("canJoin") == 1 then
            eim:setProperty("canJoin", "0")
            
            if eim:checkEventTeamLacking(true, self.minPlayers) then
                self:endEvent(eim)
            else
                eim:startEventTimer(self.eventTime * 60000)
                
                -- 如果队伍包含所有职业，随机给予一个BUFF
                if self:isTeamAllJobs(eim) then
                    local rnd = math.random(0, 3)
                    eim:applyEventPlayersItemBuff(2023000 + rnd)
                end
            end
        else
            self:endEvent(eim)
        end
    end
end

function GuildQuest:playerUnregistered(eim, player)
    -- 取消所有可能的BUFF
    for i = 0, 3 do
        player:cancelEffect(2023000 + i)
    end
end

function GuildQuest:afterChangedMap(eim, player, mapId)
    if mapId == 990000100 then
        local text = "So, here is the brief. You guys should be warned that, once out on the fortress outskirts, anyone that would not be equipping the #b#t1032033##k will die instantly due to the deteriorated state of the air around there. That being said, once your team moves out, make sure to #bhit the glowing rocks#k in that region and #bequip the dropped item#k before advancing stages. That will protect you thoroughly from the air sickness. Good luck!"
        player:getAbstractPlayerInteraction():npcTalk(9040000, text)
    end
end

function GuildQuest:playerDead(eim, player)
    if player:getMapId() == 990000900 then
        if player:getMap():countAlivePlayers() == 0 and player:getMap():countMonsters() > 0 then
            self:endEvent(eim)
        end
    end
end

function GuildQuest:playerRevive(eim, player)
    if eim:getIntProperty("canRevive") == 0 then
        if eim:isEventTeamLackingNow(true, self.minPlayers, player) and eim:getIntProperty("canJoin") == 0 then
            player:respawn(eim, self.exitMap)
            self:endEvent(eim)
        else
            player:respawn(eim, self.exitMap)
        end
        return false
    end
    return true
end

function GuildQuest:clearPQ(eim)
    eim:stopEventTimer()
    eim:setEventCleared()
    
    eim:warpEventTeam(self.clearMap)
    eim:startEventTimer(self.bonusTime * 60000)
end

function GuildQuest:dispose(eim)
    em:schedule("reopenGuildQuest", em:getLobbyDelay() * 1.5 * 1000)
end

-- 重新开启公会任务
function reopenGuildQuest()
    em:attemptStartGuildInstance()
end

-- 创建事件实例
local event = GuildQuest:new(config)

-- 导出所有方法到全局环境
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