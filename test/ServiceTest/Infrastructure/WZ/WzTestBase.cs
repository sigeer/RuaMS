using Application.Templates.Reader;
using Application.Templates.Reader.Img;
using Application.Templates.Reader.Xml;
using ServiceTest.TestUtilities;
using XmlWzReader.wz;

namespace ServiceTest.Infrastructure.WZ
{
    [TestFixture("Img", Ignore = "仓库未关联img资源")]
    [TestFixture("Xml")]
    internal abstract class WzTestBase
    {
        protected ProviderSource _providerSource = null!;
        protected string _readerType;

        public WzTestBase(string readerType)
        {
            _readerType = readerType;
        }

        [OneTimeSetUp]
        public void RunBeforeAnyTests()
        {
            WZFiles.DIRECTORY = TestVariable.XmlWzPath;

            string imgWzDir;
            if (_readerType == "Xml")
            {
                imgWzDir = TestVariable.XmlWzPath;
            }
            else
            {
                imgWzDir = TestVariable.XmlWzPath;
            }

            var file = Directory.EnumerateFiles(imgWzDir, "*", SearchOption.AllDirectories).FirstOrDefault();
            if (file == null)
            {
                throw new Application.Templates.Exceptions.DataDirNotFoundException();
            }


            if (file.EndsWith(".xml", StringComparison.OrdinalIgnoreCase))
            {
                var resolver = new ServerXmlResolver(imgWzDir);
                _providerSource = new ProviderSource(resolver);
                Application.Templates.Reader.Xml.Registor.Register(_providerSource);
            }
            else if (file.EndsWith(".img", StringComparison.OrdinalIgnoreCase))
            {
                var resolver = new ImgPathResolver(imgWzDir);
                _providerSource = new ProviderSource(resolver);
                Application.Templates.Reader.Img.Registor.Register(_providerSource);
            }
            else
            {
                throw new Application.Templates.Exceptions.DataDirNotFoundException();
            }

            ProviderSource.Instance = _providerSource;
            OnProviderRegistered();
            _providerSource.Debug();
        }

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
