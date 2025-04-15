local Common_AreaBoss = require("scripts/event-lua/__BaseAreaBoss")

-- 继承基类方法
for k, v in pairs(Common_AreaBoss) do
    _ENV[k] = v
end

-- 重写 start 方法
function start()
    local posX = 467
    local posY = 0
    Common_AreaBoss.spawnBoss(
        677000003,          -- 地图ID
        9400610,            -- 怪物ID 黑暗独角兽
        posX,               -- X坐标
        posY,               -- Y坐标
        "Amdusias has appeared!"  -- 提示消息
    )
end