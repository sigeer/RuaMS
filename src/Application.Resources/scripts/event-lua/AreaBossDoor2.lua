local Common_AreaBoss = require("scripts/event-lua/__BaseAreaBoss")

-- 继承基类方法
for k, v in pairs(Common_AreaBoss) do
    _ENV[k] = v
end

-- 重写 start 方法
function start()
    local posX = 201
    local posY = 80
    Common_AreaBoss.spawnBoss(
        677000005,          -- 地图ID
        9400609,            -- 怪物ID 印第安老斑鸠
        posX,               -- X坐标
        posY,               -- Y坐标
        "Amdusias has appeared!"  -- 提示消息
    )
end