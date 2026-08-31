using Application.Core.Channel;
using Application.Shared.Battle.Skills;
using Application.Shared.WzEntity;
using Application.Templates.Mob;
using Application.Templates.Npc;
using Application.Templates.Reader;
using Application.Templates.UI;

namespace Application.Core.Server.life;


public class LifeFactory
{
    public static LifeFactory Instance = new();

    IProvider<MobTemplate> _mobProvider;
    IProvider<NpcTemplate> _npcProvider;
    private HashSet<int> hpbarBosses;
    public LifeFactory()
    {
        hpbarBosses = ProviderSource.Instance.GetProvider<IProvider<MobWithBossHpBarTemplate>>(ProviderType.UIMobWithBossHpBar).LoadAll().Select(x => x.TemplateId).ToHashSet();
        _mobProvider = ProviderSource.Instance.GetProvider<IProvider<MobTemplate>>(ProviderType.Mob);
        _npcProvider = ProviderSource.Instance.GetProvider<IProvider<NpcTemplate>>(ProviderType.Npc);
    }

    public MonsterStats GetMonsterStats(MobTemplate mobTemplate)
    {
        MonsterStats stats = new MonsterStats();

        stats.SetMaxHP(mobTemplate.MaxHP);
        stats.setFriendly(mobTemplate.DamagedByMob);
        stats.SetMaxMP(mobTemplate.MaxMP);
        stats.setExp(mobTemplate.Exp);
        stats.setLevel(mobTemplate.Level);
        stats.setRemoveAfter(mobTemplate.RemoveAfter);
        stats.setBoss(mobTemplate.Boss);
        stats.setExplosiveReward(mobTemplate.ExplosiveReward);
        stats.setFfaLoot(mobTemplate.PublicReward > 0);
        stats.setBuffToGive(mobTemplate.Buff);
        stats.setCP(mobTemplate.GetCP);
        stats.setRemoveOnMiss(mobTemplate.RemoveOnMiss);

        if (mobTemplate.CoolDamage > 0)
        {
            stats.setCool(new(mobTemplate.CoolDamage, mobTemplate.CoolDamageProb));
        }

        if (mobTemplate.LosedItems.Length > 0)
        {
            foreach (var li in mobTemplate.LosedItems)
            {
                stats.addLoseItem(new LoseItem(li.Id, (byte)li.Prop, (byte)li.X));
            }
        }

        if (mobTemplate.SelfDestruction != null)
        {
            stats.setSelfDestruction(new SelfDestruction((byte)mobTemplate.SelfDestruction.ActionType, mobTemplate.SelfDestruction.RemoveAfter, mobTemplate.SelfDestruction.Hp));
        }


        stats.setDropPeriod(mobTemplate.DropItemPeriod * 10000);


        if (mobTemplate.Revive.Length > 0)
        {
            stats.setRevives(mobTemplate.Revive);
        }
        decodeElementalString(stats, mobTemplate.ElementStr ?? "");

        if (mobTemplate.Skill.Length > 0)
        {
            foreach (var skill in mobTemplate.Skill)
            {
                var skillId = new MobSkillId(MobSkillTypeUtils.from(skill.Skill), skill.Level);

                stats.MobSkillAnimation[skillId] = skill.EffectAfter;
            }
        }

        if (mobTemplate.NoFlip)
        {
            // Note: NoFlip handling might need origin data, assuming default
            if (mobTemplate.Stand0OriginX.HasValue)
            {
                stats.setFixedStance(mobTemplate.Stand0OriginX < 1 ? 5 : 4); // or 5 based on some logic, but simplified
            }
        }

        return stats;
    }

    public MobTemplate? getMonster(int mid)
    {
        var stringData = ClientCulture.SystemCulture.GetMobName(mid);
        if (StringConstants.WZ_MissingNo == stringData)
        {
            return null;
        }

        return _mobProvider.GetItem(mid);
    }

    public MobTemplate GetMonsterTrust(int mid) => getMonster(mid) ?? throw new BusinessResException($"MobId = {mid}");

    public int getMonsterLevel(int mid)
    {
        var s = getMonster(mid);
        if (s == null)
        {
            return -1;
        }
        return s.Level;
    }

    private static void decodeElementalString(MonsterStats stats, string elemAttr)
    {
        for (int i = 0; i < elemAttr.Length; i += 2)
        {
            stats.setEffectiveness(Element.getFromChar(elemAttr.ElementAt(i)), ElementalEffectivenessUtils.getByNumber(int.Parse(elemAttr.ElementAt(i + 1).ToString())));
        }
    }

    public NpcTemplate? getNPC(int nid) => _npcProvider.GetItem(nid);

    public NpcTemplate GetNPCTemplateTrust(int nid)
    {
        return getNPC(nid) ?? throw new BusinessResException($"NpcId = {nid}");
    }
}
