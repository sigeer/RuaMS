using Application.Module.Marriage.Channel.Net;
using Application.Scripting;

namespace Application.Module.Marriage.Channel.Scripting
{
    internal class MarriageScriptRegitry : IAddtionalRegistry
    {
        readonly WeddingManager _manager;
        readonly MarriageManager _marriageManager;
        public MarriageScriptRegitry(WeddingManager manager, MarriageManager marriageManager)
        {
            _manager = manager;
            _marriageManager = marriageManager;
        }

        public void AddHostedObject(IEngine engine)
        {
            engine.AddHostedObject("WeddingManager", _manager);
            engine.AddHostedObject("MarriageManager", _marriageManager);
            engine.AddHostedType("Wedding", typeof(WeddingPackets));
        }

        public void AddHostedType(IEngine engine)
        {

        }
    }
}
