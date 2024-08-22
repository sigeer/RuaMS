/*
    This file is part of the HeavenMS MapleStory Server
    Copyleft (L) 2016 - 2019 RonanLana

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

namespace net.server.services.task.world;

/**
 * @author Ronan
 */
public class CharacterSaveService : BaseService
{

    CharacterSaveScheduler chrSaveScheduler = new CharacterSaveScheduler();

    public override void dispose()
    {
        if (chrSaveScheduler != null)
        {
            chrSaveScheduler.dispose();
            chrSaveScheduler = null;
        }
    }

    public void registerSaveCharacter(int characterId, Action runAction)
    {
        chrSaveScheduler.registerSaveCharacter(characterId, runAction);
    }

    private class CharacterSaveScheduler : BaseScheduler
    {

        public CharacterSaveScheduler() : base()
        {
        }

        public void registerSaveCharacter(int characterId, Action runAction)
        {
            registerEntry(characterId, TempRunnable.Parse(runAction), 0);
        }

        public void unregisterSaveCharacter(int characterId)
        {
            interruptEntry(characterId);
        }

    }

}
