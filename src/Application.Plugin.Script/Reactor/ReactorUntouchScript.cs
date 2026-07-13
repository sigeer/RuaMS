using Application.Core.Client;
using scripting.reactor;
using server.maps;

namespace Application.Plugin.Script
{
    internal class ReactorUntouchScript : ReactorActionManager
    {
        public ReactorUntouchScript(IChannelClient c, Reactor r) : base(c, r)
        {
        }

        private async Task ToggleFlame(string fid, string[] flames)
        {
            var eim = getEventInstance();
            if (eim == null) return;

            if (eim.getIntProperty(fid) == 1)
            {
                for (var i = 0; i < flames.Length; i++)
                {
                    await getMap().toggleEnvironment(flames[i]);
                }
            }
            eim.setIntProperty(fid, eim.getIntProperty(fid) - 1);
        }

        // Reactor: 6109013 
        public async Task glpqstrge()
        {
            var eim = getEventInstance();
            if (eim == null) return;

            var fid = "glpq_s";
            if (eim.getIntProperty(fid) == 5)
            {
                await mapMessage(6, "All stirges have disappeared.");
                await getMap().killAllMonsters();
                eim.setIntProperty(fid, 777);
            }
            eim.setIntProperty(fid, eim.getIntProperty(fid) - 1);

        }

        // Reactor: 6109014 
        public async Task glpqflame0() => await ToggleFlame("glpq_f0", ["a1", "a2", "b1", "b2", "c1", "c2"]);

        // Reactor: 6109021 
        public async Task glpqflame1() => await ToggleFlame("glpq_f1", ["a3", "a4", "a5", "b3", "b4", "b5", "c3", "c4", "c5"]);

        // Reactor: 6109022 
        public async Task glpqflame2() => await ToggleFlame("glpq_f2", ["a6", "a7", "b6", "b7", "c6", "c7"]);

        // Reactor: 6109023 
        public async Task glpqflame3() => await ToggleFlame("glpq_f3", ["d1", "d2", "e1", "e2", "f1", "f2"]);

        // Reactor: 6109024 
        public async Task glpqflame4() => await ToggleFlame("glpq_f4", ["d6", "d7", "e6", "e7", "f6", "f7"]);

        // Reactor: 6109025 
        public async Task glpqflame5() => await ToggleFlame("glpq_f5", ["g1", "g2", "h1", "h2", "i1", "i2"]);

        // Reactor: 6109026 
        public async Task glpqflame6() => await ToggleFlame("glpq_f6", ["g3", "g4", "g5", "h3", "h4", "h5", "i3", "i4", "i5"]);

        // Reactor: 6109027 
        public async Task glpqflame7() => await ToggleFlame("glpq_f7", ["g6", "g7", "h6", "h7", "i6", "i7"]);
    }
}
