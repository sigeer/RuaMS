using Application.Utility;
using Application.Plugin.Script;
using Application.Resources.Messages;

namespace Application.Plugin.Script.Npc
{
    internal partial class NpcScript
    {
        private List<int> GetHairOptions(int[] maleHairs, int[] femaleHairs)
        {
            var result = new List<int>();
            var baseHair = getPlayer().Hair % 10;
            var hairs = getPlayer().Gender == 0 ? maleHairs : femaleHairs;
            foreach (var h in hairs)
            {
                result.Add(h + baseHair);
            }
            return result;
        }

        private List<int> GetHairColorOptions()
        {
            var result = new List<int>();
            var current = (getPlayer().Hair / 10) * 10;
            for (int i = 0; i < 8; i++)
            {
                result.Add(current + i);
            }
            return result;
        }

        private List<int> GetFaceOptions(int[] maleFaces, int[] femaleFaces)
        {
            var result = new List<int>();
            var faceBase = getPlayer().Face % 1000 - (getPlayer().Face % 100);
            var faces = getPlayer().Gender == 0 ? maleFaces : femaleFaces;
            foreach (var f in faces)
            {
                result.Add(f + faceBase);
            }
            return result;
        }

        private List<int> GetLensOptions(int[] colorOffsets)
        {
            var result = new List<int>();
            var current = getPlayer().Face % 100 + (getPlayer().Gender == 0 ? 20000 : 21000);
            foreach (var offset in colorOffsets)
            {
                result.Add(current + offset);
            }
            return result;
        }

        private List<int> GetOneTimeLensOptions()
        {
            var result = new List<int>();
            var current = getPlayer().Face % 100 + (getPlayer().Gender == 0 ? 20000 : 21000);
            for (int i = 0; i < 8; i++)
            {
                if (haveItem(5152100 + i))
                {
                    result.Add(current + 100 * i);
                }
            }
            return result;
        }

        private async Task<bool> ProcessRandomHairChange(int couponId, int[] maleHairs, int[] femaleHairs)
        {
            if (await AskYesNo(this.GetTalkMessage(ScriptMessage.Salo_HairRandom, couponId)))
            {
                if (haveItem(couponId))
                {
                    await gainItem(couponId, -1);
                    var hairOptions = GetHairOptions(maleHairs, femaleHairs);
                    var hair = Randomizer.Select(hairOptions.ToArray());
                    await setHair(hair);
                    await SayOK(this.GetTalkMessage(ScriptMessage.Salo_HairDone));
                    return true;
                }
                else
                {
                    await SayOK(this.GetTalkMessage(ScriptMessage.Salo_NoCoupon, couponId));
                }
            }
            return false;
        }

        private async Task<bool> ProcessStyleHairChange(int couponId, int[] maleHairs, int[] femaleHairs)
        {
            var hairOptions = GetHairOptions(maleHairs, femaleHairs);
            var hairIdx = await AskAvatar(this.GetTalkMessage(ScriptMessage.Salo_AskHair, couponId), hairOptions.ToArray());

            if (haveItem(couponId))
            {
                await gainItem(couponId, -1);
                await setHair(hairOptions[hairIdx]);
                await SayOK(this.GetTalkMessage(ScriptMessage.Salo_HairDone));
                return true;
            }
            else
            {
                await SayOK(this.GetTalkMessage(ScriptMessage.Salo_NoCoupon, couponId));
            }
            return false;
        }

        private async Task<bool> ProcessRandomHairColorChange(int couponId)
        {
            if (await AskYesNo(this.GetTalkMessage(ScriptMessage.Salo_HairColorRandom, couponId)))
            {
                if (haveItem(couponId))
                {
                    await gainItem(couponId, -1);
                    var hairColorOptions = GetHairColorOptions();
                    var color = Randomizer.Select(hairColorOptions.ToArray());
                    await setHair(color);
                    await SayOK(this.GetTalkMessage(ScriptMessage.Salo_HairColorDone));
                    return true;
                }
                else
                {
                    await SayOK(this.GetTalkMessage(ScriptMessage.Salo_NoCouponDye, couponId));
                }
            }
            return false;
        }

        private async Task<bool> ProcessStyleHairColorChange(int couponId)
        {
            var hairColorOptions = GetHairColorOptions();
            var colorIdx = await AskAvatar(this.GetTalkMessage(ScriptMessage.Salo_AskHairColor, couponId), hairColorOptions.ToArray());
            if (haveItem(couponId))
            {
                await gainItem(couponId, -1);
                await setHair(hairColorOptions[colorIdx]);
                await SayOK(this.GetTalkMessage(ScriptMessage.Salo_HairColorDone));
                return true;
            }
            else
            {
                await SayOK(this.GetTalkMessage(ScriptMessage.Salo_NoCouponDye, couponId));
            }
            return false;
        }

        private async Task<bool> ProcessRandomLensChange(int couponId, int[] colorOffsets)
        {
            if (await AskYesNo(this.GetTalkMessage(ScriptMessage.Salo_LensRandom, couponId)))
            {
                if (haveItem(couponId))
                {
                    await gainItem(couponId, -1);
                    var lensOptions = GetLensOptions(colorOffsets);
                    var lens = Randomizer.Select(lensOptions.ToArray());
                    await setFace(lens);
                    await SayOK(this.GetTalkMessage(ScriptMessage.Salo_LensDone));
                    return true;
                }
                else
                {
                    await SayOK(this.GetTalkMessage(ScriptMessage.Salo_NoCouponLens, couponId));
                }
            }
            return false;
        }

        private async Task<bool> ProcessStyleLensChange(int couponId, int[] colorOffsets)
        {
            var lensOptions = GetLensOptions(colorOffsets);
            var lensIdx = await AskAvatar(this.GetTalkMessage(ScriptMessage.Salo_AskLens), lensOptions.ToArray());
            if (haveItem(couponId))
            {
                await gainItem(couponId, -1);
                await setFace(lensOptions[lensIdx]);
                await SayOK(this.GetTalkMessage(ScriptMessage.Salo_LensDone));
                return true;
            }
            else
            {
                await SayOK(this.GetTalkMessage(ScriptMessage.Salo_NoCouponLens, couponId));
            }
            return false;
        }

