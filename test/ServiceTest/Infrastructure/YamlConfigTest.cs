using Application.Utility.Configs;

namespace ServiceTest.Infrastructure
{
    public class YamlConfigTest
    {
        [Test]
        public void UseEnvTest()
        {
            Environment.SetEnvironmentVariable("COSMIC_DOTNET_HOST", "1");
            Assert.That(YamlConfig.config.server.HOST == "1");
            Environment.SetEnvironmentVariable("COSMIC_DOTNET_HOST", null);
            YamlConfig.Reload();
            Assert.That(YamlConfig.config.server.HOST == "127.0.0.1");
        }
        [Test]
        public void SetValueTest()
        {
            Environment.SetEnvironmentVariable("COSMIC_DOTNET_PQ_BONUS_EXP_RATE", "1");
            Assert.That(YamlConfig.config.server.PQ_BONUS_EXP_RATE == 1);

            YamlConfig.SetValue("PQ_BONUS_EXP_RATE", "2");
            Assert.That(YamlConfig.config.server.PQ_BONUS_EXP_RATE == 2);
        }

    }

}
