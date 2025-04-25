local BaseChallenge = require("scripts/event-lua/__BaseChallenge")

local config = {
    name = "3rdJob_bowman",
    instanceName = "3rdJob_bowman_",
    entryMap = 108010100,
    entryPortal = 0,
    exitMap = 105040305,
    minMapId = 108010100,
    maxMapId = 108010101,
    eventTime = 20,
    maxLobbies = 7,

    resetPQMaps = { 108010101 }
}

BaseChallenge:new(config)
