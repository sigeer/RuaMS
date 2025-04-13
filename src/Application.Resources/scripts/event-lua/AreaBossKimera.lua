local Common_AreaBoss = require("scripts/event-lua/__BaseAreaBoss")

-- 继承基类方法
for k, v in pairs(Common_AreaBoss) do
    _ENV[k] = v
end

-- 只需要重写 start 方法
function start()
    local posX = math.random(900) - 900
    local posY = 180
    Common_AreaBoss.spawnBoss(
        261030000,          -- 地图ID
        8220002,            -- 怪物ID 吉米拉
        posX,               -- X坐标
        posY,               -- Y坐标
        "奇美拉从地下的黑暗中出现，眼中闪烁着微光。"  -- 提示消息
    )
end