local Common_AreaBoss = require("scripts/event-lua/__BaseAreaBoss")

-- 继承基类方法
for k, v in pairs(Common_AreaBoss) do
    _ENV[k] = v
end

-- 重写 start 方法
function start()
    local posX = 251
    local posY = -841
    Common_AreaBoss.spawnBoss(
        677000009,          -- 地图ID
        9400613,            -- 怪物ID 沃勒福
        posX,               -- X坐标
        posY,               -- Y坐标
        "Valefor has appeared!"  -- 提示消息
    )
end