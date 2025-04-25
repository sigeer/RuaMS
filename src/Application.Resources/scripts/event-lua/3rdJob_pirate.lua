local BaseChallenge = require("scripts/event-lua/__BaseChallenge")

local config = {
    instanceName = "3rdJob_pirate_",
    entryMap = 108010500,
    entryPortal = 0,
    exitMap = 105070200,
    minMapId = 108010500,
    maxMapId = 108010501,
    eventTime = 20,
    maxLobbies = 7,

    resetPQMaps = {108010501},
}

BaseChallenge:new(config)