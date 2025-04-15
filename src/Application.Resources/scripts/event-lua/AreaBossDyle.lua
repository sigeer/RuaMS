local Common_AreaBoss = require("scripts/event-lua/__BaseAreaBoss")

-- 继承基类方法
for k, v in pairs(Common_AreaBoss) do
    _ENV[k] = v
end

-- 重写 start 方法
function start()
    local posX = 90
    local posY = 119
    Common_AreaBoss.spawnBoss(
        107000300,          -- 地图ID
        6220000,            -- 怪物ID 多尔
        posX,               -- X坐标
        posY,               -- Y坐标
        "The huge crocodile Dyle has come out from the swamp."  -- 提示消息
    )
end