        private async Task<bool> ProcessOneTimeLensChange(string styleMessage)
        {
            var lensOptions = GetOneTimeLensOptions();
            if (lensOptions.Count == 0)
            {
                await SayOK(this.GetTalkMessage(ScriptMessage.Salo_NoLensAtAll));
                return false;
            }
            var lensIdx = await AskAvatar(styleMessage, lensOptions.ToArray());
            var color = (lensOptions[lensIdx] / 100) % 10;
            var couponId = 5152100 + color;
            if (haveItem(couponId))
            {
                await gainItem(couponId, -1);
                await setFace(lensOptions[lensIdx]);
                await SayOK(this.GetTalkMessage(ScriptMessage.Salo_LensDone));
                return true;
            }
            else
            {
                await SayOK(this.GetTalkMessage(ScriptMessage.Salo_NoCouponLens, couponId));
            }
            return false;
        }

        private async Task<bool> ProcessRandomFaceChange(int couponId, int[] maleFaces, int[] femaleFaces)
        {
            if (await AskYesNo(this.GetTalkMessage(ScriptMessage.Salo_FaceRandom, couponId)))
            {
                if (haveItem(couponId))
                {
                    await gainItem(couponId, -1);
                    var faceOptions = GetFaceOptions(maleFaces, femaleFaces);
                    var face = Randomizer.Select(faceOptions.ToArray());
                    await setFace(face);
                    await SayOK(this.GetTalkMessage(ScriptMessage.Salo_FaceDone));
                    return true;
                }
                else
                {
                    await SayOK(this.GetTalkMessage(ScriptMessage.Salo_NoCouponFace2));
                }
            }
            return false;
        }

        private async Task<bool> ProcessStyleFaceChange(int couponId, int[] maleFaces, int[] femaleFaces)
        {
            var faceOptions = GetFaceOptions(maleFaces, femaleFaces);
            var faceIdx = await AskAvatar(this.GetTalkMessage(ScriptMessage.Salo_AskFace, couponId), faceOptions.ToArray());

            if (haveItem(couponId))
            {
                await gainItem(couponId, -1);
                await setFace(faceOptions[faceIdx]);
                await SayOK(this.GetTalkMessage(ScriptMessage.Salo_FaceDone));
                return true;
            }
            else
            {
                await SayOK(this.GetTalkMessage(ScriptMessage.Salo_SkinDone2));
            }
            return false;
        }


        private async Task<bool> ProcessSkinChange(int couponId, int[] skinOptions, string styleMessage)
        {
            var skinIdx = await AskAvatar(styleMessage, skinOptions);
            if (haveItem(couponId))
            {
                await gainItem(couponId, -1);
                await setSkin(skinOptions[skinIdx]);

                await SayOK(this.GetTalkMessage(ScriptMessage.Salo_NoSkinCoupon));
                return true;
            }
            else
            {
                await SayOK(this.GetTalkMessage(ScriptMessage.Salo_StylistIntro2, npc));
            }
            return false;
        }

        private async Task<bool> ProcessStyleHairChangeWithMultipleCoupons(int[] couponIds, int[] maleHairs, int[] femaleHairs, string styleMessage)
        {
            var hairOptions = GetHairOptions(maleHairs, femaleHairs);
            var hairIdx = await AskAvatar(styleMessage, hairOptions.ToArray());
            bool hasCoupon = false;
            int usedCouponId = -1;
            foreach (var couponId in couponIds)
            {
                if (haveItem(couponId))
                {
                    hasCoupon = true;
                    usedCouponId = couponId;
                    break;
                }
            }
            if (hasCoupon)
            {
                await gainItem(usedCouponId, -1);
                await setHair(hairOptions[hairIdx]);
                await SayOK(this.GetTalkMessage(ScriptMessage.Salo_HairSalonMenu2));
                return true;
            }
            else
            {
                await SayOK(this.GetTalkMessage(ScriptMessage.Salo_Brittany, npc));
            }
            return false;
        }

        // Npc: 1012117 
        public async Task hair_royal()
        {
            int[] mhair_r = [30010, 30070, 30080, 30090, 30100, 30690, 30760, 33000];
            int[] fhair_r = [31130, 31530, 31820, 31920, 31940, 34000, 34030];

            int[] mhair_v = [30010, 30070, 30080, 30090, 30100, 30480, 30560, 30690, 30760, 30850, 30890, 30930, 30950];
            int[] fhair_v = [31020, 31130, 31510, 31530, 31820, 31860, 31890, 31920, 31940, 31950, 34000];


            var option = await AskMenu(this.GetTalkMessage(ScriptMessage.Salo_HenesysSkin));
            switch (option)
            {
                case 0:
                    await ProcessRandomHairChange(5150040, mhair_r, fhair_r);
                    break;
                case 1:
                    await ProcessStyleHairChange(5150044, mhair_v, fhair_v);
                    break;
                default:
                    break;
            }

        }

        // Npc: 1012103 
        public async Task hair_henesys1()
        {
            int[] mhair_v = [30060, 30140, 30200, 30210, 30310, 33040, 33100];
            int[] fhair_v = [31150, 31300, 31350, 31700, 31740, 34050, 34110];

            var option = await AskMenu(this.GetTalkMessage(ScriptMessage.Salo_ShouldNotDo));
            if (option == 0)
            {
                await ProcessStyleHairChangeWithMultipleCoupons([5420002, 5150001], mhair_v, fhair_v, this.GetTalkMessage(ScriptMessage.Salo_AskHairMulti));
            }
            else if (option == 1)
            {
                await ProcessStyleHairColorChange(5151001);
            }
        }


        // Npc: 1012104 
        public async Task hair_henesys2()
        {
            int[] mhair_r = [30060, 30140, 30200, 30210, 30310, 30610, 33040, 33100];
            int[] fhair_r = [31070, 31080, 31150, 31300, 31350, 31700, 34050, 34110];

            int[] mhair_e = [30030, 30140, 30200, 30210, 30310, 30610, 33040, 33100];
            int[] fhair_e = [31070, 31150, 31300, 31350, 31430, 31700, 34050, 34110];

            var option = await AskMenu(this.GetTalkMessage(ScriptMessage.Salo_FaceRandom2));
            if (option == 0)
            {
                await ProcessRandomHairChange(5150000, mhair_r, fhair_r);
            }
            else if (option == 1)
            {
                await ProcessRandomHairChange(5150010, mhair_e, fhair_e);
            }
            else if (option == 2)
            {
                await ProcessRandomHairColorChange(5151000);
            }
        }


