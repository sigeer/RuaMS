using Application.Utility;

namespace Application.Plugin.Script
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
            if (await AskYesNo($"如果您使用#b#t{couponId}##k，您的发型将随机改变，并有机会获得我设计的新实验发型。您要使用#b#t{couponId}##k来真正改变您的发型吗？"))
            {
                if (haveItem(couponId))
                {
                    gainItem(couponId, -1);
                    var hairOptions = GetHairOptions(maleHairs, femaleHairs);
                    var hair = Randomizer.Select(hairOptions.ToArray());
                    setHair(hair);
                    await SayOK("享受你的新发型吧！");
                    return true;
                }
                else
                {
                    await SayOK($"嗯...看起来你没有#t{couponId}#...恐怕我不能给你理发。对不起...");
                }
            }
            return false;
        }

        private async Task<bool> ProcessStyleHairChange(int couponId, int[] maleHairs, int[] femaleHairs)
        {
            var hairOptions = GetHairOptions(maleHairs, femaleHairs);
            var hairIdx = await AskAvatar($"我完全可以改变你的发型，让它看起来好极了。你为什么不改一下呢？只需 #b#t{couponId}##k，剩下的事我来帮你处理，选你喜欢的风格吧！", hairOptions.ToArray());

            if (haveItem(couponId))
            {
                gainItem(couponId, -1);
                setHair(hairOptions[hairIdx]);
                await SayOK("享受你的新发型吧！");
                return true;
            }
            else
            {
                await SayOK($"嗯...看起来你没有#t{couponId}#...恐怕我不能给你理发。对不起...");
            }
            return false;
        }

        private async Task<bool> ProcessRandomHairColorChange(int couponId)
        {
            if (await AskYesNo($"如果你使用#t{couponId}#，你的发色将会随机改变。你还想使用#t{couponId}#吗？"))
            {
                if (haveItem(couponId))
                {
                    gainItem(couponId, -1);
                    var hairColorOptions = GetHairColorOptions();
                    var color = Randomizer.Select(hairColorOptions.ToArray());
                    setHair(color);
                    await SayOK("享受你的新发色吧！");
                    return true;
                }
                else
                {
                    await SayOK($"嗯...看起来你没有#t{couponId}#...恐怕我不能给你染发。很抱歉...");
                }
            }
            return false;
        }

        private async Task<bool> ProcessStyleHairColorChange(int couponId)
        {
            var hairColorOptions = GetHairColorOptions();
            var colorIdx = await AskAvatar($"我完全可以改变你的发色，让它看起来那么好。你为什么不改一下呢？只需要 #b#t{couponId}##k，剩下的交给我，选你喜欢的颜色吧！", hairColorOptions.ToArray());
            if (haveItem(couponId))
            {
                gainItem(couponId, -1);
                setHair(hairColorOptions[colorIdx]);
                await SayOK("享受你的新发色吧！");
                return true;
            }
            else
            {
                await SayOK($"嗯...看起来你没有#t{couponId}#...恐怕我不能给你染发。很抱歉...");
            }
            return false;
        }

        private async Task<bool> ProcessRandomLensChange(int couponId, int[] colorOffsets)
        {
            if (await AskYesNo($"如果你使用#b#t{couponId}##k，你将获得一副随机的化妆隐形眼镜。你打算使用#b#t{couponId}##k，真的改变你的眼睛吗？"))
            {
                if (haveItem(couponId))
                {
                    gainItem(couponId, -1);
                    var lensOptions = GetLensOptions(colorOffsets);
                    var lens = Randomizer.Select(lensOptions.ToArray());
                    setFace(lens);
                    await SayOK("享受你的新款和升级版的隐形眼镜吧！");
                    return true;
                }
                else
                {
                    await SayOK($"对不起，但我觉得你现在没有#t{couponId}#。恐怕我不能为你做。");
                }
            }
            return false;
        }

        private async Task<bool> ProcessStyleLensChange(int couponId, int[] colorOffsets)
        {
            var lensOptions = GetLensOptions(colorOffsets);
            var lensIdx = await AskAvatar("用我们的专业机器，您可以提前看到治疗后的自己，您想戴什么样的镜片呢？选择自己喜欢的款式吧。", lensOptions.ToArray());
            if (haveItem(couponId))
            {
                gainItem(couponId, -1);
                setFace(lensOptions[lensIdx]);
                await SayOK("享受你的新款和升级版的隐形眼镜吧！");
                return true;
            }
            else
            {
                await SayOK($"对不起，但我觉得你现在没有#t{couponId}#，恐怕我不能为你做。");
            }
            return false;
        }

        private async Task<bool> ProcessOneTimeLensChange(string styleMessage)
        {
            var lensOptions = GetOneTimeLensOptions();
            if (lensOptions.Count == 0)
            {
                await SayOK("你没有任何一次性化妆镜片可供使用。");
                return false;
            }
            var lensIdx = await AskAvatar(styleMessage, lensOptions.ToArray());
            var color = (lensOptions[lensIdx] / 100) % 10;
            var couponId = 5152100 + color;
            if (haveItem(couponId))
            {
                gainItem(couponId, -1);
                setFace(lensOptions[lensIdx]);
                await SayOK("享受你的新款和升级版的隐形眼镜吧！");
                return true;
            }
            else
            {
                await SayOK($"对不起，但我觉得你现在没有#t{couponId}#，恐怕我不能为你做。");
            }
            return false;
        }

        private async Task<bool> ProcessRandomFaceChange(int couponId, int[] maleFaces, int[] femaleFaces)
        {
            if (await AskYesNo($"如果你使用#b#t{couponId}##k，你的脸可能会变成一个随机的新样子...你还想用#b#t{couponId}##k来做吗？"))
            {
                if (haveItem(couponId))
                {
                    gainItem(couponId, -1);
                    var faceOptions = GetFaceOptions(maleFaces, femaleFaces);
                    var face = Randomizer.Select(faceOptions.ToArray());
                    setFace(face);
                    await SayOK("享受你的新面容吧！");
                    return true;
                }
                else
                {
                    await SayOK($"嗯...看起来你没有#t{couponId}#。很抱歉要说这个，但没有#t{couponId}#，你就不能进行整形手术了...");
                }
            }
            return false;
        }

        private async Task<bool> ProcessStyleFaceChange(int couponId, int[] maleFaces, int[] femaleFaces)
        {
            var faceOptions = GetFaceOptions(maleFaces, femaleFaces);
            var faceIdx = await AskAvatar($"让我看看... 我完全可以把你的脸变成新的。 你不想试试吗？ 使用 #b#t{couponId}##k, 花点时间选择你可以得到你喜欢的面孔。", faceOptions.ToArray());

            if (haveItem(couponId))
            {
                gainItem(couponId, -1);
                setFace(faceOptions[faceIdx]);
                await SayOK("享受你的新面容吧！");
                return true;
            }
            else
            {
                await SayOK($"嗯...看起来你没有#t{couponId}#。很抱歉要说这个，但没有#t{couponId}#，你就不能进行整形手术了...");
            }
            return false;
        }


        private async Task<bool> ProcessSkinChange(int couponId, int[] skinOptions, string styleMessage)
        {
            var skinIdx = await AskAvatar(styleMessage, skinOptions);
            if (haveItem(couponId))
            {
                gainItem(couponId, -1);
                setSkin(skinOptions[skinIdx]);

                await SayOK("享受你的新肤色吧！");
                return true;
            }
            else
            {
                await SayOK($"嗯...您没有#t{couponId}#。抱歉，恐怕我们不能为您服务...");
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
                gainItem(usedCouponId, -1);
                setHair(hairOptions[hairIdx]);
                await SayOK("享受你的新发型吧！");
                return true;
            }
            else
            {
                await SayOK("嗯...看起来你没有我们指定的优惠券...恐怕我不能给你理发。对不起...");
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


            var option = await AskMenu("嗨，我是#p1012117#，最迷人、最时尚的造型师。如果你正在寻找最漂亮的发型，那就不用再找了！\r\n#L0##i5150040##t5150040##l\r\n#L1##i5150044##t5150044##l");
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

            var option = await AskMenu("我是这家美发沙龙的负责人。如果你有#b#t5150001##k或者#b#t5151001##k，请让我来为你打理发型。请选择你想要的那个。\r\n#L0#理发：#i5150001##t5150001##l\r\n#L1#染发：#i5151001##t5151001##l");
            if (option == 0)
            {
                await ProcessStyleHairChangeWithMultipleCoupons([5420002, 5150001], mhair_v, fhair_v, "我完全可以改变你的发型，让它看起来好极了。你为什么不改一下呢？如果你有 #b#t5150001##k 就可以。选一个自己喜欢的吧~。");
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

            var option = await AskMenu($"我是#p{npc}#。如果你碰巧有#b#t5150000##k、#b#t5150010##k或#b#t5151000##k，那么让我来帮你换个发型怎么样？\r\n#L0#理发：#i5150000##t5150000##l\r\n#L1#理发：#i5150010##t5150010##l\r\n#L2#染发：#i5151000##t5151000##l");
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

            var option = await AskMenu("嗨，你好！欢迎来到射手村皮肤护理中心！你想要像我一样拥有紧致健康的皮肤吗？使用 #b#t5153000##k，你可以让我们来照顾剩下的事情，拥有你一直想要的肌肤~！\r\n#L0#皮肤护理：#i5153000##t5153000##l");
            if (option == 0)
            {
                await ProcessSkinChange(5153000, skinOptions, "使用我们的专业机器，您可以提前看到治疗后的自己。您想做什么样的皮肤护理？选择您喜欢的风格。");
            }
        }

        // Npc: 1052004 
        public async Task face_henesys1()
        {
            int[] mface_v = [20000, 20001, 20003, 20004, 20005, 20006, 20007, 20008, 20012, 20014, 20015, 20022, 20028, 20031];
            int[] fface_v = [21000, 21001, 21002, 21003, 21004, 21005, 21006, 21007, 21008, 21012, 21013, 21014, 21023, 21026];

            var option = await AskMenu("嗨，你好！欢迎来到射手村整形外科！你想把你的脸变成全新的样子吗？使用 #b#t5152001##k，你可以让我们来照顾剩下的事情，拥有你一直想要的脸~！\r\n#L0#整形外科：#i5152001##t5152001##l");
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

            var option = await AskMenu("嗨，我其实不应该这样做，但是用一个#b#t5152000##k，我还是会为你做。但别忘了，结果会是随机的！\r\n#L0#整形手术：#i5152000##t5152000##l");
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

            var option = await AskMenu("欢迎来到玩具城美发沙龙！你有#b#t5150007##k或者#b#t5151007##k吗？如果有的话，让我来为你打理一下头发吧？请选择你想要做的事情...\r\n#L0#理发：#i5150007##t5150007##l\r\n#L1#染发：#i5151007##t5151007##l");
            if (option == 0)
            {
                await ProcessStyleHairChangeWithMultipleCoupons([5420005, 5150007], mhair_v, fhair_v, "我可以完全改变你的发型，你还没准备好接受改变吗？给我 #b#t5150007##k，剩下的事我来帮你处理，选你喜欢的风格吧！");
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

            var option = await AskMenu("嗨，我是这里的助手。别担心，我完全能胜任这个任务。如果你碰巧有#b#t5150006##k、#b#t5150012##k或#b#t5151006##k，那就让我来处理剩下的事情，好吗？\r\n#L0#理发：#i5150006##t5150006##l\r\n#L1#理发：#i5150012##t5150012##l\r\n#L2#染发：#i5151006##t5151006##l");
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

            var option = await AskMenu("嗨，你好！欢迎来到玩具城整形外科！你想把你的脸变成全新的样子吗？使用 #b#t5152007##k，你可以让我们来照顾剩下的事情，拥有你一直想要的脸~！\r\n#L0#整形外科：#i5152007##t5152007##l");
            if (option == 0)
            {
                await ProcessStyleFaceChange(5152007, mface_v, fface_v);
            }
        }


        // Npc: 2041013 
        public async Task skin_ludi1()
        {
            int[] skinOptions = [0, 1, 2, 3, 4];

            var option = await AskMenu("哦，你好！欢迎来到玩具城美容中心！你有兴趣晒黑变性感吗？或者想要拥有美丽雪白的肌肤？如果你有#b#t5153002##k，你可以让我们来照顾剩下的事情，拥有你一直梦寐以求的肌肤！\r\n#L0#美容护肤：#i5153002##t5153002##l");
            if (option == 0)
            {
                await ProcessSkinChange(5153002, skinOptions, "通过我们的专业机器，你可以在手术前看到自己在治疗后的样子。你想要什么样的表情？快来选择你喜欢的风格吧～！");
            }
        }

        // Npc: 2010001 
        public async Task hair_orbis1()
        {
            int[] mhair_v = [30230, 30260, 30280, 30340, 30490];
            int[] fhair_v = [31110, 31220, 31230, 31630, 31790];

            var option = await AskMenu("欢迎来到天空之城美发沙龙！你有#b#t5150003##k或者#b#t5151003##k吗？如果有的话，让我来为你打理一下头发吧？请选择你想要做的事情...\r\n#L0#理发：#i5150003##t5150003##l\r\n#L1#染发：#i5151003##t5151003##l");
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

            var option = await AskMenu("嗨，你好！欢迎来到天空之城整形外科！你想把你的脸变成全新的样子吗？使用 #b#t5152003##k，你可以让我们来照顾剩下的事情，拥有你一直想要的脸~！\r\n#L0#整形外科：#i5152003##t5152003##l");
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

            var option = await AskMenu($"我是助手#p{npc}#。你有#b#t5154000##k、#b#t5150004##k、#b#t5150013##k或#b#t5151004##k吗？如果有的话，你觉得让我来给你打理发型怎么样？你想怎么处理你的头发呢？\r\n#L0#理发：#i5154000##t5154000##l\r\n#L1#理发：#i5150004##t5150004##l\r\n#L2#理发：#i5150013##t5150013##l\r\n#L3#染发：#i5151004##t5151004##l");
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

            var option = await AskMenu("嗨，你好！欢迎来到天空之城护肤中心~！你想要像我一样拥有紧致健康的皮肤吗？使用#b#t5153001##k，让我们来照顾你的肌肤，拥有你一直想要的肌肤吧~！\r\n#L0#护肤服务：#i5153001##t5153001##l");
            if (option == 0)
            {
                await ProcessSkinChange(5153001, skinOptions, "通过我们的专业机器，你可以在手术前看到自己在治疗后的样子。你想要什么样的表情？快来选择你喜欢的风格吧～！");
            }
        }


        // Npc: 2012009 
        public async Task face_orbis2()
        {
            int[] mface_r = [20003, 20011, 20021, 20022, 20023, 20027, 20031];
            int[] fface_r = [21004, 21007, 21010, 21012, 21020, 21021, 21030];

            var option = await AskMenu("嗨，我其实不应该这样做，但是有了一个#b#t5152004##k，我还是会为你做。但别忘了，这将是随机的！\r\n#L0#整形手术：#i5152004##t5152004##l");
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

            var option = await AskMenu($"嘿！我是#p{npc}#，是马兹拉的学徒。如果你有 #b阿里安特发型券(REG)#k 或 #b阿里安特染发券(REG)#k，你愿意让我给你做头发吗？\r\n#L0#理发：#i5150026##t5150026##l\r\n#L1#染发：#i5151021##t5151021##l");
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

            var option = await AskMenu("哈哈哈...在沙漠中，一个人要注意自己的发型，需要有很多的风格和魅力。像你这样的人...如果你有#b阿里安特发型券（VIP）#k或#b阿里安特染发券（VIP）#k，我会给你的头发一个全新的造型。\r\n#L0#理发：#i5150027##t5150027##l\r\n#L1#染发：#i5151022##t5151022##l");
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

            await SayNext("Hohoh~ 欢迎欢迎。欢迎来到阿里安特护肤中心。你已经踏入了一家著名的护肤店，甚至连女王本人都经常光顾这个地方。如果你带着 #b阿里安特护肤优惠券#k，我们会照顾好你的其余事项。今天让我们来护理一下你的肌肤吧？");
            await ProcessSkinChange(5153007, skinOptions, "通过我们的专业机器，你可以在手术前看到自己在治疗后的样子。你想要什么样的表情？快来选择你喜欢的风格吧～！");
        }


        // Npc: 2100008 
        public async Task face_ariant1()
        {
            int[] mface_v = [20000, 20004, 20005, 20012, 20013, 20031];
            int[] fface_v = [21000, 21003, 21006, 21009, 21012, 21024];

            var option = await AskMenu("啊，欢迎来到阿里安特整形中心！您想将您的脸变成全新的样子吗？通过使用#b#t5152030##k或者#b#t5152047##k，我可以让您的脸变得更好看！\r\n#L0#整形手术：#i5152030##t5152030##l\r\n#L1#美瞳：#i5152047##t5152047##l\r\n#L2#一次性美瞳：#i5152101#（任何颜色）#l");
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
                await ProcessOneTimeLensChange("你想戴什么样的眼镜？请选择您喜欢的风格。");
            }
        }


        // Npc: 2100009 
        public async Task face_ariant2()
        {
            int[] mface_r = [20001, 20003, 20009, 20010, 20025, 20031];
            int[] fface_r = [21002, 21009, 21011, 21013, 21016, 21029, 21030];

            var option = await AskMenu("嗨，我是这里的整容助理医生。用一个#b#t5152029##k或者一个#b#t5152048##k，我可以让它变得完美，相信我。啊，别忘了，手术后的结果是随机的！那么，你要做哪个呢？\r\n#L0#整形手术：#i5152029##t5152029##l\r\n#L1#美瞳：#i5152048##t5152048##l");
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

            var option = await AskMenu($"你好！我是美容院的院长#p{npc}#！如果你有#b#t5150003##k或者#b#t5151003##k，为什么不让我来照顾剩下的事情呢？决定你想要怎么处理你的头发...\r\n#L0#理发：#i5150003##t5150003##l\r\n#L1#染发：#i5151003##t5151003##l");
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

            var option = await AskMenu($"我是助手#p{npc}#。如果你有#b#t5150002##k、#b#t5150011##k或者#b#t5151002##k，请让我来给你换个发型吧！\r\n#L0#理发：#i5150002##t5150002##l\r\n#L1#理发：#i5150011##t5150011##l\r\n#L2#染发：#i5151002##t5151002##l");
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

            var option = await AskMenu("欢迎来到冒险岛美容店。如果你有#b#t5150025##k或者#b#t5151020##k，请让我来为你打理发型。请选择你想要的服务。\r\n#L1#理发：#i5150025##t5150025##l\r\n#L2#染发：#i5151020##t5151020##l");
            if (option == 1)
            {
                await ProcessStyleHairChangeWithMultipleCoupons([5420006, 5150025], mhair_v, fhair_v, "我完全可以改变你的发型，让它看起来好极了。你为什么不改一下呢？只需 #b#t5150025##k，剩下的事我来帮你处理，选你喜欢的风格吧！");
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

            var option = await AskMenu("我是这家店的发型助理。如果你碰巧有#b#t5150024##k或者#b#t5151019##k，那么让我给你换个发型怎么样？\r\n#L1#理发：#i5150024##t5150024##l\r\n#L2#染发：#i5151019##t5151019##l");
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

            var option = await AskMenu("嗨，你好！欢迎来到武陵美容中心！你想要像我一样拥有紧致健康的皮肤吗？使用#b#t5153006##k，让我们来照顾你的肌肤，拥有你一直想要的肌肤吧~！\r\n#L2#美容护理：#i5153006##t5153006##l");
            if (option == 2)
            {
                await ProcessSkinChange(5153006, skinOptions, "通过我们的专业机器，你可以在手术前看到自己在治疗后的样子。你想要什么样的表情？快来选择你喜欢的风格吧～！");
            }
        }


        // Npc: 2090103 
        public async Task face_mureung1()
        {
            int[] mface_v = [20000, 20001, 20004, 20005, 20006, 20007, 20009, 20012, 20022, 20028, 20031];
            int[] fface_v = [21000, 21003, 21005, 21006, 21008, 21009, 21011, 21012, 21023, 21024, 21026];

            var option = await AskMenu($"嘿，我是#p{npc}#，我是一位在武陵著名的整形外科医生和美瞳专家。我相信你的脸和眼睛是你身体中最重要的特征，通过使用#b#t5152028##k或者#b#t5152041##k，我可以为你开具适合的面部护理和美瞳。现在，你想要使用什么？\r\n#L1#整形手术：#i5152028##t5152028##l\r\n#L2#美瞳：#i5152041##t5152041##l\r\n#L3#一次性美瞳：#i5152100#（任何颜色）#l");
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
                await ProcessOneTimeLensChange("你想戴什么样的眼镜？请选择您喜欢的风格。");
            }
        }


        // Npc: 2090104 
        public async Task face_mureung2()
        {
            int[] mface_r = [20002, 20005, 20007, 20011, 20014, 20017, 20029];
            int[] fface_r = [21001, 21010, 21013, 21018, 21020, 21021, 21030];

            var option = await AskMenu($"嘿，我是#p{npc}#，我正在帮助帕塔进行面部改变和应用隐形眼镜，作为我的实习研究。使用#b#t5152027##k或#b#t5152042##k，我可以改变你的外貌。现在，你想要使用什么？\r\n#L1#整形手术：#i5152027##t5152027##l\r\n#L2#美瞳：#i5152042##t5152042##l");
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

            var option = await AskMenu("如果你使用这张普通的优惠券，你的脸可能会变成一个随机的新样子...你还想用 #b#t5152037##k 来做吗？我会帮你做。但别忘了，它会是随机的！\r\n#L2#好的！（使用 #i5152037# #t5152037#）#l");
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

            var option = await AskMenu("让我看看……我可以完全改变你的脸，让它变成全新的样子。你不想试试吗？用 #b#t5152038##k，你可以得到你喜欢的脸。慢慢挑选你喜欢的脸……\r\n#L2#让我得到我梦寐以求的脸！（使用 #i5152038# #t5152038#）#l");
            if (option == 2)
            {
                await ProcessStyleFaceChange(5152038, mface_v, fface_v);
            }
        }


        // Npc: 9270025 
        public async Task skin_sg1()
        {
            int[] skinOptions = [0, 1, 2, 3, 4];

            var option = await AskMenu("嗨，你好！欢迎来到莲花花护肤中心！你想要像我一样拥有紧致健康的肌肤吗？使用#b#t5153010##k，让我们来照顾你的肌肤，让你拥有一直想要的肌肤！\r\n#L1#听起来不错！（使用#i5153010# #t5153010#）#l");
            if (option == 1)
            {
                await ProcessSkinChange(5153010, skinOptions, "通过我们的专业服务，你可以提前看到治疗后的样子。你想要什么样的皮肤护理？请选择你喜欢的风格...");
            }
        }


        // Npc: 9270026 
        public async Task lens_sg1()
        {
            var option = await AskMenu($"嗨，你好！我是#p{npc}#，在CBD的大眼睛镜片店负责这里的事务！使用#b#t5152039##k或#b#t5152040##k，你可以让我们来处理剩下的事情，拥有你一直渴望的美丽外观！记住，每个人注意到的第一件事就是眼睛，我们可以帮助你找到最适合你的美瞳！那么，你想要使用什么呢？\r\n#L1#美瞳：#i5152039##t5152039##l\r\n#L2#美瞳：#i5152040##t5152040##l\r\n#L3#一次性美瞳：#i5152107#（任何颜色）#l");
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
                await ProcessOneTimeLensChange("你想戴什么样的眼镜？请选择您喜欢的风格。");
            }
        }

        // Npc: 9270036 
        public async Task hair_sg1()
        {
            int[] mhair_v = [30000, 30020, 30110, 30120, 30270, 30290, 30310, 30670, 30840];
            int[] fhair_v = [31010, 31050, 31110, 31120, 31240, 31250, 31280, 31670, 31810];

            var option = await AskMenu("欢迎来到快手美发沙龙！你有#b#t5150033##k或者#b#t5151028##k吗？如果有的话，让我来给你打理一下头发吧？请告诉我你想要做什么。\r\n#L1#理发：#i5150033##t5150033##l\r\n#L2#染发：#i5151028##t5151028##l");
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

            var option = await AskMenu("嗨，我是这里的助手。别担心，我完全有能力做到这一点。如果你碰巧有#b#t5150032##k或#b#t5151027##k，那就让我来处理剩下的事情吧？\r\n#L1#理发：#i5150032##t5150032##l\r\n#L2#染发：#i5151027##t5151027##l");
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
                await SayNext("我已经帮你做过一次头发，作为一种服务交换，伙计。如果你想再次改变发型，你需要从现金商店购买一个经验头发券！");
                return;
            }

            if (await AskYesNo("准备好要做一个超棒的发型了吗？我觉得你准备好了！只要说出来，我们就可以开始了！"))
            {
                var hairnew = new List<int>();
                var hairs = getPlayer().Gender == 0 ? mhair_q : fhair_q;
                foreach (var h in hairs)
                {
                    hairnew.Add(h);
                }
                await SayNext("我们开始吧！");

                if (haveItem(4031528))
                {
                    gainItem(4031528, -1);
                    var hair = Randomizer.Select(hairnew.ToArray());
                    setHair(hair);
                    await SayNext("还不错，如果我这么说的话！我知道我学习的那些书会派上用场……");
                }
                else
                {
                    await SayNext("嗯...你确定你有我们指定的免费优惠券吗？抱歉，没有优惠券就不能理发。");
                }
            }
        }

        // Npc: 9201069 
        public async Task NLC_FaceVip()
        {
            int[] mface_v = [20000, 20001, 20003, 20004, 20005, 20006, 20008, 20012, 20031];
            int[] fface_v = [20000, 20001, 20003, 20004, 20005, 20006, 20008, 20012, 20031];

            var option = await AskMenu("嗨，你好！欢迎来到新叶城整形外科！你想把你的脸变成全新的样子吗？使用 #b#t5152034##k，你可以让我们来照顾剩下的事情，拥有你一直想要的脸~！\r\n#L2#整形外科：#i5152034##t5152034##l");
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

            var option = await AskMenu("嗨，我其实不应该这样做，但是有了#b#t5152033##k，我还是会为你做。但别忘了，结果会是随机的！\r\n#L2#整形手术：#i5152033##t5152033##l");
            if (option == 2)
            {
                await ProcessRandomFaceChange(5152033, mface_r, fface_r);
            }
        }


        // Npc: 9201061 
        public async Task NLC_LensExp()
        {
            var option = await AskMenu($"嗨，你好~！我是#p{npc}#。如果你有#b#t5152035##k，我可以为你开具合适的美瞳。现在，你想做什么呢？\r\n#L2#美瞳：#i5152035##t5152035##l");
            if (option == 2)
            {
                await ProcessRandomLensChange(5152035, [100, 200, 300, 400, 500, 600, 700]);
            }
        }


        // Npc: 9201062 
        public async Task NLC_LensVip()
        {
            var option = await AskMenu($"嘿，你好~！我是#p{npc}#！我负责NLC商店的隐形眼镜！如果你有#b#t5152036##k，我可以为你提供有史以来最好的隐形眼镜！现在，你想做什么呢？\r\n#L2#隐形眼镜：#i5152036##t5152036##l\r\n#L3#一次性隐形眼镜：#i5152107#（任何颜色）#l");
            if (option == 2)
            {
                await ProcessStyleLensChange(5152036, [100, 200, 300, 400, 500, 600, 700]);
            }
            else if (option == 3)
            {
                await ProcessOneTimeLensChange("你想戴什么样的眼镜？请选择您喜欢的风格。");
            }
        }


        // Npc: 9201063 
        public async Task NLC_HairExp()
        {
            int[] mhair_e = [30250, 30400, 30430, 30440, 30490, 30730, 30830, 30870, 30880, 33100];
            int[] fhair_e = [31320, 31450, 31560, 31570, 31690, 31720, 31730, 31830, 34010];

            var option = await AskMenu($"我是助手#p{npc}#。如果你碰巧有#b#t5150030##k或者#b#t5151025##k，那么让我来帮你换个发型怎么样？\r\n#L1#理发：#i5150030##t5150030##l\r\n#L2#染发：#i5151025##t5151025##l");
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

            var option = await AskMenu($"我是这家美发店的负责人#p{npc}#。如果你有#b#t5150031##k或者#b#t5151026##k，请让我来为你打理发型。请选择你想要的那个。\r\n#L1#理发：#i5150031##t5150031##l\r\n#L2#染发：#i5151026##t5151026##l");
            if (option == 1)
            {
                await ProcessStyleHairChangeWithMultipleCoupons([5420001, 5150031], mhair_v, fhair_v, "我完全可以改变你的发型，让它看起来好极了。你为什么不改一下呢？只需 #b#t5150031##k，剩下的事我来帮你处理，选你喜欢的风格吧！");
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

            var option = await AskMenu("嗨，你好！欢迎来到NLC护肤中心！你想要像我一样拥有紧致健康的肌肤吗？使用#b#t5153009##k，你可以让我们来照顾剩下的部分，拥有你一直想要的肌肤~！\r\n#L2#护肤服务：#i5153009##t5153009##l");
            if (option == 2)
            {
                await ProcessSkinChange(5153009, skin, "通过我们的专业机器，你可以在手术前看到自己在治疗后的样子。你想要什么样的表情？快来选择你喜欢的风格吧～！");
            }
        }

        // Npc: 9200100 
        public async Task lens_henesys1()
        {
            var option = await AskMenu($"嗨，你好~！我是#p{npc}#，负责冒险岛的Henesys整形外科店的美瞳服务！使用#b#t5152010##k或者#b#t5152013##k，你可以让我们来照顾剩下的事情，拥有你一直渴望的美丽外观~！记住，每个人注意到的第一件事就是你的眼睛，我们可以帮助你找到最适合你的美瞳！那么，你想要使用什么呢？\r\n#L1#美瞳：#i5152010##t5152010##l\r\n#L2#美瞳：#i5152013##t5152013##l\r\n#L3#一次性美瞳：#i5152103#（任何颜色）#l");
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
                await ProcessOneTimeLensChange("你想戴什么样的眼镜？请选择您喜欢的风格。");
            }
        }


        // Npc: 9200101 
        public async Task lens_orbis1()
        {
            var option = await AskMenu($"你好，我是#p{npc}#，是天空之城整形外科店的美瞳部门主任。\r\n我的目标是通过美瞳的奇迹为每个人的眼睛增添个性，而且通过#b#t5152011##k或者#b#t5152014##k，我也可以为你做同样的事情！现在，你想要使用哪个？\r\n#L1#美瞳：#i5152011##t5152011##l\r\n#L2#美瞳：#i5152014##t5152014##l\r\n#L3#一次性美瞳：#i5152104#（任何颜色）#l");
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
                await ProcessOneTimeLensChange("你想戴什么样的眼镜？请选择您喜欢的风格。");
            }
        }


        // Npc: 9200102 
        public async Task lens_ludi1()
        {
            var option = await AskMenu($"嗯...嗨，我是#p{npc}#，我是卢迪布里姆整形外科诊所的美瞳专家。我相信你的眼睛是你身体中最重要的特征，通过使用#b#t5152012##k或者#b#t5152015##k，我可以为你开具适合的美瞳。现在，你想要使用哪种呢？\r\n#L1#美瞳：#i5152012##t5152012##l\r\n#L2#美瞳：#i5152015##t5152015##l\r\n#L3#一次性美瞳：#i5152105#（任何颜色）#l");
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
                await ProcessOneTimeLensChange("你想戴什么样的眼镜？请选择您喜欢的风格。");
            }
        }





        // Npc: 9201015 
        public async Task hair_wedding1()
        {
            int[] mhair_v = [30050, 30300, 30410, 30450, 30510, 30570, 30580, 30590, 30660, 30910];
            int[] fhair_v = [31150, 31220, 31260, 31310, 31420, 31480, 31490, 31580, 31590, 31610, 31630];

            var option = await AskMenu("欢迎来到阿莫利亚美发店。如果你有#b#t5150020##k，或者#b#t5151017##k，请让我来为你打理发型。请选择你想要的服务。\r\n#L1#理发：#i5150020##t5150020##l\r\n#L2#染发：#i5151017##t5151017##l");
            if (option == 1)
            {
                await ProcessStyleHairChangeWithMultipleCoupons([5420000, 5150020], mhair_v, fhair_v, "我完全可以改变你的发型，让它看起来好极了。你为什么不改一下呢？只需 #b#t5150020##k，剩下的事我来帮你处理，选你喜欢的风格吧！");
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

            var option = await AskMenu($"我是造型师#p{npc}#。如果你碰巧有#b#t5150019##k或者#b#t5151016##k，那么让我来给你换个发型怎么样？\r\n#L1#理发：#i5150019##t5150019##l\r\n#L2#染发：#i5151016##t5151016##l");
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
            var option = await AskMenu($"嗨，你好~！我是#p{npc}#，负责Amoria整形外科店的美瞳。使用#b#t5152025##k或#b#t5152026##k，你可以让我们来处理剩下的事情，拥有你一直渴望的美丽外观~！记住，每个人注意到的第一件事就是眼睛，我们可以帮助你找到最适合你的美瞳！那么，你想要使用什么呢？\r\n#L1#美瞳：#i5152025##t5152025##l\r\n#L2#美瞳：#i5152026##t5152026##l\r\n#L3#一次性美瞳：#i5152106#（任何颜色）#l");
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
                await ProcessOneTimeLensChange("你想戴什么样的眼镜？请选择您喜欢的风格。");
            }
        }


        // Npc: 9201018 
        public async Task face_wedding1()
        {
            int[] mface_v = [20000, 20001, 20003, 20004, 20005, 20006, 20007, 20008, 20018, 20019];
            int[] fface_v = [21001, 21002, 21003, 21004, 21005, 21006, 21007, 21012, 21018, 21019];

            var option = await AskMenu("嗨，你好！欢迎来到阿莫利亚整形外科！你想把你的脸变成全新的样子吗？使用#b#t5152022##k，你可以让我们来照顾剩下的事情，拥有你一直想要的脸~！\r\n#L2#整形外科：#i5152022##t5152022##l");
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

            var option = await AskMenu("嗨，我其实不应该这样做，但是有了#b#t5152021##k，我还是会为你做。但别忘了，结果会是随机的！\r\n#L2#整形手术：#i5152021##t5152021##l");
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

            var option = await AskMenu("欢迎来到昭和发廊。如果你有#b#t5150009##k或者#b#t5151009##k，请让我来为你打理发型。请选择你想要的服务。\r\n#L1#理发：#i5150009##t5150009##l\r\n#L2#染发：#i5151009##t5151009##l");
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

            var option = await AskMenu("嗨，我是这里的助手。别担心，我完全有能力做到这一点。如果你碰巧有#b#t5150008##k或#b#t5151008##k，那就让我来处理剩下的事情，好吗？\r\n#L1#理发：#i5150008##t5150008##l\r\n#L2#染发：#i5151008##t5151008##l");
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

            var option = await AskMenu("嗯嗯嗯，欢迎来到昭和整形外科！您想将您的脸变成全新的样子吗？通过使用#b#t5152009##k或者#b#t5152045##k，您可以让我们来照顾剩下的事情，拥有您一直想要的脸~！\r\n#L1#整形手术：#i5152009##t5152009##l\r\n#L2#美瞳：#i5152045##t5152045##l\r\n#L3#一次性美瞳：#i5152102#（任何颜色）#l");
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
                await ProcessOneTimeLensChange("你想戴什么样的眼镜？请选择您喜欢的风格。");
            }
        }


        // Npc: 9120103 
        public async Task face_shouwa2()
        {
            int[] mface_r = [20000, 20016, 20019, 20020, 20021, 20024, 20026];
            int[] fface_r = [21000, 21002, 21009, 21016, 21022, 21025, 21027];

            var option = await AskMenu("嗨，我其实不应该这样做，但是用#b#t5152008##k或者#b#t5152046##k，我还是会为你做。但别忘了，这将是随机的！\r\n#L1#整形手术：#i5152008##t5152008##l\r\n#L2#美容镜片：#i5152046##t5152046##l");
            if (option == 1)
            {
                await ProcessRandomFaceChange(5152008, mface_r, fface_r);
            }
            else if (option == 2)
            {
                await ProcessRandomLensChange(5152046, [0, 100, 200, 300, 400, 500, 700]);
            }
        }
    }
}
