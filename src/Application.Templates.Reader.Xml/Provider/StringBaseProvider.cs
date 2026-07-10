using Application.Templates.Exceptions;
using Application.Templates.Reader;
using Application.Templates.Reader.Resolvers;
using Application.Templates.String;
using Microsoft.Extensions.Logging;
using System.Globalization;
using System.Xml;
using System.Xml.Linq;

namespace Application.Templates.Reader.Xml.Provider
{
    public abstract class StringBaseProvider : AbstractStringProvider
    {
        protected StringBaseProvider(CultureInfo currentCulture, IWzPathResolver resolver) : base(currentCulture, resolver)
        {
        }

        protected override AbstractTemplate? GetItemInternal(int templateId)
        {
            return LoadAll().FirstOrDefault(x => x.TemplateId == templateId);
        }

        protected override IEnumerable<AbstractTemplate> LoadAllInternal()
        {
            List<AbstractTemplate> all = new List<AbstractTemplate>();
            try
            {
                foreach (var file in _resolver.ResolveGroup(Type))
                {
                    all.AddRange(GetDataFromImg(file));
                }
            }
            catch (Exception ex)
            {
                LibLog.Logger.LogError(ex.ToString());
            }
            return all;
        }

        protected override IEnumerable<AbstractTemplate> GetDataFromImg(string path)
        {
            try
            {
                using var fis = File.Open(_resolver.ResolveFullPath(path, _culture), FileMode.Open, FileAccess.Read, FileShare.Read);
                using var reader = XmlReader.Create(fis, XmlReaderUtils.ReaderSettings);
                var xDoc = XDocument.Load(reader).Root!;

                var typeAttr = xDoc.Attribute("name")?.Value;
                if (string.IsNullOrEmpty(typeAttr))
                    throw new TemplateFormatException("String.wz", path);

                StringTemplateType fileType;
                try
                {
                    fileType = Enum.Parse<StringTemplateType>(typeAttr.Split('.')[0], true);
                }
                catch (Exception)
                {
                    throw new TemplateFormatException("String.wz", path);
                }

                return ProcessStringXml(fileType, xDoc);
            }
            catch (Exception ex)
            {
                LibLog.Logger.LogError(ex.ToString());
                return [];
            }
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
