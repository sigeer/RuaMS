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
        800020120,          -- 地图ID
        6090002,            -- 怪物ID 青竹武士
        posX,               -- X坐标
        posY,               -- Y坐标
        "From amongst the ruins shrouded by the mists, Bamboo Warrior appears."  -- 提示消息
    )
end