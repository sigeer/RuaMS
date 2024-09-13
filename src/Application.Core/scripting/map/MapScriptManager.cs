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


using Application.Core.Scripting.Infrastructure;
using JavaScriptEngineSwitcher.Core;

namespace scripting.map;

public class MapScriptManager : AbstractScriptManager
{
    private static MapScriptManager instance = new MapScriptManager();

    private Dictionary<string, IEngine> scripts = new();

    public static MapScriptManager getInstance()
    {
        return instance;
    }

    public void reloadScripts()
    {
        scripts.Clear();
    }

    public bool runMapScript(IClient c, string mapScriptPath, bool firstUser)
    {
        if (firstUser)
        {
            var chr = c.OnlinedCharacter;
            int mapid = chr.getMapId();
            if (chr.hasEntered(mapScriptPath, mapid))
            {
                return false;
            }
            else
            {
                chr.enteredScript(mapScriptPath, mapid);
            }
        }

        var iv = scripts.GetValueOrDefault(mapScriptPath);
        if (iv != null)
        {
            try
            {
                iv.CallFunction("start", new MapScriptMethods(c));
                return true;
            }
            catch (Exception e)
            {
                log.Error(e.ToString());
            }
        }

        try
        {
            iv = getInvocableScriptEngine("map/" + mapScriptPath + ".js");
            if (iv == null)
            {
                return false;
            }

            scripts.AddOrUpdate(mapScriptPath, iv);
            iv.CallFunction("start", new MapScriptMethods(c));
            return true;
        }
        catch (Exception e)
        {
            log.Error(e, "Error running map script {ScriptPath}", mapScriptPath);
        }

        return false;
    }
}