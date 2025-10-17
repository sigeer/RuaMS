using Application.Templates.Etc;
using Application.Templates.Providers;
using Application.Templates.XmlWzReader.Provider;
using client.creator;
using ServiceTest.TestUtilities;

namespace ServiceTest.Infrastructure.WZ
{
    internal class EtcMakeCharInfoTests: WzTestBase
    {
        protected override void OnProviderRegistering()
        {
            ProviderFactory.ConfigureWith(o =>
            {
                o.RegisterProvider<EtcMakeCharInfoProvider>(() => new EtcMakeCharInfoProvider(new Application.Templates.TemplateOptions()));
            });
        }

        [Test]
        public void DataEquals()
        {
            var newProvider = ProviderFactory.GetProvider<EtcMakeCharInfoProvider>();

            var newData = newProvider.GetItem(0)!;

            VerifyItems(MakeCharInfoValidator.charMale, newData.CharMale);
            VerifyItems(MakeCharInfoValidator.charFemale, newData.CharFemale);
            VerifyItems(MakeCharInfoValidator.orientCharMale, newData.OrientCharMale);
            VerifyItems(MakeCharInfoValidator.orientCharFemale, newData.OrientCharFemale);
            VerifyItems(MakeCharInfoValidator.premiumCharMale, newData.PremiumCharMale);
            VerifyItems(MakeCharInfoValidator.premiumCharFemale, newData.PremiumCharFemale);
        }

        void VerifyItems(MakeCharInfo oldObj, MakerCharInfoItemTemplate newObj)
        {
            foreach (var item in newObj.FaceIdArray)
            {
                Assert.That(oldObj.verifyFaceId(item));
            }

            foreach (var item in newObj.HairIdArray)
            {
                Assert.That(oldObj.verifyHairId(item));
            }

            foreach (var item in newObj.WeaponIdArray)
            {
                Assert.That(oldObj.verifyWeaponId(item));
            }

            foreach (var item in newObj.ShoeIdArray)
            {
                Assert.That(oldObj.verifyShoeId(item));
            }

            foreach (var item in newObj.SkinIdArray)
            {
                Assert.That(oldObj.verifySkinId(item));
            }

            foreach (var item in newObj.HairColorIdArray)
            {
                Assert.That(oldObj.verifyHairColorId(item));
            }

            foreach (var item in newObj.TopIdArray)
            {
                Assert.That(oldObj.verifyTopId(item));
            }

            foreach (var item in newObj.BottomIdArray)
            {
                Assert.That(oldObj.verifyBottomId(item));
            }
        }
    }
}
