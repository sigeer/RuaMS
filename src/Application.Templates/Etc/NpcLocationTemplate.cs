namespace Application.Templates.Etc
{
    public sealed class NpcLocationTemplate : AbstractTemplate
    {
        public int[] Maps { get; set; }
        public NpcLocationTemplate(int templateId) : base(templateId)
        {
            Maps = Array.Empty<int>();
        }
    }
}