        // Npc: 1012105 
        public async Task skin_henesys1()
        {
            int[] skinOptions = [0, 1, 2, 3, 4];

            var option = await AskMenu(this.GetTalkMessage(ScriptMessage.Salo_LudiHairSalon));
            if (option == 0)
            {
                await ProcessSkinChange(5153000, skinOptions, this.GetTalkMessage(ScriptMessage.Salo_AskSkin));
            }
        }

        // Npc: 1052004 
        public async Task face_henesys1()
        {
            int[] mface_v = [20000, 20001, 20003, 20004, 20005, 20006, 20007, 20008, 20012, 20014, 20015, 20022, 20028, 20031];
            int[] fface_v = [21000, 21001, 21002, 21003, 21004, 21005, 21006, 21007, 21008, 21012, 21013, 21014, 21023, 21026];

            var option = await AskMenu(this.GetTalkMessage(ScriptMessage.Salo_LudiAssistant));
            if (option == 0)
            {
                await ProcessStyleFaceChange(5152001, mface_v, fface_v);
            }
        }


        // Npc: 1052005 
        public async Task face_henesys2()
        {
            int[] mface_r = [20000, 20005, 20008, 20012, 20016, 20022, 20032];
            int[] fface_r = [21000, 21002, 21008, 21014, 21020, 21024, 21029];

            var option = await AskMenu(this.GetTalkMessage(ScriptMessage.Salo_LudiPlastic));
            if (option == 0)
            {
                await ProcessRandomFaceChange(5152000, mface_r, fface_r);
            }
        }



        // Npc: 2041007 
        public async Task hair_ludi1()
        {
            int[] mhair_v = [30160, 30190, 30250, 30640, 30660, 30840, 30870, 30990];
            int[] fhair_v = [31270, 31290, 31550, 31680, 31810, 31830, 31840, 31870];

            var option = await AskMenu(this.GetTalkMessage(ScriptMessage.Salo_LudiSkin));
            if (option == 0)
            {
                await ProcessStyleHairChangeWithMultipleCoupons([5420005, 5150007], mhair_v, fhair_v, this.GetTalkMessage(ScriptMessage.Salo_AskHairMulti));
            }
            else if (option == 1)
            {
                await ProcessStyleHairColorChange(5151007);
            }
        }


        // Npc: 2041009 
        public async Task hair_ludi2()
        {
            int[] mhair_r = [30190, 30220, 30250, 30540, 30610, 30620, 30640, 30650, 30660, 30840, 30870, 30940, 30990];
            int[] fhair_r = [31170, 31270, 31290, 31510, 31540, 31550, 31600, 31640, 31680, 31810, 31830, 31840, 31870];
            int[] mhair_e = [30030, 30190, 30220, 30250, 30540, 30610, 30620, 30640, 30650, 30660, 30840, 30990];
            int[] fhair_e = [31170, 31270, 31430, 31510, 31540, 31550, 31600, 31680, 31810, 31830, 31840, 31870];

            var option = await AskMenu(this.GetTalkMessage(ScriptMessage.Salo_OrbisHairSalon));
            if (option == 0)
            {
                await ProcessRandomHairChange(5150006, mhair_r, fhair_r);
            }
            else if (option == 1)
            {
                await ProcessRandomHairChange(5150012, mhair_e, fhair_e);
            }
            else if (option == 2)
            {
                await ProcessRandomHairColorChange(5151006);
            }
        }

        // Npc: 2041010 
        public async Task face_ludi1()
        {
            int[] mface_v = [20000, 20001, 20003, 20004, 20005, 20006, 20007, 20008, 20011, 20012, 20014, 20031];
            int[] fface_v = [21000, 21001, 21002, 21003, 21004, 21005, 21006, 21007, 21008, 21010, 21012, 21014];

            var option = await AskMenu(this.GetTalkMessage(ScriptMessage.Salo_OrbisPlastic));
            if (option == 0)
            {
                await ProcessStyleFaceChange(5152007, mface_v, fface_v);
            }
        }


        // Npc: 2041013 
        public async Task skin_ludi1()
        {
            int[] skinOptions = [0, 1, 2, 3, 4];

            var option = await AskMenu(this.GetTalkMessage(ScriptMessage.Salo_OrbisAssistant, npc));
            if (option == 0)
            {
                await ProcessSkinChange(5153002, skinOptions, this.GetTalkMessage(ScriptMessage.Salo_AskSkin));
            }
        }

        // Npc: 2010001 
        public async Task hair_orbis1()
        {
            int[] mhair_v = [30230, 30260, 30280, 30340, 30490];
            int[] fhair_v = [31110, 31220, 31230, 31630, 31790];

            var option = await AskMenu(this.GetTalkMessage(ScriptMessage.Salo_OrbisSkin));
            if (option == 0)
            {
                await ProcessStyleHairChange(5150003, mhair_v, fhair_v);
            }
            else if (option == 1)
            {
                await ProcessStyleHairColorChange(5151003);
            }
        }


        // Npc: 2010002 
        public async Task face_orbis1()
        {
            int[] mface_v = [20000, 20001, 20003, 20004, 20005, 20006, 20007, 20008, 20012, 20014, 20022, 20028, 20031];
            int[] fface_v = [21000, 21001, 21002, 21003, 21004, 21005, 21006, 21007, 21008, 21012, 21014, 21023, 21026];

            var option = await AskMenu(this.GetTalkMessage(ScriptMessage.Salo_OrbisShouldNotDo));
            if (option == 0)
            {
                await ProcessStyleFaceChange(5152003, mface_v, fface_v);
            }
        }
        // Npc: 2012007 
        public async Task hair_orbis2()
        {
            int[] mhair_d = [30030, 30020, 30000, 30270, 30230];
            int[] fhair_d = [31040, 31000, 31250, 31220, 31260];
            int[] mhair_r = [30230, 30260, 30280, 30340, 30490, 30530, 30630, 30740];
            int[] fhair_r = [31110, 31220, 31230, 31630, 31650, 31710, 31790, 31890, 31930];
            int[] mhair_e = [30230, 30280, 30340, 30490, 30530, 30740];
            int[] fhair_e = [31110, 31220, 31230, 31710, 31790, 31890, 31930];

            var option = await AskMenu(this.GetTalkMessage(ScriptMessage.Salo_AriantApprentice, npc));
            if (option == 0)
            {
                await ProcessRandomHairChange(5154000, mhair_d, fhair_d);
            }
            else if (option == 1)
            {
                await ProcessRandomHairChange(5150004, mhair_r, fhair_r);
            }
            else if (option == 2)
            {
                await ProcessRandomHairChange(5150013, mhair_e, fhair_e);
            }
            else if (option == 3)
            {
                await ProcessRandomHairColorChange(5151004);
            }
        }


