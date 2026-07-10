using Application.Templates.Etc;
using Application.Templates.Reader;
using Application.Templates.Reader.Resolvers;
using Duey.Abstractions;
using Duey.Provider.WZ.Files;
using Microsoft.Extensions.Logging;

namespace Application.Templates.Reader.Img.Provider
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
                    var rootNode = new WZImage(fullPath);

                    foreach (var item in rootNode.Children)
                    {
                        if (int.TryParse(item.Name, out var sourceId))
                        {
                            var pEntry = new CashPackageTemplate(sourceId);
                            var snList = new List<int>();
                            foreach (var snChild in item.Children)
                            {
                                if (snChild.Name == "SN")
                                {
                                    foreach (var value in snChild.Children)
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
