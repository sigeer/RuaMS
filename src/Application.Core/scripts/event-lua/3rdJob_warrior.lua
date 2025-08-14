local BaseChallenge = require("scripts/event-lua/__BaseChallenge")

local config = {
    instanceName = "3rdJob_warrior_",
    entryMap = 108010300,
    entryPortal = 0,
    exitMap = 105070001,
    minMapId = 108010300,
    maxMapId = 108010301,
    eventTime = 20,
    maxLobbies = 7,

    resetPQMaps = { 108010301 }
}

BaseChallenge:new(config)
