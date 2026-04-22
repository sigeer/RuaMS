using Application.Utility;

namespace Application.Plugin.Script
{
    internal partial class NpcScript
    {
        // Npc: 1012117 
        public async Task hair_royal()
        {
            int[] mhair_r = [30010, 30070, 30080, 30090, 30100, 30690, 30760, 33000];
            int[] fhair_r = [31130, 31530, 31820, 31920, 31940, 34000, 34030];

            int[] mhair_v = [30010, 30070, 30080, 30090, 30100, 30480, 30560, 30690, 30760, 30850, 30890, 30930, 30950];
            int[] fhair_v = [31020, 31130, 31510, 31530, 31820, 31860, 31890, 31920, 31940, 31950, 34000];


            var option = await SayOption("嗨，我是#p1012117#，最迷人、最时尚的造型师。如果你正在寻找最漂亮的发型，那就不用再找了！\r\n#L0##i5150040##t5150040##l\r\n#L1##i5150044##t5150044##l");
            switch (option)
            {
                case 0:
                    if (await SayYesNo("如果你使用#t5150040#，你的头发可能会变成一个随机的新造型……你还想用 #b#t5150040##k 来做吗？我会帮你做。但别忘了，结果会是随机的！"))
                    {
                        var hair = Randomizer.Select((getPlayer().Gender == 0 ? mhair_r : fhair_r).Select(x => x + getPlayer().Hair % 10).ToArray());
                        if (haveItem(5150040))
                        {
                            gainItem(5150040, -1);
                            setHair(hair);
                            await SayOK("享受你的新发型吧！");
                        }
                        else
                        {
                            await SayOK("嗯...看起来你没有#t5150040#...恐怕我不能给你理发。对不起...");
                        }
                    }
                    break;
                case 1:
                    var affectHair = (getPlayer().Gender == 0 ? mhair_v : fhair_v).Select(x => x + getPlayer().Hair % 10).ToArray();

                    var hairIdx = await SayStyle("使用#t5150040#，您可以选择发型的造型。挑选最适合您心意的发型", affectHair);
                    if (haveItem(5150044))
                    {
                        gainItem(5150044, -1);
                        setHair(affectHair[hairIdx]);
                        await SayOK("享受你的新发型吧！");
                    }
                    else
                    {
                        await SayOK("嗯...看起来你没有#t5150040#...恐怕我不能给你理发。对不起...");
                    }
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

            var option = await SayOption("我是这家美发沙龙的负责人。如果你有#b#t5150001##k或者#b#t5151001##k，请让我来为你打理发型。请选择你想要的那个。\r\n#L0#理发：#i5150001##t5150001##l\r\n#L1#染发：#i5151001##t5151001##l");
            if (option == 0)
            {
                // Hair cut
                var hairnew = new List<int>();
                var baseHair = getPlayer().Hair % 10;
                var hairs = getPlayer().Gender == 0 ? mhair_v : fhair_v;
                foreach (var h in hairs)
                {
                    var newHair = h + baseHair;
                    // Assuming cosmetic check is handled or simplified
                    hairnew.Add(newHair);
                }
                var hairIdx = await SayStyle("我完全可以改变你的发型，让它看起来好极了。你为什么不改一下呢？如果你有 #b#t5150001##k 就可以。选一个自己喜欢的吧~。", hairnew.ToArray());
                if (haveItem(5420002) || haveItem(5150001))
                {
                    if (haveItem(5420002))
                    {
                        // Membership coupon, no consume?
                    }
                    else
                    {
                        gainItem(5150001, -1);
                    }
                    setHair(hairnew[hairIdx]);
                    await SayOK("享受你的新发型吧！");
                }
                else
                {
                    await SayOK("嗯...看起来你没有我们指定的优惠券...恐怕我不能给你理发。对不起...");
                }
            }
            else if (option == 1)
            {
                // Hair color
                var haircolor = new List<int>();
                var current = (getPlayer().Hair / 10) * 10;
                for (int i = 0; i < 8; i++)
                {
                    var newColor = current + i;
                    // Assuming cosmetic check
                    haircolor.Add(newColor);
                }
                var colorIdx = await SayStyle("我完全可以改变你的发色，让它看起来那么好。你为什么不改一下呢？只需要 #b#t5151001##k 就可以。选一个自己喜欢的吧~。", haircolor.ToArray());
                if (haveItem(5151001))
                {
                    gainItem(5151001, -1);
                    setHair(haircolor[colorIdx]);
                    await SayOK("享受你的新发色！");
                }
                else
                {
                    await SayOK("嗯...看起来你没有我们指定的优惠券...恐怕我不能给你染发。很抱歉...");
                }
            }
        }


        // Npc: 1012104 
        public async Task hair_henesys2()
        {
            int[] mhair_r = [30060, 30140, 30200, 30210, 30310, 30610, 33040, 33100];
            int[] fhair_r = [31070, 31080, 31150, 31300, 31350, 31700, 34050, 34110];
            int[] mhair_e = [30030, 30140, 30200, 30210, 30310, 30610, 33040, 33100];
            int[] fhair_e = [31070, 31150, 31300, 31350, 31430, 31700, 34050, 34110];

            var option = await SayOption("我是布兰妮助手。如果你碰巧有#b#t5150000##k、#b#t5150010##k或#b#t5151000##k，那么让我来帮你换个发型怎么样？\r\n#L0#理发：#i5150000##t5150000##l\r\n#L1#理发：#i5150010##t5150010##l\r\n#L2#染发：#i5151000##t5151000##l");
            if (option == 0)
            {
                // REG random hair
                var hairnew = new List<int>();
                var baseHair = getPlayer().Hair % 10;
                var hairs = getPlayer().Gender == 0 ? mhair_r : fhair_r;
                foreach (var h in hairs)
                {
                    hairnew.Add(h + baseHair);
                }
                if (await SayYesNo("如果您使用REG优惠券，您的发型将随机改变，并有机会获得一种新的实验风格，甚至您自己都认为不可能。您要使用#b#t5150000##k来真正改变您的发型吗？"))
                {
                    if (haveItem(5150000))
                    {
                        gainItem(5150000, -1);
                        var hair = Randomizer.Select(hairnew.ToArray());
                        setHair(hair);
                        await SayOK("享受你的新发型吧！");
                    }
                    else
                    {
                        await SayOK("嗯...看起来你没有#t5150000#...恐怕我不能给你理发。对不起...");
                    }
                }
            }
            else if (option == 1)
            {
                // EXP random hair
                var hairnew = new List<int>();
                var baseHair = getPlayer().Hair % 10;
                var hairs = getPlayer().Gender == 0 ? mhair_e : fhair_e;
                foreach (var h in hairs)
                {
                    hairnew.Add(h + baseHair);
                }
                if (await SayYesNo("如果你使用了经验值券，你的发型将会随机改变，并有机会获得一种你自己都没想到可能存在的新实验性发型。你要使用 #b#t5150010##k 真的改变你的发型吗？"))
                {
                    if (haveItem(5150010))
                    {
                        gainItem(5150010, -1);
                        var hair = Randomizer.Select(hairnew.ToArray());
                        setHair(hair);
                        await SayOK("享受你的新发型吧！");
                    }
                    else
                    {
                        await SayOK("嗯...看起来你没有#t5150010#...恐怕我不能给你理发。对不起...");
                    }
                }
            }
            else if (option == 2)
            {
                // Random color
                var haircolor = new List<int>();
                var current = (getPlayer().Hair / 10) * 10;
                for (int i = 0; i < 8; i++)
                {
                    haircolor.Add(current + i);
                }
                if (await SayYesNo("如果你使用普通的优惠券，你的发型将会随机改变。你还想使用 #b#t5151000##k 来改变吗？"))
                {
                    if (haveItem(5151000))
                    {
                        gainItem(5151000, -1);
                        var color = Randomizer.Select(haircolor.ToArray());
                        setHair(color);
                        await SayOK("享受你的新发色！");
                    }
                    else
                    {
                        await SayOK("嗯...看起来你没有#t5151000#...恐怕我不能给你染发。对不起...");
                    }
                }
            }
        }


        // Npc: 1012105 
        public async Task skin_henesys1()
        {
            int[] skinOptions = [0, 1, 2, 3, 4];

            var option = await SayOption("嗨，你好！欢迎来到明斯皮肤护理中心！你想要像我一样拥有紧致健康的皮肤吗？使用 #b#t5153000##k，你可以让我们来照顾剩下的事情，拥有你一直想要的肌肤~！\r\n#L0#皮肤护理：#i5153000##t5153000##l");
            if (option == 0)
            {
                if (haveItem(5153000))
                {
                    var skinIdx = await SayStyle("使用我们的专业机器，您可以提前看到治疗后的自己。您想做什么样的皮肤护理？选择您喜欢的风格。", skinOptions);
                    gainItem(5153000, -1);
                    setSkin(skinOptions[skinIdx]);
                    await SayOK("享受你的新肤色吧！");
                }
                else
                {
                    await SayOK("嗯...你没有你需要接受治疗的护肤券。抱歉，恐怕我们不能为你做这个...");
                }
            }
        }

        // Npc: 1052004 
        public async Task face_henesys1()
        {
            int[] mface_v = [20000, 20001, 20003, 20004, 20005, 20006, 20007, 20008, 20012, 20014, 20015, 20022, 20028, 20031];
            int[] fface_v = [21000, 21001, 21002, 21003, 21004, 21005, 21006, 21007, 21008, 21012, 21013, 21014, 21023, 21026];

            var option = await SayOption("嗨，你好！欢迎来到射手村整形外科！你想把你的脸变成全新的样子吗？使用 #b#t5152001##k，你可以让我们来照顾剩下的事情，拥有你一直想要的脸~！\r\n#L0#整形外科：#i5152001##t5152001##l");
            if (option == 0)
            {
                var facenew = new List<int>();
                var baseFace = getPlayer().Face % 1000 - (getPlayer().Face % 100);
                var faces = getPlayer().Gender == 0 ? mface_v : fface_v;
                foreach (var f in faces)
                {
                    facenew.Add(f + baseFace);
                }
                var faceIdx = await SayStyle("让我看看... 我完全可以把你的脸变成新的。 你不想试试吗？ 使用 #b#t5152001##k, 花点时间选择你可以得到你喜欢的面孔。", facenew.ToArray());
                if (haveItem(5152001))
                {
                    gainItem(5152001, -1);
                    setFace(facenew[faceIdx]);
                    await SayOK("享受你的新面容吧！");
                }
                else
                {
                    await SayOK("嗯...看起来你没有这个地方专门的优惠券。很抱歉要说这个，但没有优惠券，你就不能进行整形手术了...");
                }
            }
        }


        // Npc: 1052005 
        public async Task face_henesys2()
        {
            int[] mface_r = [20000, 20005, 20008, 20012, 20016, 20022, 20032];
            int[] fface_r = [21000, 21002, 21008, 21014, 21020, 21024, 21029];

            var option = await SayOption("嗨，我其实不应该这样做，但是用一个#b#t5152000##k，我还是会为你做。但别忘了，结果会是随机的！\r\n#L0#整形手术：#i5152000##t5152000##l");
            if (option == 0)
            {
                var facenew = new List<int>();
                var baseFace = getPlayer().Face % 1000 - (getPlayer().Face % 100);
                var faces = getPlayer().Gender == 0 ? mface_r : fface_r;
                foreach (var f in faces)
                {
                    facenew.Add(f + baseFace);
                }
                if (await SayYesNo("如果你使用普通的优惠券，你的脸可能会变成一个随机的新样子……你还想用#b#t5152000##k来做吗？"))
                {
                    if (haveItem(5152000))
                    {
                        gainItem(5152000, -1);
                        var face = Randomizer.Select(facenew.ToArray());
                        setFace(face);
                        await SayOK("享受你的新面容吧！");
                    }
                    else
                    {
                        await SayOK("嗯...看起来你没有这个地方专门的优惠券。很抱歉要说这个，但没有优惠券，你就不能进行整形手术了...");
                    }
                }
            }
        }



        // Npc: 2041007 
        public async Task hair_ludi1()
        {
            int[] mhair_v = [30160, 30190, 30250, 30640, 30660, 30840, 30870, 30990];
            int[] fhair_v = [31270, 31290, 31550, 31680, 31810, 31830, 31840, 31870];

            var option = await SayOption("欢迎来到鲁塔比姆美发沙龙！你有#b#t5150007##k或者#b#t5151007##k吗？如果有的话，让我来为你打理一下头发吧？请选择你想要做的事情...\r\n#L0#理发：#i5150007##t5150007##l\r\n#L1#染发：#i5151007##t5151007##l");
            if (option == 0)
            {
                // Hair cut
                var hairnew = new List<int>();
                var baseHair = getPlayer().Hair % 10;
                var hairs = getPlayer().Gender == 0 ? mhair_v : fhair_v;
                foreach (var h in hairs)
                {
                    hairnew.Add(h + baseHair);
                }
                var hairIdx = await SayStyle("我可以完全改变你的发型，你还没准备好接受改变吗？给我 #b#t5150007##k，剩下的事我来帮你处理，选你喜欢的风格吧！", hairnew.ToArray());
                if (haveItem(5420005) || haveItem(5150007))
                {
                    if (haveItem(5420005))
                    {
                        // Membership
                    }
                    else
                    {
                        gainItem(5150007, -1);
                    }
                    setHair(hairnew[hairIdx]);
                    await SayOK("享受你的新发型吧！");
                }
                else
                {
                    await SayOK("嗯...看起来你没有我们指定的优惠券...恐怕我不能给你理发。对不起...");
                }
            }
            else if (option == 1)
            {
                // Hair color
                var haircolor = new List<int>();
                var current = (getPlayer().Hair / 10) * 10;
                for (int i = 0; i < 8; i++)
                {
                    haircolor.Add(current + i);
                }
                var colorIdx = await SayStyle("我可以完全改变你头发的颜色，你还没准备好接受改变吗？给我 #b#t5151007##k，剩下的我来负责，选你喜欢的颜色吧！", haircolor.ToArray());
                if (haveItem(5151007))
                {
                    gainItem(5151007, -1);
                    setHair(haircolor[colorIdx]);
                    await SayOK("享受你的新发色！");
                }
                else
                {
                    await SayOK("嗯...看起来你没有我们指定的优惠券...恐怕我不能给你染发。对不起...");
                }
            }
        }


        // Npc: 2041009 
        public async Task hair_ludi2()
        {
            int[] mhair_r = [30190, 30220, 30250, 30540, 30610, 30620, 30640, 30650, 30660, 30840, 30870, 30940, 30990];
            int[] fhair_r = [31170, 31270, 31290, 31510, 31540, 31550, 31600, 31640, 31680, 31810, 31830, 31840, 31870];
            int[] mhair_e = [30030, 30190, 30220, 30250, 30540, 30610, 30620, 30640, 30650, 30660, 30840, 30990];
            int[] fhair_e = [31170, 31270, 31430, 31510, 31540, 31550, 31600, 31680, 31810, 31830, 31840, 31870];

            var option = await SayOption("嗨，我是这里的助手。别担心，我完全能胜任这个任务。如果你碰巧有#b#t5150006##k、#b#t5150012##k或#b#t5151006##k，那就让我来处理剩下的事情，好吗？\r\n#L0#理发：#i5150006##t5150006##l\r\n#L1#理发：#i5150012##t5150012##l\r\n#L2#染发：#i5151006##t5151006##l");
            if (option == 0)
            {
                // REG random hair
                var hairnew = new List<int>();
                var baseHair = getPlayer().Hair % 10;
                var hairs = getPlayer().Gender == 0 ? mhair_r : fhair_r;
                foreach (var h in hairs)
                {
                    hairnew.Add(h + baseHair);
                }
                if (await SayYesNo("如果您使用REG优惠券，您的发型将随机改变，并有机会获得我设计的新实验风格。您要使用#b#t5150006##k来真正改变您的发型吗？"))
                {
                    if (haveItem(5150006))
                    {
                        gainItem(5150006, -1);
                        var hair = Randomizer.Select(hairnew.ToArray());
                        setHair(hair);
                        await SayOK("享受你的新发型吧！");
                    }
                    else
                    {
                        await SayOK("嗯...看起来你没有#t5150006#...恐怕我不能给你理发。对不起...");
                    }
                }
            }
            else if (option == 1)
            {
                // EXP random hair
                var hairnew = new List<int>();
                var baseHair = getPlayer().Hair % 10;
                var hairs = getPlayer().Gender == 0 ? mhair_e : fhair_e;
                foreach (var h in hairs)
                {
                    hairnew.Add(h + baseHair);
                }
                if (await SayYesNo("如果你使用了经验值券，你的发型将会随机改变，并有机会获得我设计的新实验性发型。你要使用 #b#t5150012##k 真的改变你的发型吗？"))
                {
                    if (haveItem(5150012))
                    {
                        gainItem(5150012, -1);
                        var hair = Randomizer.Select(hairnew.ToArray());
                        setHair(hair);
                        await SayOK("享受你的新发型吧！");
                    }
                    else
                    {
                        await SayOK("嗯...看起来你没有#t5150012#...恐怕我不能给你理发。对不起...");
                    }
                }
            }
            else if (option == 2)
            {
                // Random color
                var haircolor = new List<int>();
                var current = (getPlayer().Hair / 10) * 10;
                for (int i = 0; i < 8; i++)
                {
                    haircolor.Add(current + i);
                }
                if (await SayYesNo("如果你使用普通的优惠券，你的发型将会随机改变。你还想使用 #b#t5151006##k 来改变吗？"))
                {
                    if (haveItem(5151006))
                    {
                        gainItem(5151006, -1);
                        var color = Randomizer.Select(haircolor.ToArray());
                        setHair(color);
                        await SayOK("享受你的新发色！");
                    }
                    else
                    {
                        await SayOK("嗯...看起来你没有#t5151006#...恐怕我不能给你染发。对不起...");
                    }
                }
            }
        }

        // Npc: 2041010 
        public async Task face_ludi1()
        {
            int[] mface_v = [20000, 20001, 20003, 20004, 20005, 20006, 20007, 20008, 20011, 20012, 20014, 20031];
            int[] fface_v = [21000, 21001, 21002, 21003, 21004, 21005, 21006, 21007, 21008, 21010, 21012, 21014];

            var option = await SayOption("嗨，你好！欢迎来到鲁塔比姆整形外科！你想把你的脸变成全新的样子吗？使用 #b#t5152007##k，你可以让我们来照顾剩下的事情，拥有你一直想要的脸~！\r\n#L0#整形外科：#i5152007##t5152007##l");
            if (option == 0)
            {
                var facenew = new List<int>();
                var baseFace = getPlayer().Face % 1000 - (getPlayer().Face % 100);
                var faces = getPlayer().Gender == 0 ? mface_v : fface_v;
                foreach (var f in faces)
                {
                    facenew.Add(f + baseFace);
                }
                var faceIdx = await SayStyle("让我看看... 我完全可以把你的脸变成新的。 你不想试试吗？ 使用 #b#t5152007##k, 花点时间选择你可以得到你喜欢的面孔。", facenew.ToArray());
                if (haveItem(5152007))
                {
                    gainItem(5152007, -1);
                    setFace(facenew[faceIdx]);
                    await SayOK("享受你的新面容吧！");
                }
                else
                {
                    await SayOK("嗯...看起来你没有这个地方专门的优惠券。很抱歉要说这个，但没有优惠券，你就不能进行整形手术了...");
                }
            }
        }


        // Npc: 2041013 
        public async Task skin_ludi1()
        {
            int[] skinOptions = [0, 1, 2, 3, 4];

            var option = await SayOption("哦，你好！欢迎来到鲁塔比姆美容中心！你有兴趣晒黑变性感吗？或者想要拥有美丽雪白的肌肤？如果你有#b#t5153002##k，你可以让我们来照顾剩下的事情，拥有你一直梦寐以求的肌肤！\r\n#L0#美容护肤：#i5153002##t5153002##l");
            if (option == 0)
            {
                if (haveItem(5153002))
                {
                    var skinIdx = await SayStyle("通过我们的专业机器，你可以在手术前看到自己在治疗后的样子。你想要什么样的表情？快来选择你喜欢的风格吧～！", skinOptions);
                    gainItem(5153002, -1);
                    setSkin(skinOptions[skinIdx]);
                    await SayOK("享受你的新肤色吧！");
                }
                else
                {
                    await SayOK("嗯...您没有需要接受护肤折扣的券。抱歉，恐怕我们不能为您服务...");
                }
            }
        }

        // Npc: 2010001 
        public async Task hair_orbis1()
        {
            int[] mhair_v = [30230, 30260, 30280, 30340, 30490];
            int[] fhair_v = [31110, 31220, 31230, 31630, 31790];

            var option = await SayOption("欢迎来到奥比斯美发沙龙！你有#b#t5150003##k或者#b#t5151003##k吗？如果有的话，让我来为你打理一下头发吧？请选择你想要做的事情...\r\n#L0#理发：#i5150003##t5150003##l\r\n#L1#染发：#i5151003##t5151003##l");
            if (option == 0)
            {
                // Hair cut
                var hairnew = new List<int>();
                var baseHair = getPlayer().Hair % 10;
                var hairs = getPlayer().Gender == 0 ? mhair_v : fhair_v;
                foreach (var h in hairs)
                {
                    hairnew.Add(h + baseHair);
                }
                var hairIdx = await SayStyle("我可以完全改变你的发型，你还没准备好接受改变吗？给我 #b#t5150003##k，剩下的事我来帮你处理，选你喜欢的风格吧！", hairnew.ToArray());
                if (haveItem(5150003))
                {
                    gainItem(5150003, -1);
                    setHair(hairnew[hairIdx]);
                    await SayOK("享受你的新发型吧！");
                }
                else
                {
                    await SayOK("嗯...看起来你没有我们指定的优惠券...恐怕我不能给你理发。对不起...");
                }
            }
            else if (option == 1)
            {
                // Hair color
                var haircolor = new List<int>();
                var current = (getPlayer().Hair / 10) * 10;
                for (int i = 0; i < 8; i++)
                {
                    haircolor.Add(current + i);
                }
                var colorIdx = await SayStyle("我可以完全改变你头发的颜色，你还没准备好接受改变吗？给我 #b#t5151003##k，剩下的我来负责，选你喜欢的颜色吧！", haircolor.ToArray());
                if (haveItem(5151003))
                {
                    gainItem(5151003, -1);
                    setHair(haircolor[colorIdx]);
                    await SayOK("享受你的新发色！");
                }
                else
                {
                    await SayOK("嗯...看起来你没有我们指定的优惠券...恐怕我不能给你染发。对不起...");
                }
            }
        }


        // Npc: 2010002 
        public async Task face_orbis1()
        {
            int[] mface_v = [20000, 20001, 20003, 20004, 20005, 20006, 20007, 20008, 20012, 20014, 20022, 20028, 20031];
            int[] fface_v = [21000, 21001, 21002, 21003, 21004, 21005, 21006, 21007, 21008, 21012, 21014, 21023, 21026];

            var option = await SayOption("嗨，你好！欢迎来到奥比斯整形外科！你想把你的脸变成全新的样子吗？使用 #b#t5152003##k，你可以让我们来照顾剩下的事情，拥有你一直想要的脸~！\r\n#L0#整形外科：#i5152003##t5152003##l");
            if (option == 0)
            {
                var facenew = new List<int>();
                var baseFace = getPlayer().Face % 1000 - (getPlayer().Face % 100);
                var faces = getPlayer().Gender == 0 ? mface_v : fface_v;
                foreach (var f in faces)
                {
                    facenew.Add(f + baseFace);
                }
                var faceIdx = await SayStyle("让我看看... 我完全可以把你的脸变成新的。 你不想试试吗？ 使用 #b#t5152003##k, 花点时间选择你可以得到你喜欢的面孔。", facenew.ToArray());
                if (haveItem(5152003))
                {
                    gainItem(5152003, -1);
                    setFace(facenew[faceIdx]);
                    await SayOK("享受你的新面容吧！");
                }
                else
                {
                    await SayOK("嗯...看起来你没有这个地方专门的优惠券。很抱歉要说这个，但没有优惠券，你就不能进行整形手术了...");
                }
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

            var option = await SayOption("我是助手Rinz。你有#b#t5154000##k、#b#t5150004##k、#b#t5150013##k或#b#t5151004##k吗？如果有的话，你觉得让我来给你打理发型怎么样？你想怎么处理你的头发呢？\r\n#L0#理发：#i5154000##t5154000##l\r\n#L1#理发：#i5150004##t5150004##l\r\n#L2#理发：#i5150013##t5150013##l\r\n#L3#染发：#i5151004##t5151004##l");
            if (option == 0)
            {
                // REG random hair
                var hairnew = new List<int>();
                var baseHair = getPlayer().Hair % 10;
                var hairs = getPlayer().Gender == 0 ? mhair_d : fhair_d;
                foreach (var h in hairs)
                {
                    hairnew.Add(h + baseHair);
                }
                if (await SayYesNo("如果你使用REG优惠券，您的发型将随机改变。你要使用#b#t5154000##k来真正改变您的发型吗？"))
                {
                    if (haveItem(5154000))
                    {
                        gainItem(5154000, -1);
                        var hair = Randomizer.Select(hairnew.ToArray());
                        setHair(hair);
                        await SayOK("享受你的新发型吧！");
                    }
                    else
                    {
                        await SayOK("嗯...看起来你没有#t5154000#...恐怕我不能给你理发。对不起...");
                    }
                }
            }
            else if (option == 1)
            {
                // EXP random hair
                var hairnew = new List<int>();
                var baseHair = getPlayer().Hair % 10;
                var hairs = getPlayer().Gender == 0 ? mhair_r : fhair_r;
                foreach (var h in hairs)
                {
                    hairnew.Add(h + baseHair);
                }
                if (await SayYesNo("如果你使用了经验值券，你的发型将会随机改变。你要使用 #b#t5150004##k 真的改变你的发型吗？"))
                {
                    if (haveItem(5150004))
                    {
                        gainItem(5150004, -1);
                        var hair = Randomizer.Select(hairnew.ToArray());
                        setHair(hair);
                        await SayOK("享受你的新发型吧！");
                    }
                    else
                    {
                        await SayOK("嗯...看起来你没有#t5150004#...恐怕我不能给你理发。对不起...");
                    }
                }
            }
            else if (option == 2)
            {
                // Random hair
                var hairnew = new List<int>();
                var baseHair = getPlayer().Hair % 10;
                var hairs = getPlayer().Gender == 0 ? mhair_e : fhair_e;
                foreach (var h in hairs)
                {
                    hairnew.Add(h + baseHair);
                }
                if (await SayYesNo("如果你使用普通的优惠券，你的发型将会随机改变。你还想使用 #b#t5150013##k 来改变吗？"))
                {
                    if (haveItem(5150013))
                    {
                        gainItem(5150013, -1);
                        var hair = Randomizer.Select(hairnew.ToArray());
                        setHair(hair);
                        await SayOK("享受你的新发型吧！");
                    }
                    else
                    {
                        await SayOK("嗯...看起来你没有#t5150013#...恐怕我不能给你理发。对不起...");
                    }
                }
            }
            else if (option == 3)
            {
                // Random color
                var haircolor = new List<int>();
                var current = (getPlayer().Hair / 10) * 10;
                for (int i = 0; i < 8; i++)
                {
                    haircolor.Add(current + i);
                }
                if (await SayYesNo("如果你使用普通的优惠券，你的发型将会随机改变。你还想使用 #b#t5151004##k 来改变吗？"))
                {
                    if (haveItem(5151004))
                    {
                        gainItem(5151004, -1);
                        var color = Randomizer.Select(haircolor.ToArray());
                        setHair(color);
                        await SayOK("享受你的新发色！");
                    }
                    else
                    {
                        await SayOK("嗯...看起来你没有#t5151004#...恐怕我不能给你染发。对不起...");
                    }
                }
            }
        }


        // Npc: 2012008 
        public async Task skin_orbis1()
        {
            int[] skinOptions = [0, 1, 2, 3, 4];

            var option = await SayOption("嗨，你好！欢迎来到奥比斯护肤中心~！你想要像我一样拥有紧致健康的皮肤吗？使用#b#t5153001##k，让我们来照顾你的肌肤，拥有你一直想要的肌肤吧~！\r\n#L0#护肤服务：#i5153001##t5153001##l");
            if (option == 0)
            {
                if (haveItem(5153001))
                {
                    var skinIdx = await SayStyle("通过我们的专业机器，你可以在手术前看到自己在治疗后的样子。你想要什么样的表情？快来选择你喜欢的风格吧～！", skinOptions);
                    gainItem(5153001, -1);
                    setSkin(skinOptions[skinIdx]);
                    await SayOK("享受你的新肤色吧！");
                }
                else
                {
                    await SayOK("嗯...你没有你需要接受治疗的护肤券。抱歉，恐怕我们不能为你做这个...");
                }
            }
        }


        // Npc: 2012009 
        public async Task face_orbis2()
        {
            int[] mface_r = [20003, 20011, 20021, 20022, 20023, 20027, 20031];
            int[] fface_r = [21004, 21007, 21010, 21012, 21020, 21021, 21030];

            var option = await SayOption("嗨，我其实不应该这样做，但是有了一个#b#t5152004##k，我还是会为你做。但别忘了，这将是随机的！\r\n#L0#整形手术：#i5152004##t5152004##l");
            if (option == 0)
            {
                var facenew = new List<int>();
                var baseFace = getPlayer().Face % 1000 - (getPlayer().Face % 100);
                var faces = getPlayer().Gender == 0 ? mface_r : fface_r;
                foreach (var f in faces)
                {
                    facenew.Add(f + baseFace);
                }
                if (await SayYesNo("如果你使用普通的优惠券，你的脸可能会变成一个随机的新样子……你还想用#b#t5152004##k来做吗？"))
                {
                    if (haveItem(5152004))
                    {
                        gainItem(5152004, -1);
                        var face = Randomizer.Select(facenew.ToArray());
                        setFace(face);
                        await SayOK("享受你的新面容吧！");
                    }
                    else
                    {
                        await SayOK("嗯...看起来你没有这个地方专门的优惠券。很抱歉要说这个，但没有优惠券，你就不能进行整形手术了...");
                    }
                }
            }
        }


        // Npc: 2100005 
        public async Task hair_ariant2()
        {
            int[] mhair_r = [30150, 30170, 30180, 30320, 30330, 30410, 30460, 30680, 30800, 30820, 30900];
            int[] fhair_r = [31090, 31190, 31330, 31340, 31400, 31420, 31520, 31620, 31650, 31660, 34000];

            var option = await SayOption("嘿！我是沙提，是马兹拉的学徒。如果你有 #b阿里安特发型券(REG)#k 或 #b阿里安特染发券(REG)#k，你愿意让我给你做头发吗？\r\n#L0#理发：#i5150026##t5150026##l\r\n#L1#染发：#i5151021##t5151021##l");
            if (option == 0)
            {
                // Random hair
                var hairnew = new List<int>();
                var baseHair = getPlayer().Hair % 10;
                var hairs = getPlayer().Gender == 0 ? mhair_r : fhair_r;
                foreach (var h in hairs)
                {
                    hairnew.Add(h + baseHair);
                }
                if (await SayYesNo("如果你使用REG优惠券，您的发型将随机改变。你要使用#b#t5150026##k来真正改变您的发型吗？"))
                {
                    if (haveItem(5150026))
                    {
                        gainItem(5150026, -1);
                        var hair = Randomizer.Select(hairnew.ToArray());
                        setHair(hair);
                        await SayOK("享受你的新发型吧！");
                    }
                    else
                    {
                        await SayOK("嗯...看起来你没有#t5150026#...恐怕我不能给你理发。对不起...");
                    }
                }
            }
            else if (option == 1)
            {
                // Random color
                var haircolor = new List<int>();
                var current = (getPlayer().Hair / 10) * 10;
                for (int i = 0; i < 8; i++)
                {
                    haircolor.Add(current + i);
                }
                if (await SayYesNo("如果你使用REG优惠券，你的发色将随机改变。你要使用#b#t5151021##k来真正改变你的发色吗？"))
                {
                    if (haveItem(5151021))
                    {
                        gainItem(5151021, -1);
                        var color = Randomizer.Select(haircolor.ToArray());
                        setHair(color);
                        await SayOK("享受你的新发色！");
                    }
                    else
                    {
                        await SayOK("嗯...看起来你没有#t5151021#...恐怕我不能给你染发。对不起...");
                    }
                }
            }
        }


        // Npc: 2100006 
        public async Task hair_ariant1()
        {
            int[] mhair_v = [30150, 30170, 30180, 30320, 30330, 30410, 30460, 30820, 30900];
            int[] fhair_v = [31040, 31090, 31190, 31330, 31340, 31400, 31420, 31620, 31660];

            var option = await SayOption("哈哈哈...在沙漠中，一个人要注意自己的发型，需要有很多的风格和魅力。像你这样的人...如果你有#b阿里安特发型券（VIP）#k或#b阿里安特染发券（VIP）#k，我会给你的头发一个全新的造型。\r\n#L0#理发：#i5150027##t5150027##l\r\n#L1#染发：#i5151022##t5151022##l");
            if (option == 0)
            {
                // Hair cut
                var hairnew = new List<int>();
                var baseHair = getPlayer().Hair % 10;
                var hairs = getPlayer().Gender == 0 ? mhair_v : fhair_v;
                foreach (var h in hairs)
                {
                    hairnew.Add(h + baseHair);
                }
                var hairIdx = await SayStyle("我可以完全改变你的发型，你还没准备好接受改变吗？给我 #b#t5150027##k，剩下的事我来帮你处理，选你喜欢的风格吧！", hairnew.ToArray());
                if (haveItem(5150027))
                {
                    gainItem(5150027, -1);
                    setHair(hairnew[hairIdx]);
                    await SayOK("享受你的新发型吧！");
                }
                else
                {
                    await SayOK("嗯...看起来你没有我们指定的优惠券...恐怕我不能给你理发。对不起...");
                }
            }
            else if (option == 1)
            {
                // Hair color
                var haircolor = new List<int>();
                var current = (getPlayer().Hair / 10) * 10;
                for (int i = 0; i < 8; i++)
                {
                    haircolor.Add(current + i);
                }
                var colorIdx = await SayStyle("我可以完全改变你头发的颜色，你还没准备好接受改变吗？给我 #b#t5151022##k，剩下的我来负责，选你喜欢的颜色吧！", haircolor.ToArray());
                if (haveItem(5151022))
                {
                    gainItem(5151022, -1);
                    setHair(haircolor[colorIdx]);
                    await SayOK("享受你的新发色！");
                }
                else
                {
                    await SayOK("嗯...看起来你没有我们指定的优惠券...恐怕我不能给你染发。对不起...");
                }
            }
        }


        // Npc: 2100007 
        public async Task skin_ariant1()
        {
            int[] skinOptions = [0, 1, 2, 3, 4];

            await SayNext("Hohoh~ 欢迎欢迎。欢迎来到阿里安特护肤中心。你已经踏入了一家著名的护肤店，甚至连女王本人都经常光顾这个地方。如果你带着 #b阿里安特护肤优惠券#k，我们会照顾好你的其余事项。今天让我们来护理一下你的肌肤吧？");
            if (haveItem(5153007))
            {
                var skinIdx = await SayStyle("通过我们的专业机器，你可以在手术前看到自己在治疗后的样子。你想要什么样的表情？快来选择你喜欢的风格吧～！", skinOptions);
                gainItem(5153007, -1);
                setSkin(skinOptions[skinIdx]);
                await SayOK("享受你的新肤色吧！");
            }
            else
            {
                await SayNext("嗯...我觉得你没有我们的护肤券。没有券，我就不能给你护理服务。");
            }
        }


        // Npc: 2100008 
        public async Task face_ariant1()
        {
            int[] mface_v = [20000, 20004, 20005, 20012, 20013, 20031];
            int[] fface_v = [21000, 21003, 21006, 21009, 21012, 21024];

            var option = await SayOption("啊，欢迎来到阿里安特整形中心！您想将您的脸变成全新的样子吗？通过使用#b#t5152030##k或者#b#t5152047##k，我可以让您的脸变得更好看！\r\n#L0#整形手术：#i5152030##t5152030##l\r\n#L1#美瞳：#i5152047##t5152047##l\r\n#L2#一次性美瞳：#i5152101#（任何颜色）#l");
            if (option == 0)
            {
                // Face surgery
                var facenew = new List<int>();
                var baseFace = getPlayer().Face % 1000 - (getPlayer().Face % 100);
                var faces = getPlayer().Gender == 0 ? mface_v : fface_v;
                foreach (var f in faces)
                {
                    facenew.Add(f + baseFace);
                }
                var faceIdx = await SayStyle("嗯......即使在遮蔽和燃烧的沙漠下，美丽的面孔也会发光。选择你想要的面孔，我会拿出我杰出的技艺来进行美化。", facenew.ToArray());
                if (haveItem(5152030))
                {
                    gainItem(5152030, -1);
                    setFace(facenew[faceIdx]);
                    await SayOK("享受你的新面容吧！");
                }
                else
                {
                    await SayNext("嗯...你似乎没有这家医院的专属优惠券。没有优惠券，恐怕我不能为你办理。");
                }
            }
            else if (option == 1)
            {
                // Lens
                var current = getPlayer().Face % 100 + (getPlayer().Gender == 0 ? 20000 : 21000);
                var colors = new List<int> { current, current + 100, current + 300, current + 600, current + 700 };
                var colorIdx = await SayStyle("我们将用新的镜头使您的眼睛更加明亮，与沙漠中闪闪发光的沙子相匹配，沙漠高兴地拥抱着宫殿的屋顶。选择您想要使用的...", colors.ToArray());
                if (haveItem(5152047))
                {
                    gainItem(5152047, -1);
                    setFace(colors[colorIdx]);
                    await SayOK("享受你的新款和升级版的隐形眼镜吧！");
                }
                else
                {
                    await SayOK("嗯...看起来你没有这个地方专门的优惠券。很抱歉要说这个，但没有优惠券，你就不能进行整形手术了...");
                }
            }
            else if (option == 2)
            {
                // One-time lens
                var current = getPlayer().Face % 100 + (getPlayer().Gender == 0 ? 20000 : 21000);
                var colors = new List<int>();
                for (int i = 0; i < 8; i++)
                {
                    if (haveItem(5152100 + i))
                    {
                        colors.Add(current + 100 * i);
                    }
                }
                if (colors.Count == 0)
                {
                    await SayOK("你没有任何一次性化妆镜片可供使用。");
                    return;
                }
                var colorIdx = await SayStyle("你想戴什么样的眼镜？请选择您喜欢的风格。", colors.ToArray());
                var color = (colors[colorIdx] / 100) % 10;
                if (haveItem(5152100 + color))
                {
                    gainItem(5152100 + color, -1);
                    setFace(colors[colorIdx]);
                    await SayOK("享受你的新款和升级版的隐形眼镜吧！");
                }
                else
                {
                    await SayOK("对不起，但我觉得你现在没有我们的化妆镜片优惠券。没有优惠券，恐怕我不能为你做。");
                }
            }
        }


        // Npc: 2100009 
        public async Task face_ariant2()
        {
            int[] mface_r = [20001, 20003, 20009, 20010, 20025, 20031];
            int[] fface_r = [21002, 21009, 21011, 21013, 21016, 21029, 21030];

            var option = await SayOption("嗨，我是这里的整容助理医生。用一个#b#t5152029##k或者一个#b#t5152048##k，我可以让它变得完美，相信我。啊，别忘了，手术后的结果是随机的！那么，你要做哪个呢？\r\n#L0#整形手术：#i5152029##t5152029##l\r\n#L1#美瞳：#i5152048##t5152048##l");
            if (option == 0)
            {
                // Face surgery (random)
                var facenew = new List<int>();
                var baseFace = getPlayer().Face % 1000 - (getPlayer().Face % 100);
                var faces = getPlayer().Gender == 0 ? mface_r : fface_r;
                foreach (var f in faces)
                {
                    facenew.Add(f + baseFace);
                }
                var confirm = await SayYesNo("如果你使用普通的优惠券，你的脸可能会变成一个随机的新样子...你还想用#b#t5152029##k来做吗？");
                if (confirm)
                {
                    if (haveItem(5152029))
                    {
                        gainItem(5152029, -1);
                        setFace(Randomizer.Select(facenew));
                        await SayOK("享受你的新面容吧！");
                    }
                    else
                    {
                        await SayNext("嗯...看起来你没有这个地方特定的优惠券...很抱歉要说这个，但没有优惠券的话，你就不能进行整形手术了。");
                    }
                }
            }
            else if (option == 1)
            {
                // Lens (random)
                var current = getPlayer().Face % 100 + (getPlayer().Gender == 0 ? 20000 : 21000);
                var colors = new List<int> { current, current + 100, current + 300, current + 600, current + 700 };
                var confirm = await SayYesNo("如果你使用普通优惠券，你将获得一副随机的化妆隐形眼镜。你打算使用#b#t5152048##k，真的改变你的眼睛吗？");
                if (confirm)
                {
                    if (haveItem(5152048))
                    {
                        gainItem(5152048, -1);
                        setFace(Randomizer.Select(colors));
                        await SayOK("享受你的新款和升级版的隐形眼镜吧！");
                    }
                    else
                    {
                        await SayOK("嗯...看起来你没有这个地方专门的优惠券。很抱歉要说这个，但没有优惠券，你就不能进行整形手术了...");
                    }
                }
            }
        }

        // Npc: 1052100 
        public async Task hair_kerning1()
        {
            int[] mhair_v = [30040, 30130, 30780, 30850, 30860, 30920, 33040];
            int[] fhair_v = [31090, 31140, 31330, 31440, 31760, 31880, 34050];

            var option = await SayOption("你好！我是美容院的院长唐·乔瓦尼！如果你有#b#t5150003##k或者#b#t5151003##k，为什么不让我来照顾剩下的事情呢？决定你想要怎么处理你的头发...\r\n#L0#理发：#i5150003##t5150003##l\r\n#L1#染发：#i5151003##t5151003##l");
            if (option == 0)
            {
                // Hair change
                var hairnew = new List<int>();
                var baseHair = getPlayer().Hair % 10;
                var hairs = getPlayer().Gender == 0 ? mhair_v : fhair_v;
                foreach (var h in hairs)
                {
                    hairnew.Add(h + baseHair);
                }
                var hairIdx = await SayStyle("我完全可以改变你的发型，让它看起来好极了。你为什么不改一下呢？如果你有 #b#t5150003##k 就可以。选一个自己喜欢的吧~。", hairnew.ToArray());
                if (haveItem(5150003))
                {
                    gainItem(5150003, -1);
                    setHair(hairnew[hairIdx]);
                    await SayOK("享受你的新发型吧！");
                }
                else
                {
                    await SayOK("嗯...看起来你没有我们指定的优惠券...恐怕我不能给你理发。对不起...");
                }
            }
            else if (option == 1)
            {
                // Hair color
                var haircolor = new List<int>();
                var current = (getPlayer().Hair / 10) * 10;
                for (int i = 0; i < 8; i++)
                {
                    haircolor.Add(current + i);
                }
                var colorIdx = await SayStyle("我完全可以改变你的发色，让它看起来那么好。你为什么不改一下呢？只需要 #b#t5151003##k 就可以。选一个自己喜欢的吧~。", haircolor.ToArray());
                if (haveItem(5151003))
                {
                    gainItem(5151003, -1);
                    setHair(haircolor[colorIdx]);
                    await SayOK("享受你的新发色吧！");
                }
                else
                {
                    await SayOK("嗯...看起来你没有我们指定的优惠券...恐怕我不能给你染发。对不起...");
                }
            }
        }


        // Npc: 1052101 
        public async Task hair_kerning2()
        {
            int[] mhair_r = [30040, 30130, 30520, 30770, 30780, 30850, 30920, 33040];
            int[] fhair_r = [31060, 31140, 31330, 31440, 31520, 31750, 31760, 31880, 34050];
            int[] mhair_e = [30130, 30430, 30520, 30770, 30780, 30850, 30920, 33040];
            int[] fhair_e = [31060, 31140, 31330, 31520, 31760, 31880, 34010, 34050];

            var option = await SayOption("我是安德烈，唐的助手。不过大家都叫我安德烈。如果你有#b#t5150002##k、#b#t5150011##k或者#b#t5151002##k，请让我来给你换个发型吧！\r\n#L0#理发：#i5150002##t5150002##l\r\n#L1#理发：#i5150011##t5150011##l\r\n#L2#染发：#i5151002##t5151002##l");
            if (option == 0)
            {
                // Random hair (REG coupon)
                var hairnew = new List<int>();
                var baseHair = getPlayer().Hair % 10;
                var hairs = getPlayer().Gender == 0 ? mhair_r : fhair_r;
                foreach (var h in hairs)
                {
                    hairnew.Add(h + baseHair);
                }
                var confirm = await SayYesNo("如果您使用#t5150011#，您的发型将随机改变，并有机会获得我设计的新实验风格。您要使用#b#t5150011##k来真正改变您的发型吗？");
                if (confirm)
                {
                    if (haveItem(5150011))
                    {
                        gainItem(5150011, -1);
                        setHair(Randomizer.Select(hairnew));
                        await SayOK("享受你的新发型吧！");
                    }
                    else
                    {
                        await SayOK("嗯...看起来你没有我们指定的优惠券...恐怕我不能给你理发。对不起...");
                    }
                }
            }
            else if (option == 1)
            {
                // Experimental hair (EXP coupon)
                var hairnew = new List<int>();
                var baseHair = getPlayer().Hair % 10;
                var hairs = getPlayer().Gender == 0 ? mhair_e : fhair_e;
                foreach (var h in hairs)
                {
                    hairnew.Add(h + baseHair);
                }
                var confirm = await SayYesNo("如果你使用了#t5150002#，你的发型将会随机改变，并有机会获得我设计的新实验性发型。你要使用 #b#t5150011##k 真的改变你的发型吗？");
                if (confirm)
                {
                    if (haveItem(5150002))
                    {
                        gainItem(5150002, -1);
                        setHair(Randomizer.Select(hairnew));
                        await SayOK("享受你的新发型吧！");
                    }
                    else
                    {
                        await SayOK("嗯...看起来你没有我们指定的优惠券...恐怕我不能给你理发。对不起...");
                    }
                }
            }
            else if (option == 2)
            {
                // Random hair color
                var haircolor = new List<int>();
                var current = (getPlayer().Hair / 10) * 10;
                for (int i = 0; i < 8; i++)
                {
                    haircolor.Add(current + i);
                }
                var confirm = await SayYesNo("如果你使用普通的优惠券，你的发型将会随机改变。你还想使用 #b#t5151002##k 来改变吗？");
                if (confirm)
                {
                    if (haveItem(5151002))
                    {
                        gainItem(5151002, -1);
                        setHair(Randomizer.Select(haircolor));
                        await SayOK("享受你的新发色吧！");
                    }
                    else
                    {
                        await SayOK("嗯...看起来你没有我们指定的优惠券...恐怕我不能给你染发。很抱歉...");
                    }
                }
            }
        }

        // Npc: 2090100 
        public Task hair_mureung1()
        {
            // TODO
            return Task.CompletedTask;
        }


        // Npc: 2090101 
        public Task hair_mureung2()
        {
            // TODO
            return Task.CompletedTask;
        }


        // Npc: 2090102 
        public Task skin_mureung1()
        {
            // TODO
            return Task.CompletedTask;
        }


        // Npc: 2090103 
        public Task face_mureung1()
        {
            // TODO
            return Task.CompletedTask;
        }


        // Npc: 2090104 
        public Task face_mureung2()
        {
            // TODO
            return Task.CompletedTask;
        }


        // Npc: 9270023 
        public Task face_sg2()
        {
            // TODO
            return Task.CompletedTask;
        }


        // Npc: 9270024 
        public Task face_sg1()
        {
            // TODO
            return Task.CompletedTask;
        }


        // Npc: 9270025 
        public Task skin_sg1()
        {
            // TODO
            return Task.CompletedTask;
        }


        // Npc: 9270026 
        public Task lens_sg1()
        {
            // TODO
            return Task.CompletedTask;
        }


        // Npc: 9270033 
        public Task captinsg01()
        {
            // TODO
            return Task.CompletedTask;
        }


        // Npc: 9270036 
        public Task hair_sg1()
        {
            // TODO
            return Task.CompletedTask;
        }


        // Npc: 9270037 
        public Task hair_sg2()
        {
            // TODO
            return Task.CompletedTask;
        }

        // Npc: 9201039 
        public Task hair_wedding3()
        {
            // TODO
            return Task.CompletedTask;
        }

        // Npc: 9201061 
        public Task NLC_LensExp()
        {
            // TODO
            return Task.CompletedTask;
        }


        // Npc: 9201062 
        public Task NLC_LensVip()
        {
            // TODO
            return Task.CompletedTask;
        }


        // Npc: 9201063 
        public Task NLC_HairExp()
        {
            // TODO
            return Task.CompletedTask;
        }


        // Npc: 9201064 
        public Task NLC_HairVip()
        {
            // TODO
            return Task.CompletedTask;
        }


        // Npc: 9201065 
        public Task NLC_Skin()
        {
            // TODO
            return Task.CompletedTask;
        }

        // Npc: 9200100 
        public Task lens_henesys1()
        {
            // TODO
            return Task.CompletedTask;
        }


        // Npc: 9200101 
        public Task lens_orbis1()
        {
            // TODO
            return Task.CompletedTask;
        }


        // Npc: 9200102 
        public Task lens_ludi1()
        {
            // TODO
            return Task.CompletedTask;
        }





        // Npc: 9201015 
        public Task hair_wedding1()
        {
            // TODO
            return Task.CompletedTask;
        }


        // Npc: 9201016 
        public Task hair_wedding2()
        {
            // TODO
            return Task.CompletedTask;
        }


        // Npc: 9201017 
        public Task lens_wedding1()
        {
            // TODO
            return Task.CompletedTask;
        }


        // Npc: 9201018 
        public Task face_wedding1()
        {
            // TODO
            return Task.CompletedTask;
        }


        // Npc: 9201019 
        public Task face_wedding2()
        {
            // TODO
            return Task.CompletedTask;
        }

        // Npc: 9120100 
        public Task hair_shouwa1()
        {
            // TODO
            return Task.CompletedTask;
        }


        // Npc: 9120101 
        public Task hair_shouwa2()
        {
            // TODO
            return Task.CompletedTask;
        }


        // Npc: 9120102 
        public Task face_shouwa1()
        {
            // TODO
            return Task.CompletedTask;
        }


        // Npc: 9120103 
        public Task face_shouwa2()
        {
            // TODO
            return Task.CompletedTask;
        }
    }
}
