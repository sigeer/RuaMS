local BaseChallenge = require("scripts/event-lua/__BaseChallenge")

local config = {
    instanceName = "Aran_2ndmount_",
    entryMap = 921110000,
    entryPortal = 2,
    exitMap = 211050000,
    minMapId = 921110000,
    maxMapId = 921110000,
    eventTime = 3,
    maxLobbies = 7
}

local Event = BaseChallenge:extend()

function Event:friendlyKilled(mob, eim)
    if em:getProperty("noEntry") ~= "false" then
        local player = eim:getPlayers()[0]
        self:playerExit(eim, player)
        player:changeMap(self.exitMap)
    end
end

function Event:ResetMap(eim)
    local mapObject = eim:getInstanceMap(self.entryMap)
    mapObject:resetPQ(1)
    mapObject:instanceMapForceRespawn()
end

Event:new(config)