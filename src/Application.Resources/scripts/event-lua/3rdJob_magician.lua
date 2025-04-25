local BaseChallenge = require("scripts/event-lua/__BaseChallenge")

local config = {
    instanceName = "3rdJob_magician_",
    entryMap = 108010200,
    entryPortal = 0,
    exitMap = 100040106,
    minMapId = 108010200,
    maxMapId = 108010201,
    eventTime = 20,
    maxLobbies = 7,

    resetPQMaps = { 108010201 }
}

BaseChallenge:new(config)