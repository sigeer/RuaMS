namespace Application.Templates.Providers
{
    public interface IKeyedProvider : IProvider
    {
        string Key { get; }
        IProvider? GetSubProvider(int key);
        IEnumerable<IProvider> GetSubProviders();
    }
}
