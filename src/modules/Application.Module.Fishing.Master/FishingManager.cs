using System.Collections.Concurrent;

namespace Application.Module.Fishing.Master
{
    public class FishingManager
    {
        private ConcurrentDictionary<int, int> fishingAttempters = new();


        public bool RegisterFisherPlayer(IPlayer chr, int baitLevel)
        {
            return fishingAttempters.TryAdd(chr, baitLevel);
        }

        public int UnregisterFisherPlayer(IPlayer chr)
        {
            if (fishingAttempters.TryRemove(chr, out var baitLevel))
            {
                return baitLevel;
            }
            else
            {
                return 0;
            }
        }

        public void RunCheckFishingSchedule()
        {
            double[] fishingLikelihoods = FetchFishingLikelihood();
            double yearLikelihood = fishingLikelihoods[0], timeLikelihood = fishingLikelihoods[1];

            if (fishingAttempters.Count > 0)
            {
                var fishingAttemptersList = fishingAttempters.Keys.ToList();
                foreach (var chr in fishingAttemptersList)
                {
                    int baitLevel = UnregisterFisherPlayer(chr);
                    Fishing.doFishing(chr, baitLevel, yearLikelihood, timeLikelihood);
                }
            }
        }

        public double[] FetchFishingLikelihood()
        {
            var dt = DateTimeOffset.UtcNow;
            int dayOfYear = dt.DayOfYear;

            int hours = dt.Hour;
            int minutes = dt.Minute;
            int seconds = dt.Second;

            double yearLikelihood = getFishingLikelihood(dayOfYear);
            double timeLikelihood = getFishingLikelihood(hours + minutes + seconds);

            return new double[] { yearLikelihood, timeLikelihood };
        }

        private double getFishingLikelihood(int x)
        {
            return 50.0 + 7.0 * (7.0 * Math.Sin(x)) * (Math.Cos(Math.Pow(x, 0.777)));
        }

        private void doFishing(int chrId, int baitLevel, double yearLikelihood, double timeLikelihood)
        {
            // thanks Fadi, Vcoc for suggesting a custom fishing system

            if (!chr.isLoggedinWorld() || !chr.isAlive())
            {
                return;
            }

            if (!MapId.isFishingArea(chr.getMapId()))
            {
                chr.dropMessage("You are not in a fishing area!");
                return;
            }

            if (chr.getLevel() < 30)
            {
                chr.dropMessage(5, "You must be above level 30 to fish!");
                return;
            }

            string fishingEffect;
            if (!hitFishingTime(chr, baitLevel, yearLikelihood, timeLikelihood))
            {
                fishingEffect = "Effect/BasicEff.img/Catch/Fail";
            }
            else
            {
                string rewardStr = "";
                fishingEffect = "Effect/BasicEff.img/Catch/Success";

                int rand = (int)(3.0 * Randomizer.nextDouble());
                switch (rand)
                {
                    case 0:
                        int mesoAward = (int)((1400.0 * Randomizer.nextDouble() + 1201) * chr.getMesoRate() + (15 * chr.getLevel() / 5));
                        chr.gainMeso(mesoAward, true, true, true);

                        rewardStr = mesoAward + " mesos.";
                        break;
                    case 1:
                        int expAward = (int)((645.0 * Randomizer.nextDouble() + 620.0) * chr.getExpRate() + (15 * chr.getLevel() / 4));
                        chr.gainExp(expAward, true, true);

                        rewardStr = expAward + " EXP.";
                        break;
                    case 2:
                        int itemid = getRandomItem();
                        rewardStr = "a(n) " + ItemInformationProvider.getInstance().getName(itemid) + ".";

                        if (chr.canHold(itemid))
                        {
                            chr.getAbstractPlayerInteraction().gainItem(itemid);
                        }
                        else
                        {
                            chr.showHint("Couldn't catch a(n) #r" + ItemInformationProvider.getInstance().getName(itemid) + "#k due to #e#b" + ItemConstants.getInventoryType(itemid) + "#k#n inventory limit.");
                            rewardStr += ".. but has goofed up due to full inventory.";
                        }
                        break;
                }

                chr.getMap().dropMessage(6, chr.getName() + " found " + rewardStr);
            }

            chr.sendPacket(PacketCreator.showInfo(fishingEffect));
            chr.getMap().broadcastMessage(chr, PacketCreator.showForeignInfo(chr.getId(), fishingEffect), false);
        }

        public static int getRandomItem()
        {
            int rand = (int)(100.0 * Randomizer.nextDouble());
            int[] commons = { 1002851, 2002020, 2002020, ItemId.MANA_ELIXIR, 2000018, 2002018, 2002024, 2002027, 2002027, 2000018, 2000018, 2000018, 2000018, 2002030, 2002018, 2000016 }; // filler' up
            int[] uncommons = { 1000025, 1002662, 1002812, 1002850, 1002881, 1002880, 1012072, 4020009, 2043220, 2043022, 2040543, 2044420, 2040943, 2043713, 2044220, 2044120, 2040429, 2043220, 2040943 }; // filler' uptoo 
            int[] rares = { 1002859, 1002553, 1002762, 1002763, 1002764, 1002765, 1002766, 1002663, 1002788, 1002949, 2049100, 2340000, 2040822, 2040822, 2040822, 2040822 }; // filler' uplast 

            if (rand >= 25)
            {
                return commons[(int)(commons.Length * Randomizer.nextDouble())];
            }
            else if (rand <= 7 && rand >= 4)
            {
                return uncommons[(int)(uncommons.Length * Randomizer.nextDouble())];
            }
            else
            {
                return rares[(int)(rares.Length * Randomizer.nextDouble())];
            }
        }
    }
}
