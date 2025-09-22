namespace Application.Templates.Providers
{
    public interface IProvider: IDisposable
    {
        ProviderType ProviderName { get; }
    }
}
