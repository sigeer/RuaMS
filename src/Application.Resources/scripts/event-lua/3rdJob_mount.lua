local BaseChallenge = require("scripts/event-lua/__BaseChallenge")

local config = {
    instanceName = "3rdJob_mount_",
    entryMap = 923010000,
    entryPortal = 0,
    exitMap = 923010100,
    minMapId = 923010000,
    maxMapId = 923010000,
    eventTime = 5,
    maxLobbies = 7,
    eventMaps = {923010000}
}

local Event = BaseChallenge:extend()

function Event:SetupProperty(eim, level, lobbyid)
    eim:setProperty("whog_hp", "0")
end

function Event:ResetMap(eim)
    local mapObject = eim:getInstanceMap(self.entryMap)
    mapObject:resetPQ(1)
    mapObject:instanceMapForceRespawn()
end

function Event:respawnStages(eim)
    for _, mapId in ipairs(self.eventMaps) do
        eim:getInstanceMap(mapId):instanceMapRespawn();
    end
    checkHogHealth(eim);

    eim:schedule("respawnStages", 10 * 1000);
end

function checkHogHealth(eim)
    local watchHog = eim:getInstanceMap(923010000):getMonsterById(9300102)
    if watchHog ~= nil then
        local hp = watchHog:getHp()
        local oldHp = eim:getIntProperty("whog_hp")

        if oldHp - hp > 1000 then -- or 800, if using mobHP / eventTime
            eim:dropMessage(6, "Please protect the pig from the aliens!") -- thanks Vcoc
        end
        eim:setIntProperty("whog_hp", hp)
    end
end

function Event:playerExit(eim, player)
    local api = player:getAbstractPlayerInteraction();
    api:removeAll(4031507);
    api:removeAll(4031508);

    BaseChallenge.playerExit(self, eim, player)
end

Event:new(config)

