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


using System.Text.RegularExpressions;


namespace tools;



/**
 * Handles converting back and forth from byte arrays to hex strings.
 */
public static class HexTool
{


    /// <summary>
    /// 16进制，大写，有分隔符-
    /// </summary>
    /// <param name="bytes"></param>
    /// <returns></returns>
    public static string toHexString(byte[] bytes)
    {
        return BitConverter.ToString(bytes);
    }

    /// <summary>
    /// 16进制，大写，没有分隔符
    /// </summary>
    /// <param name="bytes"></param>
    /// <returns></returns>
    public static string ToHexString(this byte[] bytes)
    {
        return Convert.ToHexString(bytes);
    }

    /**
     * Convert a hex string to its byte array representation. Two consecutive hex characters are converted to one byte.
     *
     * @param hexString Hex string to convert to bytes. May be lower or upper case, and hex character pairs may be
     *                  delimited by a space or not.
     *                  Example: "01 10 7F FF" is converted to {1, 16, 127, -1}.
     *                  The following hex strings are considered identical and are converted to the same byte array:
     *                  "01 10 7F FF", "01107FFF", "01 10 7f ff", "01107fff"
     * @return The byte array
     */
    public static byte[] toBytes(string hexString)
    {
        return Convert.FromHexString(Regex.Replace(hexString, "\\s", ""));
    }

    public static string toStringFromAscii(byte[] bytes)
    {
        byte[] filteredBytes = new byte[bytes.Length];
        for (int i = 0; i < bytes.Length; i++)
        {
            if (isSpecialCharacter(bytes[i]))
            {
                filteredBytes[i] = (byte)'.';
            }
            else
            {
                filteredBytes[i] = (byte)(bytes[i] & 0xFF);
            }
        }

        return GlobalTools.Encoding.GetString(filteredBytes);
    }

    private static bool isSpecialCharacter(byte asciiCode)
    {
        return asciiCode >= 0 && asciiCode <= 31;
    }
}
