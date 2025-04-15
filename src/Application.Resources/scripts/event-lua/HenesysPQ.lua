local BaseEvent = require("scripts/event-lua/__BasePQ")

-- 配置事件参数
local config = {
    -- 注册的事件名
    instanceName = "Henesys",
    minPlayers = 3,
    maxPlayers = 6,
    minLevel = 10,
    maxLevel = 255,
    entryMap = 910010000,
    exitMap = 910010300,
    recruitMap = 100000200,
    clearMap = 910010100,
    minMapId = 910010000,
    maxMapId = 910010400,
    eventTime = 10,
    maxLobbies = 1,

    -- base.setup.resetMap 中调用
    resetConfig = {
        -- 重置地图
        resetPQMaps = { 910010000, 910010200 },
        -- 打乱地图reactor顺序
        resetReactorMaps = { }
    },
    -- base.setup.setEventExclusives 任务特有的道具，需要被清理
    eventItems = { 4001095, 4001096, 4001097, 4001098, 4001099, 4001100, 400110 },
    -- base.setup.setEventRewards 奖励设置
    rewardConfig = {
        -- 每一关的经验奖励
        expStages = { 1600 },
        -- 每一关的金钱奖励
        mesoStages = {},
        -- 最终关卡的物品奖励
        finalItem = {
            level = 1,
            list = { 4001158 },
            quantity = { 1 }
        }
    },
    -- base.setup.respawnStages调用 地图怪物重生设置
    respawnConfig = {
        maps = {910010000, 910010200},
        duration = 15000
    },
}

-- 创建自定义事件
local Sample = BaseEvent:extend()

-- 没办法通过设置处理的就在这里进行重载
function Sample:setup(level, lobbyid)
    local eim = BaseEvent.setup(self, level, lobbyid)
    eim:setProperty("stage", "0");
    eim:setProperty("bunnyCake", "0");
    eim:setProperty("bunnyDamaged", "0");
    return eim
end

function Sample:scheduledTimeout(eim)
    if eim:getProperty("1stageclear") then
        local curStage = 910010200
        local toStage = 910010400;
        eim.warpEventTeam(curStage, toStage);
     else 
        BaseEvent.endEvent(self, eim)
     end
end

function Sample:friendlyItemDrop(eim, mob)
    if (mob:getId() == 9300061) then
        local cakes = eim.getIntProperty("bunnyCake") + 1;
        eim:setIntProperty("bunnyCake", cakes);
        mob:getMap():broadcastMessage(PacketCreator.serverNotice(6, "The Moon Bunny made rice cake number " .. cakes .. "."));
    end
end

function Sample:friendlyDamaged(eim, mob)
    if (mob:getId() == 9300061) then
        local bunnyDamage = eim.getIntProperty("bunnyDamaged") + 1;
        if (bunnyDamage > 5) then
            mob:getMap():broadcastMessage(PacketCreator.serverNotice(6, "The Moon Bunny is feeling sick. Please protect it so it can make delicious rice cakes."));
            eim:setIntProperty("bunnyDamaged", 0);
        end
    end
end

function Sample:friendlyKilled(mob, eim)
    if (mob:getId() == 9300061) then
        eim:schedule("bunnyDefeated", 5 * 1000);
    end
end

function Sample:bunnyDefeated(eim)
    eim:dropMessage(5, "Due to your failure to protect the Moon Bunny, you have been transported to the Exile Map.");
    BaseEvent.endEvent(self, eim)
end

function Sample:clearPQ(eim)
   BaseEvent.clearPQ(self, eim)
   eim:warpEventTeam(self.clearMap)
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