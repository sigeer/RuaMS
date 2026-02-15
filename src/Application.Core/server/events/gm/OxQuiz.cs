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


using Application.Core.Channel.Commands;
using Application.Core.Game.Maps;
using Application.Resources.Messages;
using tools;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace server.events.gm;

/**
 * @author FloppyDisk
 */
public class OxQuiz
{
    private int round = 1;
    private int question = 1;
    private IMap map;
    private int expGain = 200;
    private static DataProvider stringData = DataProviderFactory.getDataProvider(WZFiles.ETC);

    public OxQuiz(IMap map)
    {
        this.map = map;
        this.round = Randomizer.nextInt(9);
        this.question = 1;
    }

    private bool isCorrectAnswer(Player chr, int answer)
    {
        double x = chr.getPosition().X;
        double y = chr.getPosition().Y;
        if ((x > -234 && y > -26 && answer == 0) || (x < -234 && y > -26 && answer == 1))
        {
            chr.dropMessage("Correct!");
            return true;
        }
        return false;
    }

    public void sendQuestion()
    {
        map.broadcastMessage(PacketCreator.showOXQuiz(round, question, true));
        map.ChannelServer.Node.TimerManager.schedule(() =>
        {
            map.ChannelServer.Post(new EventOxQuizRunningCommand(this));
        }, 30_000); // Time to answer = 30 seconds ( Ox Quiz packet shows a 30 second timer.
    }

    public void ProcessSendQuestion()
    {
        int gm = 0;
        foreach (var mc in map.getAllPlayers())
        {
            if (mc.isGM())
            {
                gm++;
            }
        }
        int number = gm;

        map.broadcastMessage(PacketCreator.showOXQuiz(round, question, true));
        List<Player> chars = new(map.getAllPlayers());

        foreach (var chr in chars)
        {
            if (chr != null) // make sure they aren't null... maybe something can happen in 12 seconds.
            {
                if (!isCorrectAnswer(chr, getOXAnswer(round, question)) && !chr.isGM())
                {
                    chr.changeMap(chr.getMap().getReturnMap());
                }
                else
                {
                    chr.gainExp(expGain, true, true);
                }
            }
        }
        //do question
        if ((round == 1 && question == 29) || ((round == 2 || round == 3) && question == 17) || ((round == 4 || round == 8) && question == 12) || (round == 5 && question == 26) || (round == 9 && question == 44) || ((round == 6 || round == 7) && question == 16))
        {
            question = 100;
        }
        else
        {
            question++;
        }
        //send question
        if (map.getAllPlayers().Count - number <= 2)
        {
            map.LightBlue(nameof(ClientMessage.Notice_EventEnd));
            map.getPortal("join00")!.setPortalStatus(true);
            map.Ox = null;
            map.setOxQuiz(false);
            //prizes here
            return;
        }
        sendQuestion();
    }

    private static int getOXAnswer(int imgdir, int id)
    {
        return DataTool.getInt(stringData.getData("OXQuiz.img")?.getChildByPath($"{imgdir}/{id}/a"));
    }
}
