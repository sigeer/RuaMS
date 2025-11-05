--
--	NPC Name: 		Insiginificant Being
--	Map(s): 		Dungeon : Another Entrance
--	Description: 		Takes you to another Dimension
--

function start()
    if (cm:getQuestStatus(6107) == 1 or cm:getQuestStatus(6108) == 1)
        local ret = checkJob();
        if (ret == -1) then
            cm:sendOk("请组建一个队伍，然后再和我交谈。");
        elseif (ret == 0) then
            cm:sendOk("请确保你的队伍人数是2人。");
        elseif (ret == 1) then
            cm:sendOk("你的团队成员之一的职业不符合进入另一个世界的资格。");
        elseif (ret == 2) then
            cm:sendOk("你的队伍成员之一的等级不符合进入另一个世界的条件。");
        else 
            local em = cm:getEventManager("s4aWorld");
            if (em == null) then
                cm:sendOk("由于未知原因，您不被允许进入。请再试一次。");
            elseif (em:getProperty("started") == "true") then
                cm:sendOk("另外一个世界已经有其他人在尝试击败小巴尔洛格了。");
            else
                local eli = em:getEligibleParty(cm:getParty())
                if (eli.Count > 0) then
                    if (not em:startInstance(cm:getParty(), cm:getPlayer():getMap(), 1)) then
                        cm:sendOk("以你的名义注册的派对已经在此实例中注册。");
                    end
                else
                    cm:sendOk("你目前无法开始这个组队任务，因为你的队伍可能不符合人数要求，有些队员可能不符合尝试条件，或者他们不在这张地图上。如果你找不到队员，可以尝试使用组队搜索功能。");
                end
            end
        end
    else
        cm:sendOk("你不被允许以未知的原因进入另一个世界。");
    end

    cm:dispose();
end

function action(mode, type, selection)
end

function checkJob()
    local party = cm:GetTeamMembers();

    if (party == null) then
        return -1;
    end

    for i = 0, party.Count - 1 do
        local cPlayer = party[i]
        if (cPlayer:getJobId() == 312 or cPlayer:getJobId() == 322 or cPlayer:getJobId() == 900) then
            if cPlayer.Level < 120 then
                return 2
            end
        else
            return 1;
        end
    end

    return 3;
end