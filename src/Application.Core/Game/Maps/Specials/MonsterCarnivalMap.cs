using Application.Core.Channel;
using Application.Core.Game.Life;
using Application.Core.Scripting.Events;
using Application.Shared.Events;
using Application.Templates.Map;
using scripting.Event;
using server.maps;
using server.partyquest;
using static server.partyquest.CarnivalFactory;

namespace Application.Core.Game.Maps.Specials
{
    public interface ICPQMap : IMap, IGroupEffectEventMap, IGroupSoundEventMap, ITimeLimitedEventMap
    {
        public int MaxMobs { get; set; }
        public int MaxReactors { get; set; }
        public int DeathCP { get; set; }

        public int RewardMapWin { get; set; }
        public int RewardMapLose { get; set; }

        public int ReactorRed { get; set; }
        public int ReactorBlue { get; set; }


        public List<int> GetSkillIds();
        public void AddSkillId(int z);

        GuardianSpawnPoint? getRandomGuardianSpawn(int team);
        void AddGuardianSpawnPoint(GuardianSpawnPoint a);
        int spawnGuardian(int team, int num);
        Point getRandomSP(int team);

        void AddMobSpawn(int mobId, int spendCP);
        List<KeyValuePair<int, int>> getMobsToSpawn();

        List<MCSkill> getBlueTeamBuffs();
        List<MCSkill> getRedTeamBuffs();

        int GetInitPortal(sbyte team);
    }
    /// <summary>
    /// 0. 准备地图  1. 战场 3. 胜利地图 4. 失败地图
    /// 10. 中断离开地图
    /// </summary>
    public class MonsterCarnivalMap : MapleMap, ICPQMap
    {

        public const string DefaultEffectWin = "quest/carnival/win";
        public const string DefaultEffectLose = "quest/carnival/lose";
        public const string DefaultSoundWin = "MobCarnival/Win";
        public const string DefaultSoundLose = "MobCarnival/Lose";

        private List<int> skillIds = new();

        private List<GuardianSpawnPoint> guardianSpawns = new();
        private List<MCSkill> blueTeamBuffs = new();
        private List<MCSkill> redTeamBuffs = new();
        private List<Point> takenSpawns = new();
        private List<KeyValuePair<int, int>> mobsToSpawn = new();

        public MonsterCarnivalMap(MapTemplate mapTemplate, WorldChannel worldChannel, AbstractEventInstanceManager? eim) : base(mapTemplate, worldChannel, eim)
        {
        }

        public int MaxMobs { get; set; }
        public int MaxReactors { get; set; }
        public int DeathCP { get; set; }
        public int TimeDefault { get; set; }
        public int TimeExpand { get; set; }
        public int TimeFinish { get; set; }
        public string EffectWin { get; set; } = DefaultEffectWin;
        public string EffectLose { get; set; } = DefaultEffectLose;
        public string SoundWin { get; set; } = DefaultSoundWin;
        public string SoundLose { get; set; } = DefaultSoundLose;
        public int RewardMapWin { get; set; }
        public int RewardMapLose { get; set; }
        public int ReactorRed { get; set; }
        public int ReactorBlue { get; set; }



        public string GetDefaultSoundWin()
        {
            return DefaultSoundWin;
        }
        public string GetDefaultSoundLose()
        {
            return DefaultSoundLose;
        }

        public string GetDefaultEffectWin()
        {
            return DefaultEffectWin;
        }
        public string GetDefaultEffectLose()
        {
            return DefaultEffectLose;
        }

        public List<int> GetSkillIds()
        {
            return skillIds;
        }

        public void AddSkillId(int z)
        {
            this.skillIds.Add(z);
        }
        public List<MCSkill> getBlueTeamBuffs()
        {
            return blueTeamBuffs;
        }

        public List<MCSkill> getRedTeamBuffs()
        {
            return redTeamBuffs;
        }

        public void clearBuffList()
        {
            redTeamBuffs.Clear();
            blueTeamBuffs.Clear();
        }

        public GuardianSpawnPoint? getRandomGuardianSpawn(int team)
        {
            bool alltaken = false;
            foreach (GuardianSpawnPoint a in this.guardianSpawns)
            {
                if (!a.isTaken())
                {
                    alltaken = false;
                    break;
                }
            }
            if (alltaken)
            {
                return null;
            }
            if (this.guardianSpawns.Count > 0)
            {
                while (true)
                {
                    foreach (GuardianSpawnPoint gsp in this.guardianSpawns)
                    {
                        if (!gsp.isTaken() && Randomizer.nextDouble() < 0.3 && (gsp.getTeam() == -1 || gsp.getTeam() == team))
                        {
                            return gsp;
                        }
                    }
                }
            }
            return null;
        }

