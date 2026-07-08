namespace Application.Templates.Reader
{
    /// <summary>
    /// 基础 provider，无类型信息。用于 ProviderSource 内部存储。
    /// </summary>
    public interface IProvider : IDisposable
    {
        ProviderType Type { get; }
        string GetBaseDir();
        AbstractTemplate? GetItem(int templateId);
        TTrustTemplate? GetRequiredItem<TTrustTemplate>(int templateId) where TTrustTemplate : AbstractTemplate;
        IEnumerable<AbstractTemplate> LoadAll();

        void AddRef();
        void Release();
    }

    /// <summary>
    /// 泛型 provider，返回强类型模板。消费者用此接口。
    /// </summary>
    public interface IProvider<out TTemplate> : IProvider where TTemplate : AbstractTemplate
    {
        new TTemplate? GetItem(int templateId);
        new IEnumerable<TTemplate> LoadAll();
    }
}