        // Npc: 2012008 
        public async Task skin_orbis1()
        {
            int[] skinOptions = [0, 1, 2, 3, 4];

            var option = await AskMenu(this.GetTalkMessage(ScriptMessage.Salo_AriantVip));
            if (option == 0)
            {
                await ProcessSkinChange(5153001, skinOptions, this.GetTalkMessage(ScriptMessage.Salo_AskSkin));
            }
        }


        // Npc: 2012009 
        public async Task face_orbis2()
        {
            int[] mface_r = [20003, 20011, 20021, 20022, 20023, 20027, 20031];
            int[] fface_r = [21004, 21007, 21010, 21012, 21020, 21021, 21030];

            var option = await AskMenu(this.GetTalkMessage(ScriptMessage.Salo_AriantSkin));
            if (option == 0)
            {
                await ProcessRandomFaceChange(5152004, mface_r, fface_r);
            }
        }


        // Npc: 2100005 
        public async Task hair_ariant2()
        {
            int[] mhair_r = [30150, 30170, 30180, 30320, 30330, 30410, 30460, 30680, 30800, 30820, 30900];
            int[] fhair_r = [31090, 31190, 31330, 31340, 31400, 31420, 31520, 31620, 31650, 31660, 34000];

            var option = await AskMenu(this.GetTalkMessage(ScriptMessage.Salo_T0, npc));
            if (option == 0)
            {
                await ProcessRandomHairChange(5150026, mhair_r, fhair_r);
            }
            else if (option == 1)
            {
                await ProcessRandomHairColorChange(5151021);
            }
        }


        // Npc: 2100006 
        public async Task hair_ariant1()
        {
            int[] mhair_v = [30150, 30170, 30180, 30320, 30330, 30410, 30460, 30820, 30900];
            int[] fhair_v = [31040, 31090, 31190, 31330, 31340, 31400, 31420, 31620, 31660];

            var option = await AskMenu(this.GetTalkMessage(ScriptMessage.Salo_T1));
            if (option == 0)
            {
                await ProcessStyleHairChange(5150027, mhair_v, fhair_v);
            }
            else if (option == 1)
            {
                await ProcessStyleHairColorChange(5151022);
            }
        }


        // Npc: 2100007 
        public async Task skin_ariant1()
        {
            int[] skinOptions = [0, 1, 2, 3, 4];

            await SayNext(this.GetTalkMessage(ScriptMessage.Salo_T2));
            await ProcessSkinChange(5153007, skinOptions, this.GetTalkMessage(ScriptMessage.Salo_AskSkin));
        }


        // Npc: 2100008 
        public async Task face_ariant1()
        {
            int[] mface_v = [20000, 20004, 20005, 20012, 20013, 20031];
            int[] fface_v = [21000, 21003, 21006, 21009, 21012, 21024];

            var option = await AskMenu(this.GetTalkMessage(ScriptMessage.Salo_T3));
            if (option == 0)
            {
                await ProcessStyleFaceChange(5152030, mface_v, fface_v);
            }
            else if (option == 1)
            {
                await ProcessStyleLensChange(5152047, [0, 100, 300, 600, 700]);
            }
            else if (option == 2)
            {
                await ProcessOneTimeLensChange(this.GetTalkMessage(ScriptMessage.Salo_AskLens));
            }
        }


        // Npc: 2100009 
        public async Task face_ariant2()
        {
            int[] mface_r = [20001, 20003, 20009, 20010, 20025, 20031];
            int[] fface_r = [21002, 21009, 21011, 21013, 21016, 21029, 21030];

            var option = await AskMenu(this.GetTalkMessage(ScriptMessage.Salo_T4));
            if (option == 0)
            {
                await ProcessRandomFaceChange(5152029, mface_r, fface_r);
            }
            else if (option == 1)
            {
                await ProcessRandomLensChange(5152048, [0, 100, 300, 600, 700]);
            }
        }

        // Npc: 1052100 
        public async Task hair_kerning1()
        {
            int[] mhair_v = [30040, 30130, 30780, 30850, 30860, 30920, 33040];
            int[] fhair_v = [31090, 31140, 31330, 31440, 31760, 31880, 34050];

            var option = await AskMenu(this.GetTalkMessage(ScriptMessage.Salo_T5, npc));
            if (option == 0)
            {
                await ProcessStyleHairChange(5150003, mhair_v, fhair_v);
            }
            else if (option == 1)
            {
                await ProcessStyleHairColorChange(5151003);
            }
        }


        // Npc: 1052101 
        public async Task hair_kerning2()
        {
            int[] mhair_r = [30040, 30130, 30520, 30770, 30780, 30850, 30920, 33040];
            int[] fhair_r = [31060, 31140, 31330, 31440, 31520, 31750, 31760, 31880, 34050];
            int[] mhair_e = [30130, 30430, 30520, 30770, 30780, 30850, 30920, 33040];
            int[] fhair_e = [31060, 31140, 31330, 31520, 31760, 31880, 34010, 34050];

            var option = await AskMenu(this.GetTalkMessage(ScriptMessage.Salo_T6, npc));
            if (option == 0)
            {
                await ProcessRandomHairChange(5150011, mhair_r, fhair_r);
            }
            else if (option == 1)
            {
                await ProcessRandomHairChange(5150002, mhair_e, fhair_e);
            }
            else if (option == 2)
            {
                await ProcessRandomHairColorChange(5151002);
            }
        }

