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

namespace Application.Core.Game.Players
{
    public interface IPlayerStats
    {
        bool assignDex(int x);
        bool assignHP(int deltaHP, int deltaAp);
        bool assignInt(int x);
        bool assignLuk(int x);
        bool assignMP(int deltaMP, int deltaAp);
        bool assignStr(int x);
        bool assignStrDexIntLuk(int? deltaStr, int? deltaDex, int? deltaInt, int? deltaLuk);
        void changeRemainingAp(int x, bool silent);
        void gainAp(int deltaAp, bool silent);
        void gainSp(int deltaSp, int skillbook, bool silent);
        int getDex();
        int getHpMpApUsed();
        int getInt();
        int getLuk();

        int getRemainingAp();
        int[] getRemainingSps();
        int getStr();
        void healHpMp();
        bool isAlive();
        int safeAddHP(int delta);
        void updateStrDexIntLuk(int x);

        void SendStats();
        void UpdateLocalStats();
        void PrintStatsUpdated();
        /// <summary>
        /// 更新属性时（Stat），都通过这个方法包裹
        /// </summary>
        /// <param name="action"></param>
        void UpdateStatsChunk(Action action);
        TOut UpdateStatsChunk<TOut>(Func<TOut> action);

        void MaxStat();
    }
}