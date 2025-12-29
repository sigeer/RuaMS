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


using Application.Core.Game.Life;
using tools;

namespace server.life;




/**
 * @author Danny (Leifde)
 */
public class MobAttackInfoFactory
{
    private static Dictionary<string, MobAttackInfo?> mobAttacks = new();
    private static DataProvider dataSource = DataProviderFactory.getDataProvider(WZFiles.MOB);

    public static MobAttackInfo? getMobAttackInfo(int mobId, int attack)
    {
        var ret = mobAttacks.GetValueOrDefault(mobId + "" + attack);
        if (ret != null)
        {
            return ret;
        }
        lock (mobAttacks)
        {
            ret = mobAttacks.GetValueOrDefault(mobId + "" + attack);
            if (ret == null)
            {
                var mobData = dataSource.getData(StringUtil.getLeftPaddedStr(mobId + ".img", '0', 11));
                if (mobData != null)
                {
                    //					MapleData infoData = mobData.getChildByPath("info");
                    string linkedmob = DataTool.getString("link", mobData) ?? "";
                    if (!linkedmob.Equals(""))
                    {
                        mobData = dataSource.getData(StringUtil.getLeftPaddedStr(linkedmob + ".img", '0', 11));
                    }
                    var attackData = mobData.getChildByPath("attack" + (attack + 1) + "/info");

                    if (attackData == null)
                    {
                        return null;
                    }

                    var deadlyAttack = attackData.getChildByPath("deadlyAttack");
                    int mpBurn = DataTool.getInt("mpBurn", attackData, 0);
                    int disease = DataTool.getInt("disease", attackData, 0);
                    int level = DataTool.getInt("level", attackData, 0);
                    int mpCon = DataTool.getInt("conMP", attackData, 0);
                    ret = new MobAttackInfo(mobId, attack);
                    ret.setDeadlyAttack(deadlyAttack != null);
                    ret.setMpBurn(mpBurn);
                    ret.setDiseaseSkill(disease);
                    ret.setDiseaseLevel(level);
                    ret.setMpCon(mpCon);
                }
                mobAttacks.AddOrUpdate(mobId + "" + attack, ret);
            }
            return ret;
        }
    }
}
