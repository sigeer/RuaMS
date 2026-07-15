namespace Application.Templates.Item.Consume
{
    [GenerateTag]
    public sealed class ScriptItemTemplate : ConsumeItemTemplate
    {
        public ScriptItemTemplate(int templateId) : base(templateId)
        {
            Script = $"item{templateId}";
        }

        /// <summary>
        /// default item{TemplateId}
        /// </summary>
        [WZPath("spec/script")]
        public string Script { get; set; }
        [WZPath("spec/npc")]
        public int Npc { get; set; }
        [WZPath("spec/runOnPickup")]
        public bool RunOnPickup { get; set; }
    }
}
