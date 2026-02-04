using server.life;
using server.maps;
using System;
using System.Collections.Generic;
using System.Text;
using tools;

namespace Application.Core.Channel.Commands
{
    internal class ReactorSummonBossCommand : IWorldChannelCommand
    {
        Reactor _reactor;
        int mobId;
        int x;
        int y;
        string bgm;
        string summonMessage;

        public ReactorSummonBossCommand(Reactor reactor, int mobId, int x, int y, string bgm, string summonMessage)
        {
            _reactor = reactor;
            this.mobId = mobId;
            this.x = x;
            this.y = y;
            this.bgm = bgm;
            this.summonMessage = summonMessage;
        }

        public void Execute(ChannelCommandContext ctx)
        {
            var map = _reactor.getMap();
            var monster = LifeFactory.Instance.GetMonsterTrust(mobId);
            monster.setPosition(new Point(x, y));
            map.spawnMonster(monster);
            map.broadcastMessage(PacketCreator.musicChange(bgm));
            map.LightBlue(summonMessage);
        }
    }
}