        // Npc: 2090100 
        public async Task hair_mureung1()
        {
            int[] mhair_v = [30150, 30240, 30370, 30420, 30640, 30710, 30750, 30810];
            int[] fhair_v = [31140, 31160, 31180, 31300, 31460, 31470, 31660, 31910];

            var option = await AskMenu(this.GetTalkMessage(ScriptMessage.Salo_T7));
            if (option == 1)
            {
                await ProcessStyleHairChangeWithMultipleCoupons([5420006, 5150025], mhair_v, fhair_v, this.GetTalkMessage(ScriptMessage.Salo_AskHairMulti));
            }
            else if (option == 2)
            {
                await ProcessStyleHairColorChange(5151020);
            }
        }


        // Npc: 2090101 
        public async Task hair_mureung2()
        {
            int[] mhair_e = [30030, 30150, 30240, 30370, 30420, 30550, 30600, 30640, 30700, 30710, 30720, 30750, 30810, 30830];
            int[] fhair_e = [31140, 31160, 31180, 31210, 31300, 31430, 31460, 31470, 31660, 31690, 31800, 31890, 31910, 31940];

            var option = await AskMenu(this.GetTalkMessage(ScriptMessage.Salo_T8));
            if (option == 1)
            {
                await ProcessRandomHairChange(5150024, mhair_e, fhair_e);
            }
            else if (option == 2)
            {
                await ProcessRandomHairColorChange(5151019);
            }
        }


        // Npc: 2090102 
        public async Task skin_mureung1()
        {
            int[] skinOptions = [0, 1, 2, 3, 4];

            var option = await AskMenu(this.GetTalkMessage(ScriptMessage.Salo_T9));
            if (option == 2)
            {
                await ProcessSkinChange(5153006, skinOptions, this.GetTalkMessage(ScriptMessage.Salo_AskSkin));
            }
        }


        // Npc: 2090103 
        public async Task face_mureung1()
        {
            int[] mface_v = [20000, 20001, 20004, 20005, 20006, 20007, 20009, 20012, 20022, 20028, 20031];
            int[] fface_v = [21000, 21003, 21005, 21006, 21008, 21009, 21011, 21012, 21023, 21024, 21026];

            var option = await AskMenu(this.GetTalkMessage(ScriptMessage.Salo_T10, npc));
            if (option == 1)
            {
                await ProcessStyleFaceChange(5152028, mface_v, fface_v);
            }
            else if (option == 2)
            {
                await ProcessStyleLensChange(5152041, [0, 100, 300, 500, 600, 700]);
            }
            else if (option == 3)
            {
                await ProcessOneTimeLensChange(this.GetTalkMessage(ScriptMessage.Salo_AskLens));
            }
        }


        // Npc: 2090104 
        public async Task face_mureung2()
        {
            int[] mface_r = [20002, 20005, 20007, 20011, 20014, 20017, 20029];
            int[] fface_r = [21001, 21010, 21013, 21018, 21020, 21021, 21030];

            var option = await AskMenu(this.GetTalkMessage(ScriptMessage.Salo_T11, npc));
            if (option == 1)
            {
                await ProcessRandomFaceChange(5152027, mface_r, fface_r);
            }
            else if (option == 2)
            {
                await ProcessRandomLensChange(5152042, [0, 100, 300, 500, 600, 700]);
            }
        }


        // Npc: 9270023 
        public async Task face_sg2()
        {
            int[] mface_r = [20002, 20005, 20006, 20013, 20017, 20021, 20024];
            int[] fface_r = [21002, 21003, 21014, 21016, 21017, 21021, 21027];

            var option = await AskMenu(this.GetTalkMessage(ScriptMessage.Salo_T12));
            if (option == 2)
            {
                await ProcessRandomFaceChange(5152037, mface_r, fface_r);
            }
        }


        // Npc: 9270024 
        public async Task face_sg1()
        {
            int[] mface_v = [20005, 20012, 20013, 20020, 20021, 20026];
            int[] fface_v = [21006, 21009, 21011, 21012, 21021, 21025];

            var option = await AskMenu(this.GetTalkMessage(ScriptMessage.Salo_T13));
            if (option == 2)
            {
                await ProcessStyleFaceChange(5152038, mface_v, fface_v);
            }
        }


        // Npc: 9270025 
        public async Task skin_sg1()
        {
            int[] skinOptions = [0, 1, 2, 3, 4];

            var option = await AskMenu(this.GetTalkMessage(ScriptMessage.Salo_T14));
            if (option == 1)
            {
                await ProcessSkinChange(5153010, skinOptions, this.GetTalkMessage(ScriptMessage.Salo_AskSkin));
            }
        }


        // Npc: 9270026 
        public async Task lens_sg1()
        {
            var option = await AskMenu(this.GetTalkMessage(ScriptMessage.Salo_T15, npc));
            if (option == 1)
            {
                await ProcessRandomLensChange(5152039, [100, 200, 300, 400, 500, 600, 700]);
            }
            else if (option == 2)
            {
                await ProcessStyleLensChange(5152040, [200, 300, 400, 700]);
            }
            else if (option == 3)
            {
                await ProcessOneTimeLensChange(this.GetTalkMessage(ScriptMessage.Salo_AskLens));
            }
        }

        // Npc: 9270036 
        public async Task hair_sg1()
        {
            int[] mhair_v = [30000, 30020, 30110, 30120, 30270, 30290, 30310, 30670, 30840];
            int[] fhair_v = [31010, 31050, 31110, 31120, 31240, 31250, 31280, 31670, 31810];

            var option = await AskMenu(this.GetTalkMessage(ScriptMessage.Salo_T16));
            if (option == 1)
            {
                await ProcessStyleHairChange(5150033, mhair_v, fhair_v);
            }
            else if (option == 2)
            {
                await ProcessStyleHairColorChange(5151028);
            }
        }


        // Npc: 9270037 
        public async Task hair_sg2()
        {
            int[] mhair_r = [30110, 30180, 30260, 30290, 30300, 30350, 30470, 30720, 30840];
            int[] fhair_r = [31110, 31200, 31250, 31280, 31600, 31640, 31670, 31810, 34020];

            var option = await AskMenu(this.GetTalkMessage(ScriptMessage.Salo_T17));
            if (option == 1)
            {
                await ProcessRandomHairChange(5150032, mhair_r, fhair_r);
            }
            else if (option == 2)
            {
                await ProcessRandomHairColorChange(5151027);
            }
        }

