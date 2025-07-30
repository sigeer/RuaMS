-- 制作订婚戒指

local dataSource = {
    {
        name = "月长石",
        itemId = 2240000,
        materials = 
        { 
            { itemId = 4011007, quantity = 1 },
            { itemId = 4021007, quantity = 1 }
        },
        costMeso = 30000
    },
    {
        name = "闪耀新星",
        itemId = 2240001,
        materials = 
        { 
            { itemId = 4021009, quantity = 1 },
            { itemId = 4021007, quantity = 1 }
        },
        costMeso = 20000
    },
    {
        name = "金心",
        itemId = 2240002,
        materials = 
        { 
            { itemId = 4011006, quantity = 1 },
            { itemId = 4021007, quantity = 1 }
        },
        costMeso = 10000
    },
    {
        name = "银翼",
        itemId = 2240003,
        materials = 
        { 
            { itemId = 4011004, quantity = 1 },
            { itemId = 4021007, quantity = 1 }
        },
        costMeso = 5000
    }
}

function start( ... )
	levelMain()
end

function levelMain( ... )
    cm:sendSelectLevel("Select", generateSelectionMenu("我是#p9201000#，订婚戒指制造商。我能为您做些什么？", { "我想做个戒指。", "我想扔掉我的戒指盒。" }))
end

function levelSelect0( ... )
	-- body
    if (!cm:isQuestCompleted(100400)) then
        if (!cm:isQuestStarted(100400)) then
            state = 0
            cm:sendNextLevel("StartQuest", "所以你想要制作订婚戒指，是吗？好的，当你从#b#p9201003##k那里得到#r祝福#k后，我可以提供一个。")
        else
            cm:sendOkLevel("Dispose", "在尝试制作订婚戒指之前，先从#b#p9201003#k那里得到祝福。他们一定在你家等着你，就在#r射手村狩猎场#k的那边。")
            return
        end
    else
        if (hasEngagementBox(cm:getPlayer())) then
            cm:sendOkLevel("Dispose", "抱歉，您已经有一个戒指盒了。")
            return
        end

        if (cm:getPlayer().getGender() ~= 0) then
            cm:sendOkLevel("Dispose", "抱歉，戒指盒目前只适用于男性。")
            return
        end

        cm:sendNextSelectLevel("SelectRing", generateSelectionMenu("那么，你想让我制作什么样的订婚戒指？", {"月光石", "星星宝石", "金心", "银天鹅"}))
    end
end

function hasEngagementBox(player)
    for i=2240000,2240003 do
	    if (player:haveItem(i)) then
            return true
        end
    end
    return false
end


function levelSelect1()
    if (hasEngagementBox(cm:getPlayer())) then
        for i = 2240000, 2240003 do
            cm:removeAll(i)
        end

        cm:sendOkLevel("Dispose", "你的戒指盒已被丢弃。")
    else
        cm:sendOkLevel("Dispose", "你没有戒指盒可以丢弃。")
    end
end

function levelStartQuest( ... )
    cm:sendOkLevel("Dispose", "他们住在哪里，你问？哦，这可追溯很久了……你知道，我是他们的朋友，我是那个制作并亲自送交他们订婚戒指的人。他们住在#r林中之城狩猎场#k的后面，我相信你知道那是哪里。")
    cm:startQuest(100400)
end

local selected
function levelSelectRing(idx)
                
    selected = dataSource[idx]

    local prompt = "然后我会给你做一个 #b#t" .. selected.name .. "##k, 那样对吗?"
    prompt += " 在这种情况下，我需要你提供特定的物品才能完成。不过，请确保你的库存中有足够的空间！#b"
        
    for i = 0, #selected.materials do
        prompt += "\r\n#i" .. selected.materials[i].itemId .. "# " + selected.materials[i].quantity .. " #t" + selected.materials[i].itemId .. "#"
    end

    if (target.costMeso > 0) then
        prompt += "\r\n#i4031138# " .. target.costMeso .. " meso"
    end

    cm:sendYesNoLevel("Dispose", "ConfirmMake", prompt)
end

function levelConfirmMake() 
    if not cm:canHold(selected.itemId, 1) then
         cm:sendOkLevel("Dispose", "首先检查你的物品栏是否有空位。")
         return
    end

    if cm:getMeso() < selected.costMeso then
         cm:sendOkLevel("Dispose", "对不起，我的服务是需要收费的。在尝试锻造戒指之前，请在这里给我带来正确数量的金币。")
         return
    end

    local materialCheck = true
    for i = 0, #selected.materials do
        if not cm:haveItem(selected.materials[i], selected.materials[i]) then
            materialCheck = false
        end
    end

    if not materialCheck then
         cm:sendOkLevel("Dispose", "嗯，看来你缺少订婚戒指的一些材料。请先提供这些材料，好吗？")
         return
    end

    for i = 0, #selected.materials do
        cm:gainItem(selected.materials[i], -selected.materials[i])
    end

    cm:gainMeso(-selected.costMeso)

    cm:gainItem(selected.itemId, -1)
    cm:sendOkLevel("Dispose", "一切都搞定了，订婚戒指做得非常完美。祝你们订婚快乐。")
end



function generateSelectionMenu(text, options)
    for i, v in ipairs(options) do
        text = text .. "\r\n#L" .. (i - 1) .. "##b" .. v .. "#l"
    end
    return text
end