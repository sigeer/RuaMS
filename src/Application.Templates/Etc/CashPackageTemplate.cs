namespace Application.Templates.Etc
{
    public class CashPackageTemplate : AbstractTemplate
    {
        public int[] SNList { get; set; }

        public CashPackageTemplate(int templateId)
            : base(templateId)
        {
            SNList = [];
        }
    }
}
