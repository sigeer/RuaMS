using Application.Core.Channel.Commands;
using Application.Core.Game.Skills;
using Application.Core.Server;
using Application.Shared.Battle.Skills;
using System;
using System.Collections.Generic;
using System.Text;
using YamlDotNet.Core;

namespace Application.Core.Game.Life.Monsters.TemporaryStat
{
    internal class MonsterVenom : MonsterPosion
    {
        public MonsterVenom(Monster mob, int fromObject, MonsterStatus buffStat, StatEffect buffSource, int value, long startTime, long expiredAt) 
            : base(mob, fromObject, buffStat, buffSource, value, startTime, expiredAt)
        {
            Value = value;
        }
    }
}