        // Npc: 9201039
        public async Task hair_wedding3()
        {
            int[] mhair_q = [30270, 30240, 30020, 30000, 30132, 30192, 30032, 30112, 30162];
            int[] fhair_q = [31150, 31250, 31310, 31050, 31050, 31030, 31070, 31091, 31001];

            if (isQuestCompleted(8860) && !haveItem(4031528))
            {
                await SayNext(this.GetTalkMessage(ScriptMessage.Salo_T18));
                return;
            }

            if (!await AskYesNo(this.GetTalkMessage(ScriptMessage.Salo_T19)))
            {
                await SayNext(this.GetTalkMessage(ScriptMessage.Salo_GiveMeAMinute));
                return;
            }

            var hairs = getPlayer().Gender == 0 ? mhair_q : fhair_q;
            var hairnew = GetAvailableCosmetics(hairs);

            await SayNext(this.GetTalkMessage(ScriptMessage.Salo_HereWeGo));

            if (haveItem(4031528))
            {
                await gainItem(4031528, -1);
                await setHair(Randomizer.Select(hairnew));
                await SayNext(this.GetTalkMessage(ScriptMessage.Salo_NotBadHair));
            }
            else
            {
                await SayNext(this.GetTalkMessage(ScriptMessage.Salo_NoFreeCouponHair));
            }
        }

        // Npc: 9201069 
        public async Task NLC_FaceVip()
        {
            int[] mface_v = [20000, 20001, 20003, 20004, 20005, 20006, 20008, 20012, 20031];
            int[] fface_v = [20000, 20001, 20003, 20004, 20005, 20006, 20008, 20012, 20031];

            var option = await AskMenu(this.GetTalkMessage(ScriptMessage.Salo_T23));
            if (option == 2)
            {
                await ProcessStyleFaceChange(5152034, mface_v, fface_v);
            }
        }


        // Npc: 9201070 
        public async Task NLC_FaceExp()
        {
            int[] mface_r = [20001, 20008, 20011, 20013, 20024, 20029, 20032];
            int[] fface_r = [21000, 21007, 21011, 21012, 21017, 21020, 21022];

            var option = await AskMenu(this.GetTalkMessage(ScriptMessage.Salo_T24));
            if (option == 2)
            {
                await ProcessRandomFaceChange(5152033, mface_r, fface_r);
            }
        }


        // Npc: 9201061 
        public async Task NLC_LensExp()
        {
            var option = await AskMenu(this.GetTalkMessage(ScriptMessage.Salo_T25, npc));
            if (option == 2)
            {
                await ProcessRandomLensChange(5152035, [100, 200, 300, 400, 500, 600, 700]);
            }
        }


        // Npc: 9201062 
        public async Task NLC_LensVip()
        {
            var option = await AskMenu(this.GetTalkMessage(ScriptMessage.Salo_T26));
            if (option == 2)
            {
                await ProcessStyleLensChange(5152036, [100, 200, 300, 400, 500, 600, 700]);
            }
            else if (option == 3)
            {
                await ProcessOneTimeLensChange(this.GetTalkMessage(ScriptMessage.Salo_AskLens));
            }
        }


        // Npc: 9201063 
        public async Task NLC_HairExp()
        {
            int[] mhair_e = [30250, 30400, 30430, 30440, 30490, 30730, 30830, 30870, 30880, 33100];
            int[] fhair_e = [31320, 31450, 31560, 31570, 31690, 31720, 31730, 31830, 34010];

            var option = await AskMenu(this.GetTalkMessage(ScriptMessage.Salo_T27, npc));
            if (option == 1)
            {
                await ProcessRandomHairChange(5150030, mhair_e, fhair_e);
            }
            else if (option == 2)
            {
                await ProcessRandomHairColorChange(5151025);
            }
        }


        // Npc: 9201064 
        public async Task NLC_HairVip()
        {
            int[] mhair_v = [30250, 30490, 30730, 30870, 30880, 33100];
            int[] fhair_v = [31320, 31450, 31560, 31730, 31830];

            var option = await AskMenu(this.GetTalkMessage(ScriptMessage.Salo_T28));
            if (option == 1)
            {
                await ProcessStyleHairChangeWithMultipleCoupons([5420001, 5150031], mhair_v, fhair_v, this.GetTalkMessage(ScriptMessage.Salo_AskHairMulti));
            }
            else if (option == 2)
            {
                await ProcessStyleHairColorChange(5151026);
            }
        }


        // Npc: 9201065 
        public async Task NLC_Skin()
        {
            int[] skin = [0, 1, 2, 3, 4];

            var option = await AskMenu(this.GetTalkMessage(ScriptMessage.Salo_T29));
            if (option == 2)
            {
                await ProcessSkinChange(5153009, skin, this.GetTalkMessage(ScriptMessage.Salo_AskSkin));
            }
        }

        // Npc: 9200100 
        public async Task lens_henesys1()
        {
            var option = await AskMenu(this.GetTalkMessage(ScriptMessage.Salo_T30, npc));
            if (option == 1)
            {
                await ProcessRandomLensChange(5152010, [0, 100, 200, 400, 600, 700]);
            }
            else if (option == 2)
            {
                await ProcessStyleLensChange(5152013, [0, 100, 200, 400, 600, 700]);
            }
            else if (option == 3)
            {
                await ProcessOneTimeLensChange(this.GetTalkMessage(ScriptMessage.Salo_AskLens));
            }
        }


        // Npc: 9200101 
        public async Task lens_orbis1()
        {
            var option = await AskMenu(this.GetTalkMessage(ScriptMessage.Salo_T31, npc));
            if (option == 1)
            {
                await ProcessRandomLensChange(5152011, [100, 300, 400, 700]);
            }
            else if (option == 2)
            {
                await ProcessStyleLensChange(5152014, [100, 300, 400, 700]);
            }
            else if (option == 3)
            {
                await ProcessOneTimeLensChange(this.GetTalkMessage(ScriptMessage.Salo_AskLens));
            }
        }


