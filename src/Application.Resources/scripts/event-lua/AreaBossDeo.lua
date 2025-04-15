local Common_AreaBoss = require("scripts/event-lua/__BaseAreaBoss")

-- 继承基类方法
for k, v in pairs(Common_AreaBoss) do
    _ENV[k] = v
end

-- 重写 start 方法
function start()
    local posX = 645
    local posY = 275
    Common_AreaBoss.spawnBoss(
        260010201,          -- 地图ID
        3220001,            -- 怪物ID 大宇
        posX,               -- X坐标
        posY,               -- Y坐标
        "Deo slowly appeared out of the sand dust."  -- 提示消息
    )
end