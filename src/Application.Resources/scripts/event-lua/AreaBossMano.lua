local Common_AreaBoss = require("scripts/event-lua/__BaseAreaBoss")

-- 继承基类方法
for k, v in pairs(Common_AreaBoss) do
    _ENV[k] = v
end

-- 重写 start 方法
function start()
    local posX = 279
    local posY = -496
    Common_AreaBoss.spawnBoss(
        104000400,          -- 地图ID
        2220000,            -- 怪物ID 红蜗牛王
        posX,               -- X坐标
        posY,               -- Y坐标
        "A cool breeze was felt when Mano appeared."  -- 提示消息
    )
end