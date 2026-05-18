using Application.Core.Channel;
using Application.Core.Channel.DataProviders;
using Application.Core.Game.Life;
using Application.Core.Game.Life.Monsters;
using Application.Shared.WzEntity;
using Application.Templates.Npc;
using Application.Templates.Providers;
using Application.Templates.XmlWzReader.Provider;
using System.Collections.Concurrent;
using System.Security.Cryptography;

namespace server.life;


public class LifeFactory
{
    public static LifeFactory Instance = new();

    MobProvider _mobProvider;
    NpcProvider _npcProvider;
    ConcurrentDictionary<int, MonsterCore> monsterStats = new();
    private HashSet<int> hpbarBosses;
    public LifeFactory()
    {
        hpbarBosses = ProviderSource.Instance.GetProvider<MobWithBossHpBarProvider>().LoadAll().Select(x => x.TemplateId).ToHashSet();
        _mobProvider = ProviderSource.Instance.GetProvider<MobProvider>();
        _npcProvider = ProviderSource.Instance.GetProvider<NpcProvider>();
    }


    public MonsterCore? getMonsterStats(int mid)
    {
        if (monsterStats.TryGetValue(mid, out var data))
        {
            return data;
        }

        var mobTemplate = _mobProvider.GetItem(mid);
        if (mobTemplate == null)
        {
            return null;
        }


        MonsterStats stats = new MonsterStats();

        stats.setHp(mobTemplate.MaxHP);
        stats.setFriendly(mobTemplate.DamagedByMob);
        stats.setPADamage(mobTemplate.PAD);
        stats.setPDDamage(mobTemplate.PDD);
        stats.setMADamage(mobTemplate.MAD);
        stats.setMDDamage(mobTemplate.MDD);
        stats.setMp(mobTemplate.MaxMP);
        stats.setExp(mobTemplate.Exp);
        stats.setLevel(mobTemplate.Level);
        stats.setRemoveAfter(mobTemplate.RemoveAfter);
        stats.setBoss(mobTemplate.Boss);
        stats.setExplosiveReward(mobTemplate.ExplosiveReward);
        stats.setFfaLoot(mobTemplate.PublicReward > 0);
        stats.setUndead(mobTemplate.UnDead);
        stats.setName(ClientCulture.SystemCulture.GetMobName(mobTemplate.TemplateId));
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

        stats.setFirstAttack(mobTemplate.IsFirstAttack);
        stats.setDropPeriod(mobTemplate.DropItemPeriod * 10000);

        // thanks yuxaij, Riizade, Z1peR, Anesthetic for noticing some bosses crashing players due to missing requirements
        bool hpbarBoss = mobTemplate.Boss && hpbarBosses.Contains(mobTemplate.TemplateId);
        stats.setTagColor(hpbarBoss ? mobTemplate.HpTagColor : 0);
        stats.setTagBgColor(hpbarBoss ? mobTemplate.HpTagBgColor : 0);

        foreach (var ani in mobTemplate.AnimateDelay)
        {
            stats.setAnimationTime(ani.Key, ani.Value);
        }

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

        if (mobTemplate.Ban != null)
        {
            stats.setBanishInfo(new BanishInfo(mobTemplate.Ban.Map, mobTemplate.Ban.PortalName ?? "sp", mobTemplate.Ban.Message));
        }

        if (mobTemplate.NoFlip)
        {
            // Note: NoFlip handling might need origin data, assuming default
            if (mobTemplate.Stand0OriginX.HasValue)
            {
                stats.setFixedStance(mobTemplate.Stand0OriginX < 1 ? 5 : 4); // or 5 based on some logic, but simplified
            }
        }

        monsterStats[mid] = data = new(mid, stats, mobTemplate.AttackInfos);
        return data;

    }

    public MonsterCore? getMonster(int mid)
    {
        var stringData = ClientCulture.SystemCulture.GetMobName(mid);
        if (StringConstants.WZ_MissingNo == stringData)
        {
            return null;
        }

        return getMonsterStats(mid);
    }

    public MonsterCore GetMonsterTrust(int mid) => getMonster(mid) ?? throw new BusinessResException($"MobId = {mid}");

    public int getMonsterLevel(int mid)
    {
        var s = getMonsterStats(mid);
        if (s == null)
        {
            return -1;
        }
        return s.Stats.getLevel();
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
