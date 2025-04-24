local BasePQ = require("scripts/event-lua/__BasePQ")

-- 配置事件参数
local config = {
    name = "YaoSengPQ",
    instanceName = "YaoSengPQ",
    minPlayers = 1,
    maxPlayers = 6,
    minLevel = 120,
    maxLevel = 255,
    entryMap = 702060000,
    exitMap = 702070400,
    recruitMap = 702070400,
    clearMap = 702060000,
    minMapId = 702060000,
    maxMapId = 702060000,
    eventTime = 30,
    maxLobbies = 1,

    bossConfig = {
        id = 9600025,
        posX = 351,
        posY = 580,
        -- 根据传入的难度值以倍数方式提高怪物的各项指标，在BossID有效的情况下。
        difficulty = true,
        drops = {
            item = 2000005,
            minCount = 0,
            maxCount = 5,
            chance = 400000
        }
    }
}

-- 创建自定义事件
local YaoSengPQ = BasePQ:extend()

function YaoSengPQ:BeforeStartEvent(eim, level, lobbyId)
    self:spawnBossDynamic(eim, level)
end

function YaoSengPQ:spawnBossDynamic(eim, level)
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
        local dropList = {}
        for i, v in ipairs(self.bossConfig.drops) do
            table.insert(dropList, DropItemEntry(v.items, v.chance, v.minCount, v.maxCount))
        end
        mob:SetCustomeDrop(LuaTableUtils.ToListA(dropList, "Application.Core.Game.Life.DropItemEntry"))
        map:spawnMonsterOnGroundBelow(mob, Point(self.bossConfig.posX, self.bossConfig.posY))
    end
end

-- 创建事件实例
local event = YaoSengPQ:new(config)

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