        // Npc: 9200102 
        public async Task lens_ludi1()
        {
            var option = await AskMenu(this.GetTalkMessage(ScriptMessage.Salo_T32, npc));
            if (option == 1)
            {
                await ProcessRandomLensChange(5152012, [200, 300, 400, 500, 700]);
            }
            else if (option == 2)
            {
                await ProcessStyleLensChange(5152015, [200, 300, 400, 500, 700]);
            }
            else if (option == 3)
            {
                await ProcessOneTimeLensChange(this.GetTalkMessage(ScriptMessage.Salo_AskLens));
            }
        }





        // Npc: 9201015 
        public async Task hair_wedding1()
        {
            int[] mhair_v = [30050, 30300, 30410, 30450, 30510, 30570, 30580, 30590, 30660, 30910];
            int[] fhair_v = [31150, 31220, 31260, 31310, 31420, 31480, 31490, 31580, 31590, 31610, 31630];

            var option = await AskMenu(this.GetTalkMessage(ScriptMessage.Salo_T33));
            if (option == 1)
            {
                await ProcessStyleHairChangeWithMultipleCoupons([5420000, 5150020], mhair_v, fhair_v, this.GetTalkMessage(ScriptMessage.Salo_AskHairMulti));
            }
            else if (option == 2)
            {
                await ProcessStyleHairColorChange(5151017);
            }
        }


        // Npc: 9201016 
        public async Task hair_wedding2()
        {
            int[] mhair_e = [30000, 30020, 30110, 30130, 30160, 30190, 30240, 30270, 30430];
            int[] fhair_e = [31000, 31030, 31050, 31070, 31090, 31150, 31310, 31910, 34010];

            var option = await AskMenu(this.GetTalkMessage(ScriptMessage.Salo_T34, npc));
            if (option == 1)
            {
                await ProcessRandomHairChange(5150019, mhair_e, fhair_e);
            }
            else if (option == 2)
            {
                await ProcessRandomHairColorChange(5151016);
            }
        }


        // Npc: 9201017 
        public async Task lens_wedding1()
        {
            var option = await AskMenu(this.GetTalkMessage(ScriptMessage.Salo_T35, npc));
            if (option == 1)
            {
                await ProcessRandomLensChange(5152025, [0, 100, 300, 400, 500, 700]);
            }
            else if (option == 2)
            {
                await ProcessStyleLensChange(5152026, [0, 100, 300, 400, 500, 700]);
            }
            else if (option == 3)
            {
                await ProcessOneTimeLensChange(this.GetTalkMessage(ScriptMessage.Salo_AskLens));
            }
        }


        // Npc: 9201018 
        public async Task face_wedding1()
        {
            int[] mface_v = [20000, 20001, 20003, 20004, 20005, 20006, 20007, 20008, 20018, 20019];
            int[] fface_v = [21001, 21002, 21003, 21004, 21005, 21006, 21007, 21012, 21018, 21019];

            var option = await AskMenu(this.GetTalkMessage(ScriptMessage.Salo_T36));
            if (option == 2)
            {
                await ProcessStyleFaceChange(5152022, mface_v, fface_v);
            }
        }


        // Npc: 9201019 
        public async Task face_wedding2()
        {
            int[] mface_r = [20002, 20005, 20007, 20011, 20014, 20027, 20029];
            int[] fface_r = [21001, 21005, 21007, 21017, 21018, 21020, 21022];

            var option = await AskMenu(this.GetTalkMessage(ScriptMessage.Salo_T37));
            if (option == 2)
            {
                await ProcessRandomFaceChange(5152021, mface_r, fface_r);
            }
        }

        // Npc: 9120100 
        public async Task hair_shouwa1()
        {
            int[] mhair_v = [30260, 30280, 30340, 30710, 30780, 30800, 30810, 30820, 30920];
            int[] fhair_v = [31000, 31030, 31100, 31350, 31460, 31550, 31770, 31790, 31850];

            var option = await AskMenu(this.GetTalkMessage(ScriptMessage.Salo_T38));
            if (option == 1)
            {
                await ProcessStyleHairChange(5150009, mhair_v, fhair_v);
            }
            else if (option == 2)
            {
                await ProcessStyleHairColorChange(5151009);
            }
        }


        // Npc: 9120101 
        public async Task hair_shouwa2()
        {
            int[] mhair_r = [30260, 30280, 30340, 30360, 30710, 30780, 30790, 30800, 30810, 30820, 30920];
            int[] fhair_r = [31350, 31410, 31460, 31540, 31550, 31710, 31720, 31770, 31790, 31800, 31850, 34000];

            var option = await AskMenu(this.GetTalkMessage(ScriptMessage.Salo_T39));
            if (option == 1)
            {
                await ProcessRandomHairChange(5150008, mhair_r, fhair_r);
            }
            else if (option == 2)
            {
                await ProcessRandomHairColorChange(5151008);
            }
        }


        // Npc: 9120102 
        public async Task face_shouwa1()
        {
            int[] mface_v = [20000, 20004, 20005, 20012, 20020, 20031];
            int[] fface_v = [21000, 21003, 21006, 21012, 21021, 21024];

            var option = await AskMenu(this.GetTalkMessage(ScriptMessage.Salo_T40));
            if (option == 1)
            {
                await ProcessStyleFaceChange(5152009, mface_v, fface_v);
            }
            else if (option == 2)
            {
                await ProcessStyleLensChange(5152045, [0, 100, 200, 300, 400, 500, 700]);
            }
            else if (option == 3)
            {
                await ProcessOneTimeLensChange(this.GetTalkMessage(ScriptMessage.Salo_AskLens));
            }
        }


        // Npc: 9120103 
        public async Task face_shouwa2()
        {
            int[] mface_r = [20000, 20016, 20019, 20020, 20021, 20024, 20026];
            int[] fface_r = [21000, 21002, 21009, 21016, 21022, 21025, 21027];

            var option = await AskMenu(this.GetTalkMessage(ScriptMessage.Salo_T41));
            if (option == 1)
            {
                await ProcessRandomFaceChange(5152008, mface_r, fface_r);
            }
            else if (option == 2)
            {
                await ProcessRandomLensChange(5152046, [0, 100, 200, 300, 400, 500, 700]);
            }
        }

