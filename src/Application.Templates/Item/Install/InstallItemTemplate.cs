namespace Application.Templates.Item.Install
{
    [GenerateTag]
    public sealed class InstallItemTemplate : ItemTemplateBase
    {
        [WZPath("info/recoveryHP")]
        public int RecoveryHP { get; set; }

        [WZPath("info/recoveryMP")]
        public int RecoveryMP { get; set; }

        [WZPath("info/reqLevel")]
        public int ReqLevel { get; set; }


        public InstallItemTemplate(int templateId)
            : base(templateId)
        { }
    }
}