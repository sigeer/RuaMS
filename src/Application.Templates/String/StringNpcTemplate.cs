namespace Application.Templates.String
{
    public class StringNpcTemplate : AbstractTemplate
    {
        public string Name { get; set; }
        /// <summary>
        /// d0
        /// </summary>
        [WZPath("d0")]
        public string DefaultTalk { get; set; }
        public StringNpcTemplate(int templateId) : base(templateId)
        {
            Name = Defaults.WZ_MissingNo;
            DefaultTalk = "(...)";
        }
    }
}
