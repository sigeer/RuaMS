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


using provider.wz;

namespace provider;

public class DataTool
{
    public static string? getString(Data? data)
    {
        if (data == null)
            return null;

        return (string?)data.getData();
    }

    public static string? getString(string path, Data? data)
    {
        return getString(data?.getChildByPath(path));
    }


    public static double getDouble(Data? data)
    {
        var d = data?.getData();
        if (d == null)
            return 0;
        return (double)d;
    }

    public static float getFloat(Data? data)
    {
        var d = data?.getData();
        if (d == null)
            return 0;
        return (float)d;
    }

    public static int getInt(string path, Data? data, int def = 0)
    {
        return getInt(data?.getChildByPath(path), def);
    }


    public static int getInt(Data? data, int def = 0)
    {
        if (data == null || data.getData() == null)
        {
            return def;
        }
        else if (data.getType() == DataType.STRING)
        {
            if (int.TryParse(getString(data), out var d))
                return d;

            return def;
        }
        else
        {
            object? numData = data.getData();
            if (numData is int)
            {
                return (int)numData;
            }
            else
            {
                return (short)numData;
            }
        }
    }

    public static int getIntConvert(Data? data, int def = 0)
    {
        if (data == null)
        {
            return def;
        }
        if (data.getType() == DataType.STRING)
        {
            string dd = getString(data) ?? "0";
            if (dd.EndsWith("%"))
            {
                dd = dd.Substring(0, dd.Length - 1);
            }

            if (int.TryParse(dd, out var d))
                return d;
            return def;
        }
        else
        {
            return getInt(data, def);
        }
    }

    public static int getIntConvert(string path, Data? data, int def = 0)
    {
        var d = data?.getChildByPath(path);
        return getIntConvert(d, def);
    }




    public static Point? getPoint(Data? data)
    {
        return ((Point?)data?.getData());
    }

    public static Point? getPoint(string path, Data? data, Point? def = null)
    {
        return getPoint(data?.getChildByPath(path)) ?? def;
    }

    public static string getFullDataPath(Data data)
    {
        string path = "";
        DataEntity myData = data;
        while (myData != null)
        {
            path = myData.getName() + "/" + path;
            myData = myData.getParent();
        }
        return path.Substring(0, path.Length - 1);
    }
}
