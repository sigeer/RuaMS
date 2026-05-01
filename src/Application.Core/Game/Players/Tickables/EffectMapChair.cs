using Application.Core.Game.Skills;
using Application.Utility.Tickables;
using OpenTelemetry.Metrics;
using server;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Core.Game.Players.Tickables
{
    internal class EffectMapChair : BuffStatValueHolder
    {
        public EffectMapChair(Player chr, StatEffect effect, long startTime, long expiredAt, int value) : base(chr, effect, startTime, expiredAt, value)
        {
            var now = chr.getChannelServer().Node.getCurrentTime();
            var p = Player.getChairTaskIntervalRate(chr.ActualMaxHP, chr.ActualMaxMP);
            Period = p.Rate;
            Next = now + p.Rate;
        }

        public override void OnTick(long now)
        {
            if (this.IsAvailable() && YamlConfig.config.server.USE_CHAIR_EXTRAHEAL)
            {
                if (ExpiredAt <= now)
                {
                    Status = TickableStatus.Remove;
                    return;
                }

                if (Next <= now)
                {
                    _chr.ApplayChairBuff();
                    Next = now + Period;
                }

            }
        }
    }
}
