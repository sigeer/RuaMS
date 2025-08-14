local BaseChallenge = require("scripts/event-lua/__BaseChallenge")

local config = {
    instanceName = "3rdJob_thief_",
    entryMap = 108010400,
    entryPortal = 0,
    exitMap = 107000402,
    minMapId = 108010400,
    maxMapId = 108010401,
    eventTime = 20,
    maxLobbies = 7,

    resetPQMaps = {108010401},
}

BaseChallenge:new(config)
