using Application.Templates.Providers;

namespace Application.Templates.XmlWzReader.Provider
{
    public abstract class ItemProviderBase : AbstractProvider<AbstractItemTemplate>
    {
        protected ItemProviderBase(TemplateOptions options) : base(options)
        {
        }

        protected void SetItemTemplate(AbstractItemTemplate itemTemplate, string? propName, string? propValue)
        {
            if (propName == "tradeBlock")
                itemTemplate.TradeBlock = Convert.ToInt32(propValue) > 0;
            if (propName == "cash")
                itemTemplate.Cash = Convert.ToInt32(propValue) > 0;
            if (propName == "notSale")
                itemTemplate.NotSale = Convert.ToInt32(propValue) > 0;
            if (propName == "quest")
                itemTemplate.Quest = Convert.ToInt32(propValue) > 0;
            if (propName == "price")
                itemTemplate.Price = Convert.ToInt32(propValue);
            if (propName == "only")
                itemTemplate.Only = Convert.ToInt32(propValue) > 0;
            if (propName == "max")
                itemTemplate.Max = Convert.ToInt32(propValue);
            if (propName == "timeLimited")
                itemTemplate.TimeLimited = Convert.ToInt32(propValue) > 0;
            if (propName == "slotMax")
                itemTemplate.SlotMax = Convert.ToInt32(propValue);
        }
    }
}
