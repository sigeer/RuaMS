using Application.Templates.Item.Cash;

namespace Application.Templates.Item.Pet
{
    [GenerateTag]
    public sealed class PetItemTemplate : CashItemTemplate
    {
        public override int SlotMax { get; set; } = 1;

        [WZPath("info/hungry")]
        public int Hungry { get; set; } = 1;
        [WZPath("info/permanent")]
        public bool Permanent { get; set; }
        [WZPath("info/life")]
        public int Life { get; set; }

        [WZPath("interact/-")]
        public PetInterActData[] InterActs
        {
            set
            {
                InterActsDict = value.ToDictionary(x => x.Id);
            }
        }
        [GenerateIgnoreProperty]
        public Dictionary<int, PetInterActData> InterActsDict { get; private set; }

        public PetItemTemplate(int templateId)
            : base(templateId)
        {
            InterActs = Array.Empty<PetInterActData>();
            InterActsDict = new Dictionary<int, PetInterActData>();
        }
    }
}
