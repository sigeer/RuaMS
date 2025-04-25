local BaseChallenge = require("scripts/event-lua/__BaseChallenge")

local config = {
    name = "q32396",
    instanceName = "q3239_",
    entryMap = 922000000,
    entryPortal = 0,
    exitMap = 922000009,
    minMapId = 922000000,
    maxMapId = 922000000,
    eventTime = 20,
    maxLobbies = 7,

    eventItems = { 4031092 }
}

local Event = BaseChallenge:extend()

function Event:ResetMap(eim)
    local mapObject = eim:getInstanceMap(self.entryMap)
    mapObject:clearDrops()
    mapObject:resetReactors()
    mapObject:shuffleReactors()
end

Event:new(config)
