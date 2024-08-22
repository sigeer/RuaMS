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
using System.Text;

namespace tools;

public class StringUtil
{
    /**
     * Gets a string padded from the left to <code>length</code> by
     * <code>padchar</code>.
     *
     * @param in      The input string to be padded.
     * @param padchar The character to pad with.
     * @param length  The length to pad to.
     * @return The padded string.
     */
    public static string getLeftPaddedStr(string inValue, char padchar, int length)
    {
        return inValue.PadLeft(length, padchar);
    }

    /**
     * Gets a string padded from the right to <code>length</code> by
     * <code>padchar</code>.
     *
     * @param in      The input string to be padded.
     * @param padchar The character to pad with.
     * @param length  The length to pad to.
     * @return The padded string.
     */
    public static string getRightPaddedStr(string inValue, char padchar, int length)
    {
        return inValue.PadRight(length, padchar);
    }

    /**
     * Joins an array of strings starting from string <code>start</code> with
     * a space.
     *
     * @param arr   The array of strings to join.
     * @param start Starting from which string.
     * @return The joined strings.
     */
    public static string joinStringFrom(string[] arr, int start)
    {
        return string.Join(' ', arr, startIndex: start, arr.Length - start);
    }

    /**
     * Joins an array of strings starting from string <code>start</code> with
     * <code>sep</code> as a seperator.
     *
     * @param arr   The array of strings to join.
     * @param start Starting from which string.
     * @return The joined strings.
     */
    public static string joinStringFrom(string[] arr, int start, string sep)
    {
        return string.Join(sep, arr, startIndex: start, arr.Length - start);
    }

    /**
     * Makes an enum name human readable (fixes spaces, capitalization, etc)
     *
     * @param enumName The name of the enum to neaten up.
     * @return The human-readable enum name.
     */
    public static string makeEnumHumanReadable(string enumName)
    {
        StringBuilder builder = new StringBuilder(enumName.Length + 1);
        string[] words = enumName.Split("_");
        foreach (string word in words)
        {
            if (word.Length <= 2)
            {
                builder.Append(word); // assume that it's an abbrevation
            }
            else
            {
                builder.Append(word.ElementAt(0));
                builder.Append(word.Substring(1).ToLower());
            }
            builder.Append(' ');
        }
        return builder.ToString().Substring(0, enumName.Length);
    }

    /**
     * Counts the number of <code>chr</code>'s in <code>str</code>.
     *
     * @param str The string to check for instances of <code>chr</code>.
     * @param chr The character to check for.
     * @return The number of times <code>chr</code> occurs in <code>str</code>.
     */
    public static int countCharacters(string str, char chr)
    {
        return str.Count(x => x == chr);
    }
}