        public void AddGuardianSpawnPoint(GuardianSpawnPoint a)
        {
            this.guardianSpawns.Add(a);
        }

        public int spawnGuardian(int team, int num)
        {
            try
            {
                if (team == 0 && redTeamBuffs.Count >= 4 || team == 1 && blueTeamBuffs.Count >= 4)
                {
                    return 2;
                }
                MCSkill? skill = CarnivalFactory.getInstance().getGuardian(num);
                if (skill == null)
                    return 0;

                if (team == 0 && redTeamBuffs.Contains(skill))
                {
                    return 0;
                }
                else if (team == 1 && blueTeamBuffs.Contains(skill))
                {
                    return 0;
                }
                var pt = this.getRandomGuardianSpawn(team);
                if (pt == null)
                {
                    return -1;
                }
                int reactorID = team == TeamGroupEnum.Red ? ReactorRed : ReactorBlue;
                Reactor reactor = new Reactor(ReactorFactory.getReactorS(reactorID), reactorID);
                pt.setTaken(true);
                reactor.setPosition(pt.getPosition());
                reactor.setName(team + "" + num); //lol
                reactor.resetReactorActions(0);
                this.spawnReactor(reactor);
                reactor.setGuardian(pt);
                this.buffMonsters(team, skill);
                getReactorByOid(reactor.getObjectId())!.hitReactor(((IPlayer)this.getAllPlayers().get(0)).getClient());
            }
            catch (Exception e)
            {
                log.Error(e.ToString());
            }
            return 1;
        }

        private void buffMonsters(int team, MCSkill skill)
        {
            if (skill == null)
            {
                return;
            }

            if (team == 0)
            {
                redTeamBuffs.Add(skill);
            }
            else if (team == 1)
            {
                blueTeamBuffs.Add(skill);
            }
            foreach (IMapObject mmo in getMapObjects())
            {
                if (mmo.getType() == MapObjectType.MONSTER)
                {
                    Monster mob = (Monster)mmo;
                    if (mob.getTeam() == team)
                    {
                        skill.getSkill().applyEffect(null, mob, false, null);
                    }
                }
            }
        }

        public Point getRandomSP(int team)
        {
            if (takenSpawns.Count > 0)
            {
                foreach (var sp in monsterSpawn)
                {
                    foreach (Point pt in takenSpawns)
                    {
                        if ((sp.getPosition().X == pt.X && sp.getPosition().Y == pt.Y) || (sp.getTeam() != team && !this.isBlueCPQMap()))
                        {
                            continue;
                        }
                        else
                        {
                            takenSpawns.Add(pt);
                            return sp.getPosition();
                        }
                    }
                }
            }
            else
            {
                foreach (var sp in monsterSpawn)
                {
                    if (sp.getTeam() == team || this.isBlueCPQMap())
                    {
                        takenSpawns.Add(sp.getPosition());
                        return sp.getPosition();
                    }
                }
            }
            return Point.Empty;
        }

        public void AddMobSpawn(int mobId, int spendCP)
        {
            this.mobsToSpawn.Add(new KeyValuePair<int, int>(mobId, spendCP));
        }
        public List<KeyValuePair<int, int>> getMobsToSpawn()
        {
            return mobsToSpawn;
        }

        protected override void SetMonsterInfo(Monster monster)
        {
            if ((monster.getTeam() == 1 || monster.getTeam() == 0) && (isCPQMap() || isCPQMap2()))
            {
                List<MCSkill>? teamS = null;
                if (monster.getTeam() == 0)
                {
                    teamS = redTeamBuffs;
                }
                else if (monster.getTeam() == 1)
                {
                    teamS = blueTeamBuffs;
                }
                if (teamS != null)
                {
                    foreach (MCSkill skil in teamS)
                    {
                        if (skil != null)
                        {
                            skil.getSkill()!.applyEffect(null, monster, false, null);
                        }
                    }
                }
            }
        }

        public int GetInitPortal(sbyte team)
        {
            if (isPurpleCPQMap())
            {
                return team == 0 ? 2 : 1;
            }
            return 0;
        }
    }
}
