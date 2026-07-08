using Application.Templates.Exceptions;
using Application.Templates.Reader;
using Application.Templates.Reader.Resolvers;
using Application.Templates.String;
using Duey.Abstractions;
using Duey.Provider.WZ.Files;
using Microsoft.Extensions.Logging;
using System.Globalization;

namespace Application.Templates.Reader.Img.Provider
{
    public abstract class StringBaseProvider : AbstractStringProvider
    {
        protected StringBaseProvider(CultureInfo currentCulture, IWzPathResolver resolver) : base(currentCulture, resolver)
        {
        }

        protected override IEnumerable<AbstractTemplate> GetDataFromImg(string path)
        {
            try
            {
                var fullPath = _resolver.ResolveFullPath(path, _culture);
                var rootNode = new WZImage(fullPath);

                if (string.IsNullOrEmpty(rootNode.Name))
                    throw new TemplateFormatException("String.wz", path);

                StringTemplateType fileType;
                try
                {
                    fileType = Enum.Parse<StringTemplateType>(rootNode.Name.Split('.')[0], true);
                }
                catch
                {
                    throw new TemplateFormatException("String.wz", path);
                }


                return ProcessContent(fileType, rootNode);
            }
            catch (Exception ex)
            {
                LibLog.Logger.LogError(ex.ToString());
                return [];
            }
        }

        protected virtual AbstractTemplate? SetStringTemplate(IDataNode rootNode)
        {
            if (int.TryParse(rootNode.Name, out var id))
            {
                var template = new StringTemplate(id);
                foreach (var propNode in rootNode.Children)
                {
                    var infoPropName = propNode.Name;
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

        protected IEnumerable<AbstractTemplate> ProcessContent(StringTemplateType fileType, IDataNode rootNode)
        {
            if (fileType == StringTemplateType.Eqp)
            {
                foreach (var first in rootNode.Children)
                    foreach (var typeElement in first.Children)
                        foreach (var itemElement in typeElement.Children)
                        {
                            var data = SetStringTemplate(itemElement);
                            if (data != null) yield return data;
                        }
            }
            else if (fileType == StringTemplateType.Etc || fileType == StringTemplateType.Map)
            {
                foreach (var etc in rootNode.Children)
                    foreach (var itemElement in etc.Children)
                    {
                        var data = SetStringTemplate(itemElement);
                        if (data != null) yield return data;
                    }
            }
            else
            {
                foreach (var itemElement in rootNode.Children)
                {
                    var data = SetStringTemplate(itemElement);
                    if (data != null) yield return data;
                }
            }
        }
    }
}
