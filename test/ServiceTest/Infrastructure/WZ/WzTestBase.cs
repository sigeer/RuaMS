using Application.Templates.Providers;
using ServiceTest.TestUtilities;

namespace ServiceTest.Infrastructure.WZ
{
    internal abstract class WzTestBase
    {
        [OneTimeSetUp]
        public void RunBeforeAnyTests()
        {
            ProviderFactory.Clear();
            ProviderFactory.Configure(o =>
            {
                o.DataDir = TestVariable.WzPath;
            });
            OnProviderRegistering();

            ProviderFactory.Apply();
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
