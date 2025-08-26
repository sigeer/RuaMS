local BaseEvent = require("scripts/event-lua/__BasePQ")

-- 配置事件参数
local config = {
    name = "EllinPQ",
    instanceName = "Ellin",
    minPlayers = 4,
    maxPlayers = 6,
    minLevel = 44,
    maxLevel = 55,
    entryMap = 930000000,
    exitMap = 930000800,
    recruitMap = 300030100,
    clearMap = 930000800,
    minMapId = 930000000,
    maxMapId = 930000800,
    eventTime = 30,
    maxLobbies = 1,

    resetPQMaps = {930000000, 930000100, 930000200, 930000300, 930000400, 930000500, 930000600, 930000700},
    resetReactorMaps = { 930000500 },

    eventItems = { 4001162, 4001163, 4001169, 2270004 },
}

-- 创建自定义事件
local EllinPQ = BaseEvent:extend()

function EllinPQ:HandleSetup(eim, level, lobbyid)
    eim:setProperty("statusStg4", 0)
end

function EllinPQ:respawnStages(eim)
    local stg2Map = eim:getMapInstance(930000200)
    if stg2Map:getPlayers().Count > 0 then
        stg2Map:instanceMapRespawn()
    end
    eim:schedule("respawnStages", 4 * 1000)
end

function EllinPQ:isPoisonGolem(mob)
    return (mob:getId() == 9300182);
end

function EllinPQ:monsterKilled(mob, eim, hasKiller)
    local map = mob:getMap();

    if (self:isPoisonGolem(mob)) then
        eim:showClearEffect(map:getId());
        eim:clearPQ();
    elseif (map:countMonsters() == 0) then
        local stage = ((map:getId() % 1000) / 100);

        if (stage == 1 or (stage == 4 and not hasKiller)) then
            eim:showClearEffect(map:getId());
        end
    end
end

EllinPQ:new(config)
