namespace Application.Templates.Etc
{
    public class CashPackageTemplate : AbstractTemplate
    {
        public long[] SNList { get; set; }

        public CashPackageTemplate(int templateId)
            : base(templateId)
        {
            SNList = new long[] { };
        }
    }
}
