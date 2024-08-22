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
namespace net.server.services;

/**
 * @author Ronan
 */
public class Service
{

    private Type cls;
    private BaseService service;

    public Service(Type s)
    {
        try
        {
            cls = s;
            service = (BaseService)Activator.CreateInstance(cls)!;
        }
        catch (Exception e)
        {
            Log.Logger.Error(e.ToString());
        }
    }

    public BaseService getService()
    {
        return (BaseService)Convert.ChangeType(service, cls);
    }

    public void dispose()
    {
        service.dispose();
        service = null;
    }

}

