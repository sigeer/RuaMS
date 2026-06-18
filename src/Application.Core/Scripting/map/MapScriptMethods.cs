/*
This file is part of the OdinMS Maple Story Server
Copyright (C) 2008 Patrick Huy <patrick.huy@frz.cc>
Matthias Butz <matze@odinms.de>
Jan Christian Meyer <vimes@odinms.de>

This program is free software: you can redistribute it and/or modify
it under the terms of the GNU Affero General Public License as
published by the Free Software Foundation version 3 as published by
the Free Software Foundation. You may not use, modify or distribute
this program under any other version of the GNU Affero General Public
License.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU Affero General Public License for more details.

You should have received a copy of the GNU Affero General Public License
along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */


using Application.Core.Game.Maps;
using client;
using server.quest;
using System.Text;
using tools;


namespace scripting.map;

public class MapScriptMethods : AbstractPlayerInteraction
{
    public IMap Map { get; }
    private string rewardstring = " title has been rewarded. Please see NPC Dalair to receive your Medal.";

    public MapScriptMethods(IChannelClient c, IMap map) : base(c)
    {
        this.Map = map;
    }

    public override IMap getMap()
    {
        return Map;
    }

    public override int getMapId()
    {
        return Map.Id;
    }

    public async Task displayCygnusIntro()
    {
        switch (c.OnlinedCharacter.getMapId())
        {
            case MapId.CYGNUS_INTRO_LEAD:
                {
                    await lockUI();
                    await c.SendPacket(PacketCreator.showIntro("Effect/Direction.img/cygnusJobTutorial/Scene0"));
                    break;
                }
            case MapId.CYGNUS_INTRO_WARRIOR: await c.SendPacket(PacketCreator.showIntro("Effect/Direction.img/cygnusJobTutorial/Scene1")); break;
            case MapId.CYGNUS_INTRO_BOWMAN: await c.SendPacket(PacketCreator.showIntro("Effect/Direction.img/cygnusJobTutorial/Scene2")); break;
            case MapId.CYGNUS_INTRO_MAGE: await c.SendPacket(PacketCreator.showIntro("Effect/Direction.img/cygnusJobTutorial/Scene3")); break;
            case MapId.CYGNUS_INTRO_PIRATE: await c.SendPacket(PacketCreator.showIntro("Effect/Direction.img/cygnusJobTutorial/Scene4")); break;
            case MapId.CYGNUS_INTRO_THIEF: await c.SendPacket(PacketCreator.showIntro("Effect/Direction.img/cygnusJobTutorial/Scene5")); break;
            case MapId.CYGNUS_INTRO_CONCLUSION:
                {
                    await lockUI();
                    await c.SendPacket(PacketCreator.showIntro("Effect/Direction.img/cygnusJobTutorial/Scene6"));
                    break;
                }
        }
    }

    public override async Task displayAranIntro()
    {
        switch (c.OnlinedCharacter.getMapId())
        {
            case MapId.ARAN_TUTO_1:
                {
                    await lockUI();
                    await c.SendPacket(PacketCreator.showIntro("Effect/Direction1.img/aranTutorial/Scene0"));
                    break;
                }
            case MapId.ARAN_TUTO_2:
                await c.SendPacket(PacketCreator.showIntro("Effect/Direction1.img/aranTutorial/Scene1" + c.OnlinedCharacter.getGender()));
                break;
            case MapId.ARAN_TUTO_3:
                await c.SendPacket(PacketCreator.showIntro("Effect/Direction1.img/aranTutorial/Scene2" + c.OnlinedCharacter.getGender()));
                break;
            case MapId.ARAN_TUTO_4: await c.SendPacket(PacketCreator.showIntro("Effect/Direction1.img/aranTutorial/Scene3")); break;
            case MapId.ARAN_POLEARM:
                {
                    await lockUI();
                    await c.SendPacket(PacketCreator.showIntro("Effect/Direction1.img/aranTutorial/HandedPoleArm" + c.OnlinedCharacter.getGender()));
                    break;
                }
        }
    }

    public async Task startExplorerExperience()
    {
        switch (c.OnlinedCharacter.getMapId())
        {
            case 1020100: //Swordman
                await c.SendPacket(PacketCreator.showIntro("Effect/Direction3.img/swordman/Scene" + c.OnlinedCharacter.getGender()));
                break;
            case 1020200: //Magician
                await c.SendPacket(PacketCreator.showIntro("Effect/Direction3.img/magician/Scene" + c.OnlinedCharacter.getGender()));
                break;
            case 1020300: //Archer
                await c.SendPacket(PacketCreator.showIntro("Effect/Direction3.img/archer/Scene" + c.OnlinedCharacter.getGender()));
                break;
            case 1020400: //Rogue
                await c.SendPacket(PacketCreator.showIntro("Effect/Direction3.img/rogue/Scene" + c.OnlinedCharacter.getGender()));
                break;
            case 1020500: //Pirate
                await c.SendPacket(PacketCreator.showIntro("Effect/Direction3.img/pirate/Scene" + c.OnlinedCharacter.getGender()));
                break;
        }
    }

    public async Task goAdventure()
    {
        await lockUI();
        await c.SendPacket(PacketCreator.showIntro("Effect/Direction3.img/goAdventure/Scene" + c.OnlinedCharacter.getGender()));
    }

    public async Task goLith()
    {
        await lockUI();
        await c.SendPacket(PacketCreator.showIntro("Effect/Direction3.img/goLith/Scene" + c.OnlinedCharacter.getGender()));
    }

    public async Task explorerQuest(short questid)
    {
        Quest quest = Quest.getInstance(questid);
        if (isQuestCompleted(questid))
        {
            return;
        }

        if (!isQuestStarted(questid))
        {
            if (!await quest.forceStart(getPlayer(), 9000066))
            {
                return;
            }
        }
        QuestStatus qs = getPlayer().getQuest(quest);
        if (!qs.addMedalMap(getPlayer().getMapId()))
        {
            return;
        }

        var questName = c.CurrentCulture.GetQuestName(questid);

        string status = qs.getMedalProgress().ToString();
        string infoex = qs.getInfoEx(0);

        // explorer quests all have an infoex/infonumber requirement that points to another quest
        // THAT quest's progress needs to be updated for Quest.canComplete() to return true
        await getPlayer().setQuestProgress(quest.getId(), quest.getInfoNumber(qs.getStatus()), status);

        StringBuilder smp = new StringBuilder();
        StringBuilder etm = new StringBuilder();
        if (status.Equals(infoex))
        {
            etm.Append("Earned the ").Append(questName).Append(" title!");
            smp.Append("You have earned the <").Append(questName).Append(">").Append(rewardstring);
            await getPlayer().SendPacket(PacketCreator.getShowQuestCompletion(quest.getId()));
        }
        else
        {
            await getPlayer().SendPacket(PacketCreator.earnTitleMessage(status + "/" + infoex + " regions explored."));
            etm.Append("Trying for the ").Append(questName).Append(" title.");
            smp.Append("You made progress on the ").Append(questName).Append(" title. ").Append(status).Append("/").Append(infoex);
        }
        await getPlayer().SendPacket(PacketCreator.earnTitleMessage(etm.ToString()));
        await showInfoText(smp.ToString());
    }
}
