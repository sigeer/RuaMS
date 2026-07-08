using Application.Templates.Etc;
using Application.Templates.Reader;
using Application.Templates.Reader.Resolvers;
using Microsoft.Extensions.Logging;
using System.Xml.Linq;

namespace Application.Templates.Reader.Xml.Provider
{
    public sealed class CashPackageProvider : AbstractAllProvider<CashPackageTemplate>
    {
        public override ProviderType Type => ProviderType.EtcCashPackage;

        public CashPackageProvider(IWzPathResolver resolver)
            : base(resolver)
        {
        }

        protected override IEnumerable<CashPackageTemplate> GetDataFromImg()
        {
            try
            {
                List<CashPackageTemplate> all = [];
                foreach (var file in _resolver.ResolveGroup(Type))
                {
                    var fullPath = _resolver.ResolveFullPath(file);
                    var xDoc = XDocument.Load(fullPath);
                    var root = xDoc.Root!;

                    foreach (var packageNode in root.Elements())
                    {
                        if (int.TryParse(packageNode.GetName(), out var sourceId))
                        {
                            var pEntry = new CashPackageTemplate(sourceId);
                            var snList = new List<int>();
                            foreach (var child in packageNode.Elements())
                            {
                                if (child.GetName() == "SN")
                                {
                                    foreach (var value in child.Elements())
                                    {
                                        snList.Add(value.GetIntValue());
                                    }
                                }
                            }
                            pEntry.SNList = snList.ToArray();

                            all.Add(pEntry);
                            InsertItem(pEntry);
                        }
                    }
                }
                return all;
            }
            catch (Exception ex)
            {
                LibLog.Logger.LogError(ex.ToString());
                return [];
            }
        }
    }
}
