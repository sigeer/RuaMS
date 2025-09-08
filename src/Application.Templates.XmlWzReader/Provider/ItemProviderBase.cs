using Application.Templates.Providers;
using System.Xml.Linq;

namespace Application.Templates.XmlWzReader.Provider
{
    public abstract class ItemProviderBase : AbstractProvider<AbstractItemTemplate>
    {
        protected ItemProviderBase(TemplateOptions options) : base(options)
        {
        }

        protected void SetItemTemplateInfo(AbstractItemTemplate itemTemplate, string? propName, XElement node)
        {
            if (propName == "tradeBlock")
                itemTemplate.TradeBlock = node.GetBoolValue();
            else if (propName == "cash")
                itemTemplate.Cash = node.GetBoolValue();
            else if (propName == "notSale")
                itemTemplate.NotSale = node.GetBoolValue();
            else if (propName == "quest")
                itemTemplate.Quest = node.GetBoolValue();
            else if (propName == "price")
                itemTemplate.Price = node.GetIntValue();
            else if (propName == "only")
                itemTemplate.Only = node.GetBoolValue();
            else if (propName == "max")
                itemTemplate.Max = node.GetIntValue();
            else if (propName == "timeLimited")
                itemTemplate.TimeLimited = node.GetBoolValue();
            else if (propName == "slotMax")
                itemTemplate.SlotMax = node.GetIntValue();
            else if (propName == "time")
                itemTemplate.Time = node.GetIntValue();
            else if (propName == "accountSharable")
                itemTemplate.AccountSharable = node.GetBoolValue();
            else if (propName == "tradeAvailable")
                itemTemplate.TradeAvailable = node.GetBoolValue();
            else if (propName == "unitPrice")
                itemTemplate.UnitPrice = node.GetDoubleValue();
            else if (propName == "expireOnLogout")
                itemTemplate.ExpireOnLogout = node.GetBoolValue();
            else if (propName == "timeLimited")
                itemTemplate.TimeLimited = node.GetBoolValue();
            else if (propName == "pquest")
                itemTemplate.PartyQuest = node.GetBoolValue();

            else if (propName == "replace")
            {
                var model = new ReplaceItemTemplate();
                foreach (var replaceProp in node.Elements())
                {
                    var replacePropName = replaceProp.GetName();
                    if (replacePropName == "itemid")
                        model.ItemId = replaceProp.GetIntValue();
                    else if (replacePropName == "msg")
                        model.Message = replaceProp.GetStringValue() ?? "";
                    else if (replacePropName == "period")
                        model.Period = replaceProp.GetIntValue();
                }
                itemTemplate.ReplaceItem = model;
            }
        }

        protected void SetItemTemplateSpec(AbstractItemTemplate itemTemplate, string? propName, XElement node)
        {
            if (propName == "time")
                itemTemplate.Time = node.GetIntValue();

        }
    }
}
