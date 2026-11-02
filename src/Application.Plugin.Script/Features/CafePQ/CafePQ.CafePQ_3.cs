namespace Application.Plugin.Script.Features.CafePQ
{
    internal class CafePQ_3 : CafePQ
    {
        internal const string EventName = "CafePQ_3";

        public CafePQ_3() : base(EventName, 192000000, [192000000, 192000001], 350, 3) { }
    }
}
