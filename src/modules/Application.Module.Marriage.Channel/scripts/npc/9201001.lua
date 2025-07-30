-- 爱的证明

local dataSource = {
    {
        index = 0,
        mapId = 100000000,
        questItem = 4000001,
        questExp = 2000
    },
    {
        index = 1,
        mapId = 103000000,
        questItem = 4000037,
        questExp = 5000
    },
    {
        index = 2,
        mapId = 102000000,
        questItem = 4000215,
        questExp = 10000
    },
    {
        index = 3,
        mapId = 101000000,
        questItem = 4000026,
        questExp = 17000
    },
    {
        index = 4,
        mapId = 200000000,
        questItem = 4000070,
        questExp = 22000
    },
    {
        index = 5,
        mapId = 220000000,
        questItem = 4000128,
        questExp = 30000
    },
}

local baseItem = 4031367
local baseQuest = 100401
local current

function start( ... )
	levelMain()
end

function levelMain( ... )
    if not cm:isQuestStarted(100400) then
        cm:sendOkLevel("Dispose", "你好 #b#h0##k，我是爱之仙子 #p9201001#。")
        return
    end

    current = getNanaLocation(cm:getPlayer())
    if current == nil then
        return
    end

    local itemId = baseItem + current.index
    if cm:haveItem(itemId, 1) then
        cm:sendOkLevel("Dispose", "嘿，你好。你已经从其他娜娜那里得到#t".. itemId .. "#了吗？")
        return
    end

    local questId = baseQuest + current.index
    if cm:isQuestCompleted(questId) then
        cm:sendAcceptDeclineLevel("Dispose", "RestartQuest", "你遗失了我给你的 #k#t" .. itemId .. "##k ？ 好吧，我可以和你分享另一个，但你需要重做我上次问的那个忙，可以吗？我需要你给我带来 #r50#k 个 ##t" + current.questItem + "##k。")
        return
    end

    if cm:isQuestStarted(questId) then
        if processNanaQuest() then
            cm:gainExp(current.questExp * cm:getPlayer().getExpRate())
            cm:completeQuest(questId)
        end
        cm:dispose()
        return
    end

    cm:sendAcceptDeclineLevel("Dispose", "StartQuest","你在找 #k#t".. itemId .. "##k？ 我可以和你分享一个，但你必须帮我一个忙，可以吗？")
end

function levelStartQuest()
    cm:startQuest(baseQuest + current.index)

    cm:sendOkLevel("Dispose", "我需要你收集#r50#k个#r#t" .. current.questId .. "##k。")
end

function levelRestartQuest( ... )
	-- body
end


function getNanaLocation(player)
    for i,v in ipairs(dataSource) do
	    if player:getMapId() == v.mapId then
            return v
        end
    end

    return nil
end

function processNanaQuest()
    local questId = baseQuest + current.index
    local itemId = baseItem + current.index
    if (cm:haveItem(questId, 50)) then
        if (cm:canHold(itemId, 1)) then
            cm:gainItem(questId, -50)
            cm:gainItem(itemId, 1)

            cm:sendOkLevel("Dispose", "咿呀~ 非常感谢，这里拿着 #b#t".. itemId .. "##k。")
            return true
        else
            cm:sendOkLevel("Dispose", "请确保有一个空余的杂项栏位来存放#t".. itemId .. "#")
        end
    else
        cm:sendOkLevel("Dispose", "请聚集到我这里，带着 #b50#k个 #b#t" + current.questItem + "##k。")
    end

    return false
end