using Application.Templates.Item.Cash;

namespace Application.Templates.Item.Pet
{
    [GenerateTag]
    public sealed class PetItemTemplate : CashItemTemplate
    {
        public override int SlotMax { get; set; } = 1;

        [WZPath("info/hungry")]
        public int Hungry { get; set; } = 1;

        [WZPath("interact/-")]
        public PetInterActData[] InterActs { get; set; }

        public PetItemTemplate(int templateId)
            : base(templateId)
        {
            InterActs = Array.Empty<PetInterActData>();
        }
    }
}
