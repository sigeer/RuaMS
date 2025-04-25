local BaseChallenge = require("scripts/event-lua/__BaseChallenge")

local config = {
    instanceName = "Aran_3rdmount_",
    entryMap = 914030000,
    entryPortal = 1,
    exitMap = 140010210,
    minMapId = 914030000,
    maxMapId = 914030000,
    eventTime = 3,
    maxLobbies = 7
}

local Event = BaseChallenge:extend()

function Event:ResetMap(eim)
    local mapObject = eim:getInstanceMap(self.entryMap)
    mapObject:resetPQ(1)
    mapObject:instanceMapForceRespawn()
    mapObject:closeMapSpawnPoints()
end

function Event:monsterKilled(mob, eim)
    if (eim.getInstanceMap(self.entryMap):countMonsters() == 0) then
        eim:showClearEffect();
    end
end

function Event:friendlyKilled(mob, eim)
    if em:getProperty("noEntry") ~= "false" then
        local player = eim:getPlayers()[0]
        self:playerExit(eim, player)
        player:changeMap(self.exitMap)
    end
end

Event:new(config)