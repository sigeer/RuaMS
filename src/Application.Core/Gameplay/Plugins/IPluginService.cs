using Application.Core.Game.Life;
using Application.Core.Game.Maps;

namespace Application.Core.Gameplay.Plugins
{
    public interface IPluginService : IMobListener, IMapListener
    {
    }
}
