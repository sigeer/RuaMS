using Application.Templates.Exceptions;
using Application.Templates.Providers;
using Application.Templates.String;
using System.Globalization;
using System.Xml;
using System.Xml.Linq;

namespace Application.Templates.XmlWzReader.Provider
{
    public abstract class StringBaseProvider : AbstractProvider<AbstractTemplate>
    {
        protected StringTemplateType[] _types;
        protected string[] _files;
        protected CultureInfo _culture;
        MultiCultureFileProvider _fileProvider;
        protected StringBaseProvider(TemplateOptions options, CultureInfo currentCulture, StringTemplateType[] types) : base(options)
        {
            _types = types;

            _culture = currentCulture;
            _fileProvider = new MultiCultureFileProvider(_dataBaseDir);
            _files = _fileProvider.ListAllFiles(x => _types.Any(y => x.EndsWith(y.ToString(), StringComparison.OrdinalIgnoreCase)));
        }

        public override ProviderType ProviderName => ProviderType.String;

        protected override IEnumerable<AbstractTemplate> LoadAllInternal()
        {
            List<AbstractTemplate> all = [];
            foreach (var file in _files)
            {
                all.AddRange(GetDataFromImg(file));
            }
            return all;
        }

        protected override AbstractTemplate? GetItemInternal(int templateId)
        {
            return LoadAll().FirstOrDefault(x => x.TemplateId == templateId);
        }


        protected override IEnumerable<AbstractTemplate> GetDataFromImg(string path)
        {
            using var fis = _fileProvider.ReadFile(path, _culture);
            if (fis == null)
                throw new ProviderNotFoundException(nameof(StringProvider), $"没有找到，路径：{path}。");

            using var reader = XmlReader.Create(fis, XmlReaderUtils.ReaderSettings);
            var xDoc = XDocument.Load(reader).Root!;

            var typeAttr = xDoc.Attribute("name")?.Value;
            if (string.IsNullOrEmpty(typeAttr))
                throw new TemplateFormatException(ProviderType.String.ToString(), path);

            StringTemplateType fileType;
            try
            {
                fileType = Enum.Parse<StringTemplateType>(typeAttr.Split('.')[0], true);
            }
            catch (Exception)
            {
                throw new TemplateFormatException(ProviderType.String.ToString(), path);
            }

            if (!_types.Contains(fileType))
                return [];

            return ProcessStringXml(fileType, xDoc);
        }


        protected virtual AbstractTemplate? SetStringTemplate(XElement rootNode)
        {
            if (int.TryParse(rootNode.GetName(), out var id))
            {
                var template = new StringTemplate(id);
                foreach (var propNode in rootNode.Elements())
                {
                    var infoPropName = propNode.GetName();
                    if (infoPropName == "name")
                        template.Name = propNode.GetStringValue() ?? "";
                    else if (infoPropName == "desc")
                        template.Description = propNode.GetStringValue();
                    else if (infoPropName == "msg")
                        template.Message = propNode.GetStringValue();
                }
                InsertItem(template);
                return template;
            }
            return null;
        }

        protected IEnumerable<AbstractTemplate> ProcessStringXml(StringTemplateType fileType, XElement rootNode)
        {
            if (fileType == StringTemplateType.Eqp)
            {
                foreach (var first in rootNode.Elements())
                {
                    foreach (var typeElement in first.Elements())
                    {
                        foreach (var itemElement in typeElement.Elements())
                        {
                            var data = SetStringTemplate(itemElement);
                            if (data != null)
                                yield return data;
                        }
                    }
                }
            }
            else if (fileType == StringTemplateType.Etc || fileType == StringTemplateType.Map)
            {
                foreach (var etc in rootNode.Elements())
                {
                    foreach (var itemElement in etc.Elements())
                    {
                        var data = SetStringTemplate(itemElement);
                        if (data != null)
                            yield return data;
                    }
                }
            }
            else
            {
                foreach (var itemElement in rootNode.Elements())
                {
                    var data = SetStringTemplate(itemElement);
                    if (data != null)
                        yield return data;
                }
            }
        }

        public virtual IEnumerable<AbstractTemplate> Search(string searchText, int maxCount = 50)
        {
            return LoadAll().OfType<StringTemplate>().Where(x => x.Name.Contains(searchText, StringComparison.OrdinalIgnoreCase)).Take(maxCount);
        }
    }
}
