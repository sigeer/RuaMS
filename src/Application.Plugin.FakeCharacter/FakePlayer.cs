using Application.Core.constants;
using Application.Core.Game.Maps;
using Application.Core.Game.Players;
using Application.Shared.Constants;
using Application.Shared.Net;
using SyncProto;
using System.Drawing;

namespace Application.Plugin.FakeCharacter
{
    public class FakePlayer : Player
    {
        Player Owner { get; }
        public FakePlayer(Player owner, IMap map, Point pos, int idx)
            : base(
                new FakeClient(),
                map,
                pos,
                new PlayerGetterDto { Character = new Dto.CharacterDto { 
                    Sp = "0,0,0,0,0,0",
                    Hp = NumericConfig.MaxHP,
                    Maxhp = NumericConfig.MaxHP,
                    Mp = NumericConfig.MaxHP,
                    Maxmp = NumericConfig.MaxMP,
                } }
            ) {
            Id = GetFakePlayerId( owner, idx );
            Name = $"假人${idx}号";
            Face = 20000;
            Hair = 30030;
            Party = -1;
            Owner = owner;
        }

        public override Player? Controller => Owner;
        public override int getObjectId()
        {
            return Id;
        }
        public override void sendPacket(Packet packet)
        {
            // 不存在客户端，不需要发送
        }

        public static int GetFakePlayerId(Player chr, int idx)
        {
            return 500000 + idx * 10000 + chr.Id;
        }

        protected override bool IsVisibleForPlayerWithoutRange(Player chr)
        {
            return Party == chr.Party;
        }

        public override void OnTick(long now)
        {
            
        }
    }
}