        // Npc: 2040019 — 玩具城随机整容
        public async Task face_ludi2()
        {
            int[] mface_r = [20001, 20003, 20007, 20013, 20021, 20023, 20025];
            int[] fface_r = [21002, 21004, 21006, 21008, 21022, 21027, 21029];

            var option = await AskMenu(this.GetTalkMessage(ScriptMessage.Salo_T42));
            if (option == 2)
            {
                await ProcessRandomFaceChange(5152006, mface_r, fface_r);
            }
        }

        const int COSTUME_PRICE = 100000;

        // Npc: 9900000 — 金币付费的外观修改（仅 GM 可用）
        public async Task levelUP()
        {
            if (getPlayer().gmLevel() < 1)
            {
                await SayOK(this.GetTalkMessage(ScriptMessage.Salo_T43));
                return;
            }

            int[] skin = [0, 1, 2, 3, 4, 5, 9, 10];
            int[] hair = [30000, 30010, 30020, 30030, 30040, 30050, 30060, 30070, 30080, 30090, 30110, 30120, 30130, 30140, 30150, 30160, 30170, 30180, 30190, 30200, 30210, 30220, 30230, 30240, 30250, 30260, 30270, 30280, 30290, 30300, 30310, 30320, 30330, 30340, 30350, 30360, 30370, 30400, 30410, 30420, 30440, 30450, 30460, 30470, 30480, 30490, 30510, 30520, 30530, 30540, 30550, 30560, 30570, 30580, 30590, 30600, 30610, 30620, 30630, 30640, 30650, 30660, 30670, 30680, 30690, 30700, 30710, 30720, 30730, 30740, 30750, 30760, 30770, 30780, 30790, 30800, 30810, 30820, 30830, 30840, 30860, 30870, 30880, 30890, 30900, 30910, 30920, 30930, 30940, 30950, 30990, 33000, 33040, 33100];
            int[] fhair = [31000, 31010, 31020, 31030, 31040, 31050, 31060, 31070, 31080, 31090, 31100, 31110, 31120, 31130, 31140, 31150, 31160, 31170, 31180, 31190, 31200, 31210, 31220, 31230, 31240, 31250, 31260, 31270, 31280, 31290, 31300, 31310, 31320, 31330, 31340, 31350, 31400, 31410, 31420, 31440, 31450, 31460, 31470, 31480, 31490, 31510, 31520, 31530, 31540, 31550, 31560, 31570, 31580, 31590, 31600, 31610, 31620, 31630, 31640, 31650, 31670, 31660, 31680, 31690, 31700, 31710, 31720, 31730, 31740, 31750, 31760, 31770, 31780, 31790, 31800, 31810, 31820, 31830, 31840, 31850, 31860, 31870, 31880, 31890, 31910, 31920, 31930, 31940, 31950, 34010, 34020, 34030, 34050, 34110];
            int[] face = [20000, 20001, 20002, 20003, 20004, 20005, 20006, 20007, 20008, 20009, 20010, 20011, 20012, 20013, 20014, 20015, 20016, 20017, 20018, 20019, 20020, 20021, 20022, 20023, 20024, 20025, 20026, 20027, 20028, 20029, 20031, 20032];
            int[] fface = [21000, 21001, 21002, 21003, 21004, 21005, 21006, 21007, 21008, 21009, 21010, 21011, 21012, 21013, 21014, 21016, 21017, 21018, 21019, 21020, 21021, 21022, 21023, 21024, 21025, 21026, 21027, 21029, 21030];

            var isMale = getGender() == 0;
            var option = await AskMenu(this.GetTalkMessage(ScriptMessage.Salo_T43)
                + this.GetTalkMessage(ScriptMessage.Salo_StylistMenuSkin) + "\r\n"
                + (isMale ? this.GetTalkMessage(ScriptMessage.Salo_StylistMenuMaleHair) : this.GetTalkMessage(ScriptMessage.Salo_StylistMenuFemaleHair))
                + "\r\n" + this.GetTalkMessage(ScriptMessage.Salo_StylistMenuHairColor) + "\r\n"
                + (isMale ? this.GetTalkMessage(ScriptMessage.Salo_StylistMenuMaleEyes) : this.GetTalkMessage(ScriptMessage.Salo_StylistMenuFemaleEyes))
                + "\r\n" + this.GetTalkMessage(ScriptMessage.Salo_StylistMenuEyeColor));

            if (getMeso() <= COSTUME_PRICE)
            {
                await SayNext(this.GetTalkMessage(ScriptMessage.Salo_T43));
                return;
            }

            switch (option)
            {
                case 0:
                    await ApplyStyle(skin, setSkin, this.GetTalkMessage(ScriptMessage.Salo_PickSkin));
                    break;
                case 1:
                case 5:
                    await ApplyStyle(GetAvailableCosmetics(option == 1 ? hair : fhair), setHair, this.GetTalkMessage(ScriptMessage.Salo_PickHair));
                    break;
                case 2:
                    var baseHair = getPlayer().getHair() / 10 * 10;
                    await ApplyStyle(GetAvailableCosmetics(Enumerable.Range(baseHair, 8).ToArray()), setHair, this.GetTalkMessage(ScriptMessage.Salo_PickHairColor));
                    break;
                case 3:
                case 6:
                    await ApplyStyle(GetAvailableCosmetics(option == 3 ? face : fface), setFace, this.GetTalkMessage(ScriptMessage.Salo_PickFace));
                    break;
                case 4:
                    var baseFace = getPlayer().getFace() / 1000 * 1000 + getPlayer().getFace() % 100;
                    await ApplyStyle(GetAvailableCosmetics([.. Enumerable.Range(0, 9).Select(i => baseFace + i * 100)]), setFace, this.GetTalkMessage(ScriptMessage.Salo_PickEyeColor));
                    break;
                default:
                    break;
            }
        }

        private int[] GetAvailableCosmetics(int[] candidates)
        {
            return [.. candidates.Select(getCosmeticItem).Where(id => id != -1 && !isCosmeticEquipped(id)).Distinct()];
        }

        private async Task ApplyStyle(int[] options, Func<int, Task> apply, string message)
        {
            var idx = await AskAvatar(message, options);
            if (idx < 0)
                return;

            await apply(options[idx]);
            await gainMeso(-COSTUME_PRICE);
        }
    }
}
