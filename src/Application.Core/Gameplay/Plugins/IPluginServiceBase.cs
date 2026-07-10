namespace Application.Core.Gameplay.Plugins
{
    public interface IPluginServiceBase : IAsyncDisposable
    {
        Task OnMounted();
    }
}
