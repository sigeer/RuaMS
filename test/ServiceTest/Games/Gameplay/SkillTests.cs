using Application.Core.Game.Skills;
using Application.Shared.GameProps;
using Application.Utility.Configs;

namespace ServiceTest.Games.Gameplay
{
    internal class SkillTests
    {
        [Test]
        public async Task BuffTest1()
        {
            int darkSight = 4001003;
            int haste = 4101004;

            var chr = (await GameTestGlobal.TestServer.GetPlayer())!;
            var hasteEffect = SkillFactory.GetSkillTrust(haste).getEffect(20);
            await hasteEffect.applyTo(chr);
            Assert.That(chr.ActiveEffects.GetValueOrDefault(BuffStat.SPEED)!.Value, Is.EqualTo(hasteEffect.Statups[BuffStat.SPEED]));
            Assert.That(chr.ActiveEffects.GetValueOrDefault(BuffStat.JUMP)!.Value, Is.EqualTo(hasteEffect.Statups[BuffStat.JUMP]));

            var darkSightEffect = SkillFactory.GetSkillTrust(darkSight).getEffect(20);
            await darkSightEffect.applyTo(chr);
            Assert.That(chr.ActiveEffects.ContainsKey(BuffStat.DARKSIGHT));

            await chr.CancelBuffFromSourceId(darkSight);
            Assert.That(chr.ActiveEffects.GetValueOrDefault(BuffStat.SPEED)!.Value, Is.EqualTo(hasteEffect.Statups[BuffStat.SPEED]));
            Assert.That(chr.ActiveEffects.GetValueOrDefault(BuffStat.JUMP)!.Value, Is.EqualTo(hasteEffect.Statups[BuffStat.JUMP]));

            darkSightEffect = SkillFactory.GetSkillTrust(darkSight).getEffect(10);
            await darkSightEffect.applyTo(chr);
            Assert.That(chr.ActiveEffects.GetValueOrDefault(BuffStat.SPEED)!.Value, Is.EqualTo(darkSightEffect.Statups[BuffStat.SPEED]));
            Assert.That(chr.ActiveEffects.GetValueOrDefault(BuffStat.JUMP)!.Value, Is.EqualTo(hasteEffect.Statups[BuffStat.JUMP]));

            await chr.CancelBuffFromSourceId(darkSight);
            Assert.That(chr.ActiveEffects.GetValueOrDefault(BuffStat.SPEED)!.Value, Is.EqualTo(hasteEffect.Statups[BuffStat.SPEED]));
            Assert.That(chr.ActiveEffects.GetValueOrDefault(BuffStat.JUMP)!.Value, Is.EqualTo(hasteEffect.Statups[BuffStat.JUMP]));

            Assert.Pass();
        }

        [TestCase(true)]
        [TestCase(false)]
        [Test]
        public async Task BuffOverwriteTest1(bool useBuffMostSignficant)
        {
            int haste1 = 4101004;
            var haste2 = 4201003;

            YamlConfig.config.server.USE_BUFF_MOST_SIGNIFICANT = useBuffMostSignficant;
            var chr = (await GameTestGlobal.TestServer.GetPlayer())!;

            var hasteEffect1 = SkillFactory.GetSkillTrust(haste1).getEffect(20);
            await hasteEffect1.applyTo(chr);
            Assert.That(chr.ActiveEffects.GetValueOrDefault(BuffStat.SPEED)!.Value, Is.EqualTo(hasteEffect1.Statups[BuffStat.SPEED]));
            Assert.That(chr.ActiveEffects.GetValueOrDefault(BuffStat.JUMP)!.Value, Is.EqualTo(hasteEffect1.Statups[BuffStat.JUMP]));


            var hasteEffect2 = SkillFactory.GetSkillTrust(haste2).getEffect(10);
            await hasteEffect2.applyTo(chr);
            if (YamlConfig.config.server.USE_BUFF_MOST_SIGNIFICANT)
            {
                // 不会覆盖高等级
                Assert.That(chr.ActiveEffects.GetValueOrDefault(BuffStat.SPEED)!.Value, Is.EqualTo(hasteEffect1.Statups[BuffStat.SPEED]));
                Assert.That(chr.ActiveEffects.GetValueOrDefault(BuffStat.JUMP)!.Value, Is.EqualTo(hasteEffect1.Statups[BuffStat.JUMP]));
            }
            else
            {
                Assert.That(chr.ActiveEffects.GetValueOrDefault(BuffStat.SPEED)!.Value, Is.EqualTo(hasteEffect2.Statups[BuffStat.SPEED]));
                Assert.That(chr.ActiveEffects.GetValueOrDefault(BuffStat.JUMP)!.Value, Is.EqualTo(hasteEffect2.Statups[BuffStat.JUMP]));
            }


            await chr.CancelAllBuffs();

            await hasteEffect2.applyTo(chr);
            Assert.That(chr.ActiveEffects.GetValueOrDefault(BuffStat.SPEED)!.Value, Is.EqualTo(hasteEffect2.Statups[BuffStat.SPEED]));
            Assert.That(chr.ActiveEffects.GetValueOrDefault(BuffStat.JUMP)!.Value, Is.EqualTo(hasteEffect2.Statups[BuffStat.JUMP]));

            await hasteEffect1.applyTo(chr);
            // 覆盖低等级 / 以最新buff覆盖
            Assert.That(chr.ActiveEffects.GetValueOrDefault(BuffStat.SPEED)!.Value, Is.EqualTo(hasteEffect1.Statups[BuffStat.SPEED]));
            Assert.That(chr.ActiveEffects.GetValueOrDefault(BuffStat.JUMP)!.Value, Is.EqualTo(hasteEffect1.Statups[BuffStat.JUMP]));

            Assert.Pass();
        }
    }
}
