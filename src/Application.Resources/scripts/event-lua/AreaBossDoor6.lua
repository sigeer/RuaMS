local Common_AreaBoss = require("scripts/event-lua/__BaseAreaBoss")

-- 继承基类方法
for k, v in pairs(Common_AreaBoss) do
    _ENV[k] = v
end

-- 重写 start 方法
function start()
    local posX = 171
    local posY = 50
    Common_AreaBoss.spawnBoss(
        677000007,          -- 地图ID
        9400611,            -- 怪物ID 雪之猫女
        posX,               -- X坐标
        posY,               -- Y坐标
        "Crocell has appeared!"  -- 提示消息
    )
end