using Application.Core.Channel;
using Application.Core.Channel.DataProviders;
using Application.Core.Game.Life;
using Application.Core.Game.Life.Monsters;
using Application.Shared.WzEntity;
using Application.Templates.Providers;
using Application.Templates.XmlWzReader.Provider;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System.Collections.Concurrent;

namespace server.life;


public class LifeFactory : IStaticService
{
    private static Lazy<LifeFactory> _instance = new Lazy<LifeFactory>(new LifeFactory());

    public static LifeFactory Instance => _instance.Value ?? throw new BusinessFatalException("LifeFactory 未注册");

    Microsoft.Extensions.Logging.ILogger log = NullLogger.Instance;
    public void Register(IServiceProvider sp)
    {
        if (_instance != null)
            return;

        log = sp.GetRequiredService<ILogger<LifeFactory>>();
    }

    MobProvider _mobProvider = ProviderSource.Instance.GetProvider<MobProvider>();
    NpcProvider _npcProvider = ProviderSource.Instance.GetProvider<NpcProvider>();
    ConcurrentDictionary<int, MonsterStats> monsterStats = new();
    private HashSet<int> hpbarBosses;
    LifeFactory()
    {
        hpbarBosses = ProviderSource.Instance.GetProvider<MobWithBossHpBarProvider>().LoadAll().Select(x => x.TemplateId).ToHashSet();
    }

    public AbstractLifeObject? getLife(int id, string type)
    {
        if (type.Equals(LifeType.NPC, StringComparison.OrdinalIgnoreCase))
        {
            return getNPC(id);
        }
        else if (type.Equals(LifeType.Monster, StringComparison.OrdinalIgnoreCase))
        {
            return getMonster(id);
        }
        else
        {
            log.LogWarning("Unknown Life type: {LifeType}", type);
            return null;
        }
    }

    private void setMonsterAttackInfo(int mid, List<MobAttackInfoHolder> attackInfos)
    {
        if (attackInfos.Count > 0)
        {
            MonsterInformationProvider mi = MonsterInformationProvider.getInstance();

            foreach (MobAttackInfoHolder attackInfo in attackInfos)
            {
                mi.setMobAttackInfo(mid, attackInfo.attackPos, attackInfo.mpCon, attackInfo.coolTime);
                mi.setMobAttackAnimationTime(mid, attackInfo.attackPos, attackInfo.animationTime);
            }
        }
    }

    public MonsterCore? getMonsterStats(int mid)
    {
        var mobTemplate = _mobProvider.GetItem(mid);
        if (mobTemplate == null)
        {
            return null;
        }

        List<MobAttackInfoHolder> attackInfos = new();
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
        stats.setName(ClientCulture.SystemCulture.GetMobName(mid));
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
                stats.addLoseItem(new LoseItem(li.Id, (byte)li.Prob, (byte)li.X));
            }
        }

        if (mobTemplate.SelfDestructHp > 0)
        {
            stats.setSelfDestruction(new SelfDestruction((byte)mobTemplate.SelfDestructActionType, mobTemplate.SelfDestructRemoveAfter, mobTemplate.SelfDestructHp));
        }

        stats.setFirstAttack(mobTemplate.IsFirstAttack);
        stats.setDropPeriod(mobTemplate.DropItemPeriod * 10000);

        // thanks yuxaij, Riizade, Z1peR, Anesthetic for noticing some bosses crashing players due to missing requirements
        bool hpbarBoss = mobTemplate.Boss && hpbarBosses.Contains(mid);
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

        MonsterInformationProvider mi = MonsterInformationProvider.getInstance();
        if (mobTemplate.Skill.Length > 0)
        {
            HashSet<MobSkillId> skills = new();
            foreach (var skill in mobTemplate.Skill)
            {
                MobSkillType type = MobSkillTypeUtils.from(skill.Skill);
                skills.Add(new MobSkillId(type, skill.Level));

                // Note: animation time handling might need adjustment
                MobSkill mobSkill = MobSkillFactory.getMobSkillOrThrow(type, skill.Level);
                mi.setMobSkillAnimationTime(mobSkill, skill.EffectAfter);
            }
            stats.setSkills(skills);
        }

        foreach (var attack in mobTemplate.AttackInfos)
        {
            int animationTime = attack.Animations.Sum(x => x.Delay);
            attackInfos.Add(new MobAttackInfoHolder(attack.Index, attack.ConMP, attack.AttackAfter, animationTime));
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

        return new(stats, attackInfos);
    }

    public Monster? getMonster(int mid)
    {
        try
        {
            var stats = monsterStats.GetValueOrDefault(mid);
            if (stats == null)
            {
                var mobStats = getMonsterStats(mid);
                if (mobStats == null)
                    return null;

                stats = mobStats.Stats;
                setMonsterAttackInfo(mid, mobStats.AttackInfo);

                monsterStats.AddOrUpdate(mid, stats);
            }
            return new Monster(mid, stats);
        }
        catch (NullReferenceException npe)
        {
            log.LogError(npe, "[SEVERE] MOB {MobId} failed to load.", mid);
            return null;
        }
    }


    public Monster GetMonsterTrust(int mid) => getMonster(mid) ?? throw new BusinessResException($"getMonster({mid})");

    public int getMonsterLevel(int mid)
    {
        try
        {
            var stats = monsterStats.GetValueOrDefault(mid);
            if (stats == null)
            {
                var mobTemplate = _mobProvider.GetItem(mid);
                if (mobTemplate == null)
                {
                    return -1;
                }
                return mobTemplate.Level;
            }
            else
            {
                return stats.getLevel();
            }
        }
        catch (NullReferenceException npe)
        {
            log.LogError(npe, "[SEVERE] MOB {MobId} failed to load.", mid);
        }

        return -1;
    }

    private static void decodeElementalString(MonsterStats stats, string elemAttr)
    {
        for (int i = 0; i < elemAttr.Length; i += 2)
        {
            stats.setEffectiveness(Element.getFromChar(elemAttr.ElementAt(i)), ElementalEffectivenessUtils.getByNumber(int.Parse(elemAttr.ElementAt(i + 1).ToString())));
        }
    }

    public NPC? getNPC(int nid)
    {
        var npcTemplate = _npcProvider.GetItem(nid);
        if (npcTemplate != null)
            return new NPC(nid, new NPCStats(ClientCulture.SystemCulture.GetNpcName(nid), npcTemplate));

        return null;
    }
}
