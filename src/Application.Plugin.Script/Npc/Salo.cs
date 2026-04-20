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
        public Task hair_henesys1()
        {
            // TODO
            return Task.CompletedTask;
        }


        // Npc: 1012104 
        public Task hair_henesys2()
        {
            // TODO
            return Task.CompletedTask;
        }


        // Npc: 1012105 
        public Task skin_henesys1()
        {
            // TODO
            return Task.CompletedTask;
        }

        // Npc: 1052004 
        public Task face_henesys1()
        {
            // TODO
            return Task.CompletedTask;
        }


        // Npc: 1052005 
        public Task face_henesys2()
        {
            // TODO
            return Task.CompletedTask;
        }



        // Npc: 2041007 
        public Task hair_ludi1()
        {
            // TODO
            return Task.CompletedTask;
        }


        // Npc: 2041009 
        public Task hair_ludi2()
        {
            // TODO
            return Task.CompletedTask;
        }

        // Npc: 2041010 
        public Task face_ludi1()
        {
            // TODO
            return Task.CompletedTask;
        }


        // Npc: 2041013 
        public Task skin_ludi1()
        {
            // TODO
            return Task.CompletedTask;
        }

        // Npc: 2010001 
        public Task hair_orbis1()
        {
            // TODO
            return Task.CompletedTask;
        }


        // Npc: 2010002 
        public Task face_orbis1()
        {
            // TODO
            return Task.CompletedTask;
        }
        // Npc: 2012007 
        public Task hair_orbis2()
        {
            // TODO
            return Task.CompletedTask;
        }


        // Npc: 2012008 
        public Task skin_orbis1()
        {
            // TODO
            return Task.CompletedTask;
        }


        // Npc: 2012009 
        public Task face_orbis2()
        {
            // TODO
            return Task.CompletedTask;
        }


        // Npc: 2100005 
        public Task hair_ariant2()
        {
            // TODO
            return Task.CompletedTask;
        }


        // Npc: 2100006 
        public Task hair_ariant1()
        {
            // TODO
            return Task.CompletedTask;
        }


        // Npc: 2100007 
        public Task skin_ariant1()
        {
            // TODO
            return Task.CompletedTask;
        }


        // Npc: 2100008 
        public Task face_ariant1()
        {
            // TODO
            return Task.CompletedTask;
        }


        // Npc: 2100009 
        public Task face_ariant2()
        {
            // TODO
            return Task.CompletedTask;
        }

        // Npc: 1052100 
        public Task hair_kerning1()
        {
            // TODO
            return Task.CompletedTask;
        }


        // Npc: 1052101 
        public Task hair_kerning2()
        {
            // TODO
            return Task.CompletedTask;
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
