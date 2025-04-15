local Common_AreaBoss = require("scripts/event-lua/__BaseAreaBoss")

-- 继承基类方法
for k, v in pairs(Common_AreaBoss) do
    _ENV[k] = v
end

-- 重写 start 方法
function start()
    local posX = 461
    local posY = 61
    Common_AreaBoss.spawnBoss(
        677000001,          -- 地图ID
        9400612,            -- 怪物ID 牛魔王
        posX,               -- X坐标
        posY,               -- Y坐标
        "Marbas has appeared!"  -- 提示消息
    )
end