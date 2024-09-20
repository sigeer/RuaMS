using client;
using client.autoban;
using constants.game;
using constants.skills;
using tools;

namespace Application.Core.Managers
{
    public class SkillManager
    {
        public static bool CanAssinSP(IPlayer player, int skillid)
        {
            if (skillid == Aran.HIDDEN_FULL_DOUBLE || skillid == Aran.HIDDEN_FULL_TRIPLE || skillid == Aran.HIDDEN_OVER_DOUBLE || skillid == Aran.HIDDEN_OVER_TRIPLE)
            {
                player.sendPacket(PacketCreator.enableActions());
                return false;
            }

            if ((!GameConstants.isPqSkillMap(player.getMapId()) && GameConstants.isPqSkill(skillid))
                || (!player.isGM() && GameConstants.isGMSkills(skillid))
                || (!GameConstants.isInJobTree(skillid, player.getJob().getId()) && !player.isGM()))
            {
                AutobanFactory.PACKET_EDIT.alert(player, "tried to packet edit in distributing sp.");
                player.Log.Warning("Chr {CharacterName} tried to use skill {SkillId} without it being in their job.", player.getName(), skillid);

                player.Client.disconnect(true, false);
                return false;
            }

            return true;
        }
        public static void ResetSkill(IPlayer player, int SPTo, int SPFrom)
        {
            var skillSPTo = SkillFactory.GetSkillTrust(SPTo);
            var skillSPFrom = SkillFactory.GetSkillTrust(SPFrom);

            if (!CanAssinSP(player, SPTo))
            {
                return;
            }

            var curLevel = player.getSkillLevel(skillSPTo);
            var curLevelSPFrom = player.getSkillLevel(skillSPFrom);
            if (curLevel < skillSPTo.getMaxLevel() && curLevelSPFrom > 0)
            {
                player.changeSkillLevel(skillSPFrom, (sbyte)(curLevelSPFrom - 1), player.getMasterLevel(skillSPFrom), -1);
                player.changeSkillLevel(skillSPTo, (sbyte)(curLevel + 1), player.getMasterLevel(skillSPTo), -1);

                // update macros, thanks to Arnah
                if ((curLevelSPFrom - 1) == 0)
                {
                    bool updated = false;
                    foreach (var macro in player.getMacros())
                    {
                        if (macro == null)
                        {
                            continue;
                        }

                        bool update = false;// cleaner?
                        if (macro.getSkill1() == SPFrom)
                        {
                            update = true;
                            macro.setSkill1(0);
                        }
                        if (macro.getSkill2() == SPFrom)
                        {
                            update = true;
                            macro.setSkill2(0);
                        }
                        if (macro.getSkill3() == SPFrom)
                        {
                            update = true;
                            macro.setSkill3(0);
                        }
                        if (update)
                        {
                            updated = true;
                            player.updateMacros(macro.getPosition(), macro);
                        }
                    }
                    if (updated)
                    {
                        player.sendMacros();
                    }
                }
            }
        }
    }
}
