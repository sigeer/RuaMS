using Application.Templates.Providers;
using Microsoft.Extensions.Configuration;
using ServiceTest.TestUtilities;

namespace ServiceTest.Infrastructure.WZ
{
    internal abstract class WzTestBase
    {
        protected ProviderSource _providerSource;
        [OneTimeSetUp]
        public void RunBeforeAnyTests()
        {
            var configurationBuilder = new ConfigurationBuilder();
            configurationBuilder.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["BaseDir"] = TestVariable.WzPath
            });
            var configuration = configurationBuilder.Build();
            _providerSource = new ProviderSource(configuration);
            OnProviderRegistering();

            OnProviderRegistered();
        }

        /// <summary>
        /// provider 注册
        /// </summary>
        protected virtual void OnProviderRegistering()
        {
        }

        /// <summary>
        /// provider 注册完成
        /// </summary>
        protected virtual void OnProviderRegistered()
        {
        }

        [OneTimeTearDown]
        public void RunAfterAnyTests()
        {

        }

        protected virtual void RunAfterTest()
        {

        }
    }
}
