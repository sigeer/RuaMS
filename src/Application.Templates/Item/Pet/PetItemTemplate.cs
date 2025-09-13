using Application.Templates.Item.Cash;

namespace Application.Templates.Item.Pet
{
    [GenerateTag]
    public sealed class PetItemTemplate : CashItemTemplate
    {

        [WZPath("info/hungry")]
        public int Hungry { get; set; }

        [WZPath("interact/-")]
        public PetInterActData[] InterActs { get; set; }

        public PetItemTemplate(int templateId)
            : base(templateId)
        {
            InterActs = Array.Empty<PetInterActData>();
        }
    }
}
