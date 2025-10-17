namespace Application.Templates.Providers
{
    public interface IProvider: IDisposable
    {
        string ProviderName { get; }

        string GetBaseDir();
    }
}
