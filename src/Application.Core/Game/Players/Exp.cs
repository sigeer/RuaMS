using tools;

namespace Application.Core.Game.Players
{
    public partial class Player
    {
        public AtomicInteger ExpValue { get; set; }
        public async Task gainExp(int gain)
        {
            await gainExp(gain, true, true);
        }

        public async Task gainExp(int gain, bool show, bool inChat)
        {
            await gainExp(gain, show, inChat, true);
        }

        public async Task gainExp(int gain, bool show, bool inChat, bool white)
        {
            await gainExp(gain, 0, show, inChat, white);
        }

        public async Task gainExp(int gain, int party, bool show, bool inChat, bool white)
        {
            if (gain < 0)
            {
                gain = int.MaxValue;   // integer overflow, heh.
            }

            if (party < 0)
            {
                party = int.MaxValue;  // integer overflow, heh.
            }

            int equip = (int)Math.Min((long)(gain / 10) * PendantExp, int.MaxValue);

            await gainExpInternal(gain, equip, party, show, inChat, white);
        }

        public async Task loseExp(int loss, bool show, bool inChat)
        {
            await loseExp(loss, show, inChat, true);
        }

        public async Task loseExp(int loss, bool show, bool inChat, bool white)
        {
            await gainExpInternal(-loss, 0, 0, show, inChat, white);
        }

        private async Task announceExpGain(long gain, int equip, int party, bool inChat, bool white)
        {
            gain = Math.Min(gain, int.MaxValue);
            if (gain == 0)
            {
                if (party == 0)
                {
                    return;
                }

                gain = party;
                party = 0;
                white = false;
            }

            await SendPacket(PacketCreator.getShowExpGain((int)gain, equip, party, inChat, white));
        }


        private async Task gainExpInternal(long gain, int equip, int party, bool show, bool inChat, bool white)
        {
            // need of method synchonization here detected thanks to MedicOP
            long total = Math.Max(gain + equip + party, -ExpValue.get());

            if (Level < getMaxLevel() && (allowExpGain || this.getEventInstance() != null))
            {
                long leftover = 0;
                long nextExp = ExpValue.get() + total;

                if (nextExp > int.MaxValue)
                {
                    total = int.MaxValue - ExpValue.get();
                    leftover = nextExp - int.MaxValue;
                }
                await updateSingleStat(Stat.EXP, ExpValue.addAndGet((int)total));
                totalExpGained += total;
                if (show)
                {
                    await announceExpGain(gain, equip, party, inChat, white);
                }
                while (ExpValue.get() >= ExpTable.getExpNeededForLevel(Level))
                {
                    await levelUp(true);
                    if (Level == getMaxLevel())
                    {
                        setExp(0);
                        await updateSingleStat(Stat.EXP, 0);
                        break;
                    }
                }

                if (leftover > 0)
                {
                    await gainExpInternal(leftover, equip, party, false, inChat, white);
                }
                else
                {
                    LastExpGainTime = Client.CurrentServer.Node.GetCurrentTimeDateTimeOffset();

                    //if (YamlConfig.config.server.USE_EXP_GAIN_LOG)
                    //{
                    //    ExpLogRecord expLogRecord = new ExpLogRecord(
                    //        Id,
                    //        getChannelServer().WorldExpRate,
                    //        expCoupon,
                    //        totalExpGained,
                    //        ExpValue.get(),
                    //        LastExpGainTime
                    //    );
                    //    ExpLogger.putExpLogRecord(expLogRecord);
                    //}

                    totalExpGained = 0;
                }
            }


        }
    }
}
