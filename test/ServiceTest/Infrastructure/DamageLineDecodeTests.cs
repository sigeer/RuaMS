using Application.Core.Game.Gameplay;

namespace ServiceTest.Infrastructure
{
    /// <summary>
    /// 攻击包伤害行的解码：兼容打了暴击位补丁的客户端与原版客户端。
    /// 二进制布局见 docs/ida/CUserRemote_OnAttack_暴击伤害编码_分析与实现.md
    /// </summary>
    public class DamageLineDecodeTests
    {
        [TestCase(0)]
        [TestCase(1)]
        [TestCase(199999)]
        [TestCase(int.MaxValue)]
        public void OriginalClient_NeverHasCritFlag(int damage)
        {
            // 原版客户端 PDamage 在 0x790586 钳制后写包，伤害恒 >= 0
            var line = DamageLine.Decode(damage);
            var (hasFlag, isCrit, value) = (line.HasCritFlag, line.IsCrit, line.Damage);

            Assert.That(hasFlag, Is.False);
            Assert.That(isCrit, Is.False);
            Assert.That(value, Is.EqualTo(damage));
        }

        [TestCase(0)]
        [TestCase(1)]
        [TestCase(199999)]
        [TestCase(int.MaxValue)]
        public void PatchedClient_CritFlagDecodes(int damage)
        {
            int raw = damage | int.MinValue;

            var line = DamageLine.Decode(raw);
            var (hasFlag, isCrit, value) = (line.HasCritFlag, line.IsCrit, line.Damage);

            Assert.That(hasFlag, Is.True);
            Assert.That(isCrit, Is.True);
            Assert.That(value, Is.EqualTo(damage));
        }

        [Test]
        public void PatchedClient_NonCritIsNonNegative()
        {
            // 补丁客户端只在暴击时置 bit31；非暴击仍是普通正值
            var line = DamageLine.Decode(12345);
            var (hasFlag, isCrit, value) = (line.HasCritFlag, line.IsCrit, line.Damage);

            Assert.That(hasFlag, Is.False);
            Assert.That(isCrit, Is.False);
            Assert.That(value, Is.EqualTo(12345));
        }

        [Test]
        public void MaxValueEdgeCase()
        {
            // 伤害恰好 int.MaxValue 且暴击：不会变成 0
            int raw = int.MaxValue | int.MinValue;   // == -1
            var line = DamageLine.Decode(raw);
            var (hasFlag, isCrit, value) = (line.HasCritFlag, line.IsCrit, line.Damage);

            Assert.That(hasFlag, Is.True);
            Assert.That(isCrit, Is.True);
            Assert.That(value, Is.EqualTo(int.MaxValue));
        }

        [Test]
        public void ZeroDamageWithCrit()
        {
            // 暴击但伤害为 0（免疫/减免到 0）可被表达
            var line = DamageLine.Decode(int.MinValue);
            var (hasFlag, isCrit, value) = (line.HasCritFlag, line.IsCrit, line.Damage);

            Assert.That(hasFlag, Is.True);
            Assert.That(isCrit, Is.True);
            Assert.That(value, Is.EqualTo(0));
        }
    }
}
