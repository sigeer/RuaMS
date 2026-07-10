using Application.Templates.String;

namespace Application.Templates.Reader
{
    public interface IKeyedProvider: IDisposable
    {
        string Key { get; }
        IProvider? GetSubProvider(StringCategory key);
        IEnumerable<IProvider> GetSubProviders();
    }

    public interface IStringProvider : IKeyedProvider
    {
        IEnumerable<AbstractTemplate> Search(StringCategory category, string searchText, int maxCount = 50);
    }
}
