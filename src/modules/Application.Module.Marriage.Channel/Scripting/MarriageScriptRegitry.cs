using Application.Scripting;

namespace Application.Module.Marriage.Channel.Scripting
{
    internal class MarriageScriptRegitry : IAddtionalRegistry
    {
        readonly WeddingManager _manager;

        public MarriageScriptRegitry(WeddingManager manager)
        {
            _manager = manager;
        }

        public void AddHostedObject(IEngine engine)
        {
            engine.AddHostedObject("WeddingManager", _manager);
        }

        public void AddHostedType(IEngine engine)
        {

        }
    }
}
