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
using provider;
using provider.wz;
using tools;

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

    private bool isCorrectAnswer(IPlayer chr, int answer)
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
        int gm = 0;
        foreach (var mc in map.getCharacters())
        {
            if (mc.gmLevel() > 1)
            {
                gm++;
            }
        }
        int number = gm;
        map.broadcastMessage(PacketCreator.showOXQuiz(round, question, true));
        TimerManager.getInstance().schedule(() =>
        {
            map.broadcastMessage(PacketCreator.showOXQuiz(round, question, true));
            List<IPlayer> chars = new(map.getCharacters());

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
            if (map.getCharacters().Count - number <= 2)
            {
                map.broadcastMessage(PacketCreator.serverNotice(6, "The event has ended"));
                map.getPortal("join00").setPortalStatus(true);
                map.setOx(null);
                map.setOxQuiz(false);
                //prizes here
                return;
            }
            sendQuestion();
        }, 30000); // Time to answer = 30 seconds ( Ox Quiz packet shows a 30 second timer.
    }

    private static int getOXAnswer(int imgdir, int id)
    {
        return DataTool.getInt(stringData.getData("OXQuiz.img")?.getChildByPath($"{imgdir}/{id}/a"));
    }
}
