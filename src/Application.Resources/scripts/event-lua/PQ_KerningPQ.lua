local BasePQ = require("scripts/event-lua/__BasePQ")

-- 配置事件参数
local config = {
    name = "KerningPQ",
    instanceName = "Kerning",
    minPlayers = 3,
    maxPlayers = 6,
    minLevel = 21,
    maxLevel = 30,
    entryMap = 103000800,
    exitMap = 103000890,
    recruitMap = 103000000,
    clearMap = 103000805,
    minMapId = 103000800,
    maxMapId = 103000805,
    eventTime = 30,
    maxLobbies = 1,

    respawnConfig = {
        maps = { 103000800, 103000805 },
        duration = 15000
    },

    eventItems = { 4001007, 4001008 },
    rewardConfig = {
        finalItem = {
            level = 1,
            list = { 2040505, 2040514, 2040502, 2040002, 2040602, 2040402, 2040802, 1032009, 1032004, 1032005, 1032006, 1032007, 1032010, 1032002, 1002026, 1002089, 1002090, 2000003, 2000001, 2000002, 2000006, 2022003, 2022000, 2000004, 4003000, 4010000, 4010001, 4010002, 4010003, 4010004, 4010005, 4010006, 4010007, 4020000, 4020001, 4020002, 4020003, 4020004, 4020005, 4020006, 4020007, 4020008 },
            quantity = { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 80, 80, 80, 50, 5, 15, 15, 30, 15, 15, 15, 15, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 3, 3}
        },
        expStages = { 100, 200, 400, 800, 1500}
    }
}

BasePQ:new(config)