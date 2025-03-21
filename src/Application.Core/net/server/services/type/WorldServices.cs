/*
    This file is part of the HeavenMS MapleStory Server, commands OdinMS-based
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


using net.server.services.task.world;

namespace net.server.services.type;

/**
 * @author Ronan
 */
public class WorldServices : EnumClass, ServiceType<WorldServices>
{

    public readonly static WorldServices SAVE_CHARACTER = new(typeof(CharacterSaveService));

    private Type s;

    private WorldServices(Type service)
    {
        s = service;
    }

    public Service createService()
    {
        return new Service(s);
    }

    public WorldServices[] enumValues()
    {
        return EnumClassUtils.GetValues<WorldServices>().ToArray();
    }

    int ServiceType<WorldServices>.ordinal()
    {
        return ordinal();
    }
}
