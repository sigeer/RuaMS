using Application.Templates.String;

namespace Application.Templates.Reader
{
    public interface IKeyedProvider: IDisposable
    {
        string Key { get; }
        IProvider? GetSubProvider(StringCategory key);
        IEnumerable<IProvider> GetSubProviders();
    }


    public interface IKeyedProvider<out TSubTemplate> : IKeyedProvider 
        where TSubTemplate : AbstractTemplate
    {
        new IProvider<TSubTemplate>? GetSubProvider(StringCategory key);
        new IEnumerable<IProvider<TSubTemplate>> GetSubProviders();
    }

    public interface IStringProvider : IKeyedProvider<StringTemplateBase>
    {
        IEnumerable<StringTemplateBase> Search(StringCategory category, string searchText, int maxCount = 50);
    }
}
