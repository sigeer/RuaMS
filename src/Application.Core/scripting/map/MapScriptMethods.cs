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


using client;
using constants.id;
using server.quest;
using System.Text;
using tools;
using static client.Character;

namespace scripting.map;

public class MapScriptMethods : AbstractPlayerInteraction
{

    private string rewardstring = " title has been rewarded. Please see NPC Dalair to receive your Medal.";

    public MapScriptMethods(Client c) : base(c)
    {
    }

    public void displayCygnusIntro()
    {
        switch (c.getPlayer().getMapId())
        {
            case MapId.CYGNUS_INTRO_LEAD:
                {
                    lockUI();
                    c.sendPacket(PacketCreator.showIntro("Effect/Direction.img/cygnusJobTutorial/Scene0"));
                    break;
                }
            case MapId.CYGNUS_INTRO_WARRIOR: c.sendPacket(PacketCreator.showIntro("Effect/Direction.img/cygnusJobTutorial/Scene1")); break;
            case MapId.CYGNUS_INTRO_BOWMAN: c.sendPacket(PacketCreator.showIntro("Effect/Direction.img/cygnusJobTutorial/Scene2")); break;
            case MapId.CYGNUS_INTRO_MAGE: c.sendPacket(PacketCreator.showIntro("Effect/Direction.img/cygnusJobTutorial/Scene3")); break;
            case MapId.CYGNUS_INTRO_PIRATE: c.sendPacket(PacketCreator.showIntro("Effect/Direction.img/cygnusJobTutorial/Scene4")); break;
            case MapId.CYGNUS_INTRO_THIEF: c.sendPacket(PacketCreator.showIntro("Effect/Direction.img/cygnusJobTutorial/Scene5")); break;
            case MapId.CYGNUS_INTRO_CONCLUSION:
                {
                    lockUI();
                    c.sendPacket(PacketCreator.showIntro("Effect/Direction.img/cygnusJobTutorial/Scene6"));
                    break;
                }
        }
    }

    public override void displayAranIntro()
    {
        switch (c.getPlayer().getMapId())
        {
            case MapId.ARAN_TUTO_1:
                {
                    lockUI();
                    c.sendPacket(PacketCreator.showIntro("Effect/Direction1.img/aranTutorial/Scene0"));
                    break;
                }
            case MapId.ARAN_TUTO_2:
                c.sendPacket(PacketCreator.showIntro("Effect/Direction1.img/aranTutorial/Scene1" + c.getPlayer().getGender()));
                break;
            case MapId.ARAN_TUTO_3:
                c.sendPacket(PacketCreator.showIntro("Effect/Direction1.img/aranTutorial/Scene2" + c.getPlayer().getGender()));
                break;
            case MapId.ARAN_TUTO_4: c.sendPacket(PacketCreator.showIntro("Effect/Direction1.img/aranTutorial/Scene3")); break;
            case MapId.ARAN_POLEARM:
                {
                    lockUI();
                    c.sendPacket(PacketCreator.showIntro("Effect/Direction1.img/aranTutorial/HandedPoleArm" + c.getPlayer().getGender()));
                    break;
                }
        }
    }

    public void startExplorerExperience()
    {
        switch (c.getPlayer().getMapId())
        {
            case 1020100: //Swordman
                c.sendPacket(PacketCreator.showIntro("Effect/Direction3.img/swordman/Scene" + c.getPlayer().getGender()));
                break;
            case 1020200: //Magician
                c.sendPacket(PacketCreator.showIntro("Effect/Direction3.img/magician/Scene" + c.getPlayer().getGender()));
                break;
            case 1020300: //Archer
                c.sendPacket(PacketCreator.showIntro("Effect/Direction3.img/archer/Scene" + c.getPlayer().getGender()));
                break;
            case 1020400: //Rogue
                c.sendPacket(PacketCreator.showIntro("Effect/Direction3.img/rogue/Scene" + c.getPlayer().getGender()));
                break;
            case 1020500: //Pirate
                c.sendPacket(PacketCreator.showIntro("Effect/Direction3.img/pirate/Scene" + c.getPlayer().getGender()));
                break;
        }
    }

    public void goAdventure()
    {
        lockUI();
        c.sendPacket(PacketCreator.showIntro("Effect/Direction3.img/goAdventure/Scene" + c.getPlayer().getGender()));
    }

    public void goLith()
    {
        lockUI();
        c.sendPacket(PacketCreator.showIntro("Effect/Direction3.img/goLith/Scene" + c.getPlayer().getGender()));
    }

    public void explorerQuest(short questid, string questName)
    {
        Quest quest = Quest.getInstance(questid);
        if (isQuestCompleted(questid))
        {
            return;
        }

        if (!isQuestStarted(questid))
        {
            if (!quest.forceStart(getPlayer(), 9000066))
            {
                return;
            }
        }
        QuestStatus qs = getPlayer().getQuest(quest);
        if (!qs.addMedalMap(getPlayer().getMapId()))
        {
            return;
        }
        string status = qs.getMedalProgress().ToString();
        string infoex = qs.getInfoEx(0);

        // explorer quests all have an infoex/infonumber requirement that points to another quest
        // THAT quest's progress needs to be updated for Quest.canComplete() to return true
        getPlayer().setQuestProgress(quest.getId(), quest.getInfoNumber(qs.getStatus()), status);

        StringBuilder smp = new StringBuilder();
        StringBuilder etm = new StringBuilder();
        if (status.Equals(infoex))
        {
            etm.Append("Earned the ").Append(questName).Append(" title!");
            smp.Append("You have earned the <").Append(questName).Append(">").Append(rewardstring);
            getPlayer().sendPacket(PacketCreator.getShowQuestCompletion(quest.getId()));
        }
        else
        {
            getPlayer().sendPacket(PacketCreator.earnTitleMessage(status + "/" + infoex + " regions explored."));
            etm.Append("Trying for the ").Append(questName).Append(" title.");
            smp.Append("You made progress on the ").Append(questName).Append(" title. ").Append(status).Append("/").Append(infoex);
        }
        getPlayer().sendPacket(PacketCreator.earnTitleMessage(etm.ToString()));
        showInfoText(smp.ToString());
    }

    public void touchTheSky()
    { //29004
        Quest quest = Quest.getInstance(29004);
        if (!isQuestStarted(29004))
        {
            if (!quest.forceStart(getPlayer(), 9000066))
            {
                return;
            }
        }
        QuestStatus qs = getPlayer().getQuest(quest);
        if (!qs.addMedalMap(getPlayer().getMapId()))
        {
            return;
        }
        string status = qs.getMedalProgress().ToString();
        getPlayer().announceUpdateQuest(DelayedQuestUpdate.UPDATE, qs, true);
        getPlayer().sendPacket(PacketCreator.earnTitleMessage(status + "/5 Completed"));
        getPlayer().sendPacket(PacketCreator.earnTitleMessage("The One Who's Touched the Sky title in progress."));
        if (qs.getMedalProgress().ToString() == qs.getInfoEx(0))
        {
            showInfoText("The One Who's Touched the Sky" + rewardstring);
            getPlayer().sendPacket(PacketCreator.getShowQuestCompletion(quest.getId()));
        }
        else
        {
            showInfoText("The One Who's Touched the Sky title in progress. " + status + "/5 Completed");
        }
    }
}
