

using provider;
using provider;
using provider;
using provider;
using provider;
using provider;
using provider.wz;
using provider.wz;
using server.life;
using server.life;
using server.life;
using server.life.LifeFactory;
using server.life.LifeFactory;
using server.life;
using server.life;
using server.life;
using tools;
using static server.life.LifeFactory;

namespace tools.mapletools;










public class MonsterStatFetcher {
    private static DataProvider data = DataProviderFactory.getDataProvider(WZFiles.MOB);
    private static DataProvider stringDataWZ = DataProviderFactory.getDataProvider(WZFiles.STRING);
    private static Data mobStringData = stringDataWZ.getData("Mob.img");
    private static Dictionary<int, MonsterStats> monsterStats = new ();

    static Dictionary<int, MonsterStats> getAllMonsterStats() {
        DataDirectoryEntry root = data.getRoot();

        System.out.print("Parsing mob stats... ");
        foreach(DataFileEntry mFile in root.getFiles()) {
            try {
                string fileName = mFile.getName();

                //Console.WriteLine("Parsing '" + fileName + "'");
                Data monsterData = data.getData(fileName);
                if (monsterData == null) {
                    continue;
                }

                int mid = getMonsterId(fileName);

                Data monsterInfoData = monsterData.getChildByPath("info");
                MonsterStats stats = new MonsterStats();
                stats.setHp(DataTool.getIntConvert("maxHP", monsterInfoData));
                stats.setFriendly(DataTool.getIntConvert("damagedByMob", monsterInfoData, 0) == 1);
                stats.setPADamage(DataTool.getIntConvert("PADamage", monsterInfoData));
                stats.setPDDamage(DataTool.getIntConvert("PDDamage", monsterInfoData));
                stats.setMADamage(DataTool.getIntConvert("MADamage", monsterInfoData));
                stats.setMDDamage(DataTool.getIntConvert("MDDamage", monsterInfoData));
                stats.setMp(DataTool.getIntConvert("maxMP", monsterInfoData, 0));
                stats.setExp(DataTool.getIntConvert("exp", monsterInfoData, 0));
                stats.setLevel(DataTool.getIntConvert("level", monsterInfoData));
                stats.setRemoveAfter(DataTool.getIntConvert("removeAfter", monsterInfoData, 0));
                stats.setBoss(DataTool.getIntConvert("boss", monsterInfoData, 0) > 0);
                stats.setExplosiveReward(DataTool.getIntConvert("explosiveReward", monsterInfoData, 0) > 0);
                stats.setFfaLoot(DataTool.getIntConvert("publicReward", monsterInfoData, 0) > 0);
                stats.setUndead(DataTool.getIntConvert("undead", monsterInfoData, 0) > 0);
                stats.setName(DataTool.getString(mid + "/name", mobStringData) ?? "MISSINGNO");
                stats.setBuffToGive(DataTool.getIntConvert("buff", monsterInfoData, -1));
                stats.setCP(DataTool.getIntConvert("getCP", monsterInfoData, 0));
                stats.setRemoveOnMiss(DataTool.getIntConvert("removeOnMiss", monsterInfoData, 0) > 0);

                var special = monsterInfoData.getChildByPath("coolDamage");
                if (special != null) {
                    int coolDmg = DataTool.getIntConvert("coolDamage", monsterInfoData);
                    int coolProb = DataTool.getIntConvert("coolDamageProb", monsterInfoData, 0);
                    stats.setCool(new (coolDmg, coolProb));
                }
                special = monsterInfoData.getChildByPath("loseItem");
                if (special != null) {
                    foreach(Data liData in special.getChildren()) {
                        stats.addLoseItem(new loseItem(DataTool.getInt(liData.getChildByPath("id")), (byte) DataTool.getInt(liData.getChildByPath("prop")), (byte) DataTool.getInt(liData.getChildByPath("x"))));
                    }
                }
                special = monsterInfoData.getChildByPath("selfDestruction");
                if (special != null) {
                    stats.setSelfDestruction(new selfDestruction((byte) DataTool.getInt(special.getChildByPath("action")), DataTool.getIntConvert("removeAfter", special, -1), DataTool.getIntConvert("hp", special, -1)));
                }
                var firstAttackData = monsterInfoData.getChildByPath("firstAttack");
                int firstAttack = 0;
                if (firstAttackData != null) {
                    if (firstAttackData.getType() == DataType.FLOAT) {
                        firstAttack = Math.Round(DataTool.getFloat(firstAttackData));
                    } else {
                        firstAttack = DataTool.getInt(firstAttackData);
                    }
                }
                stats.setFirstAttack(firstAttack > 0);
                stats.setDropPeriod(DataTool.getIntConvert("dropItemPeriod", monsterInfoData, 0) * 10000);

                stats.setTagColor(DataTool.getIntConvert("hpTagColor", monsterInfoData, 0));
                stats.setTagBgColor(DataTool.getIntConvert("hpTagBgcolor", monsterInfoData, 0));

                foreach(Data idata in monsterData) {
                    if (!idata.getName().Equals("info")) {
                        int delay = 0;
                        foreach(Data pic in idata.getChildren()) {
                            delay += DataTool.getIntConvert("delay", pic, 0);
                        }
                        stats.setAnimationTime(idata.getName(), delay);
                    }
                }
                var reviveInfo = monsterInfoData.getChildByPath("revive");
                if (reviveInfo != null) {
                    List<int> revives = new ();
                    foreach(Data data_ in reviveInfo) {
                        revives.Add(DataTool.getInt(data_));
                    }
                    stats.setRevives(revives);
                }
                decodeElementalString(stats, DataTool.getString("elemAttr", monsterInfoData) ?? "");
                var monsterSkillData = monsterInfoData.getChildByPath("skill");
                if (monsterSkillData != null) {
                    int i = 0;
                    HashSet<MobSkillId> skills = new ();
                    while (monsterSkillData.getChildByPath(i.ToString()) != null) {
                        int skillId = DataTool.getInt(i + "/skill", monsterSkillData, 0);
                        MobSkillType type = MobSkillTypeUtils.from(skillId).orElseThrow();
                        int skillLevel = DataTool.getInt(i + "/level", monsterSkillData, 0);
                        skills.Add(new MobSkillId(type, skillLevel));
                        i++;
                    }
                    stats.setSkills(skills);
                }
                var banishData = monsterInfoData.getChildByPath("ban");
                if (banishData != null) {
                    int map = DataTool.getInt("banMap/0/field", banishData, -1);
                    string portal = DataTool.getString("banMap/0/portal", banishData) ?? "sp";
                    string msg = DataTool.getString("banMsg", banishData);
                    stats.setBanishInfo(new BanishInfo(map, portal, msg));
                }

                monsterStats.Add(mid, stats);
            } catch (NullReferenceException npe) {
                //Console.WriteLine("[SEVERE] " + mFile.getName() + " failed to load. Issue: " + npe.getMessage() + "\n\n");
            }
        }

        Console.WriteLine("Done parsing mob stats!");
        return monsterStats;
    }

    private static int getMonsterId(string fileName) {
        return int.Parse(fileName.Substring(0, 7));
    }

    private static void decodeElementalString(MonsterStats stats, string elemAttr) {
        for (int i = 0; i < elemAttr.Length; i += 2) {
            stats.setEffectiveness(Element.getFromChar(elemAttr.ElementAt(i)), ElementalEffectiveness.getByNumber(int.valueOf(string.valueOf(elemAttr.ElementAt(i + 1)))));
        }
    }

    public static void main(string[] args) {
    	Instant instantStarted = Instant.now();
    	// load mob stats from WZ
    	Dictionary<int, MonsterStats> mobStats = MonsterStatFetcher.getAllMonsterStats();
        Instant instantStopped = Instant.now();
        Duration durationBetween = Duration.between(instantStarted, instantStopped);
        Console.WriteLine("Get elapsed time in milliseconds: " + durationBetween.toMillis());
      	Console.WriteLine("Get elapsed time in seconds: " + durationBetween.toSeconds());

    }
    
}
