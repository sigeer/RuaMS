local Common_AreaBoss = require("scripts/event-lua/__BaseAreaBoss")

-- 继承基类方法
for k, v in pairs(Common_AreaBoss) do
    _ENV[k] = v
end

-- 重写 start 方法
function start()
    local posX = 560
    local posY = 50
    Common_AreaBoss.spawnBoss(
        251010102,          -- 地图ID
        5220004,            -- 怪物ID 巨型蜈蚣
        posX,               -- X坐标
        posY,               -- Y坐标
        "From the mists surrounding the herb garden, the gargantuous Giant Centipede appears."  -- 提示消息
    )
end