namespace Application.Templates.Etc
{
    [GenerateTag]
    public class MakerCharInfoTemplate: AbstractTemplate
    {
        public MakerCharInfoTemplate(): base(0)
        {
            CharMale = new MakerCharInfoItemTemplate();
            CharFemale = new MakerCharInfoItemTemplate();
            PremiumCharMale = new MakerCharInfoItemTemplate();
            PremiumCharFemale = new MakerCharInfoItemTemplate();
            OrientCharMale = new MakerCharInfoItemTemplate();
            OrientCharFemale = new MakerCharInfoItemTemplate();
        }

        [WZPath("Info/CharMale")]
        public MakerCharInfoItemTemplate CharMale { get; set; }
        [WZPath("Info/CharFemale")]
        public MakerCharInfoItemTemplate CharFemale { get; set; }
        [WZPath("PremiumCharFemale")]
        public MakerCharInfoItemTemplate PremiumCharFemale { get; set; }
        [WZPath("PremiumCharMale")]
        public MakerCharInfoItemTemplate PremiumCharMale { get; set; }
        [WZPath("OrientCharMale")]
        public MakerCharInfoItemTemplate OrientCharMale { get; set; }
        [WZPath("OrientCharFemale")]
        public MakerCharInfoItemTemplate OrientCharFemale { get; set; }
    }

    public class MakerCharInfoItemTemplate
    {
        public MakerCharInfoItemTemplate()
        {
            FaceIdArray = Array.Empty<int>();
            HairIdArray = Array.Empty<int>();
            HairColorIdArray = Array.Empty<int>();
            SkinIdArray = Array.Empty<int>();
            TopIdArray = Array.Empty<int>();
            BottomIdArray = Array.Empty<int>();
            ShoeIdArray = Array.Empty<int>();
            WeaponIdArray = Array.Empty<int>();
        }

        [WZPath("~/0/-")]
        public int[] FaceIdArray { get; set; }
        [WZPath("~/1/-")]
        public int[] HairIdArray { get; set; }
        [WZPath("~/2/-")]
        public int[] HairColorIdArray { get; set; }
        [WZPath("~/3/-")]
        public int[] SkinIdArray { get; set; }
        [WZPath("~/4/-")]
        public int[] TopIdArray { get; set; }
        [WZPath("~/5/-")]
        public int[] BottomIdArray { get; set; }
        [WZPath("~/6/-")]
        public int[] ShoeIdArray { get; set; }
        [WZPath("~/7/-")]
        public int[] WeaponIdArray { get; set; }
    }
}
