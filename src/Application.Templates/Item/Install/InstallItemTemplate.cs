namespace Application.Templates.Item.Install
{
    public sealed class InstallItemTemplate : AbstractItemTemplate
    {
        [WZPath("info/recoveryHP")]
        public int RecoveryHP { get; set; }

        [WZPath("info/recoveryMP")]
        public int RecoveryMP { get; set; }

        [WZPath("info/reqLevel")]
        public int ReqLevel { get; set; }

        [WZPath("info/tamingMob")]
        public int TamingMob { get; set; }

        public InstallItemTemplate(int templateId)
            : base(templateId)
        { }
    }
}