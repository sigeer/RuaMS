using Application.Core.Channel.Net.Packets;
using Application.Core.Game.Maps;

namespace Application.Core.Game.Life
{
    public class PlayerNpc : AbstractMapObject
    {
        public int Id { get; set; }

        public string Name { get; set; } = null!;

        public int Hair { get; set; }

        public int Face { get; set; }

        public int Skin { get; set; }

        public int Gender { get; set; }

        public int X { get; set; }

        public int Cy { get; set; }

        public int World { get; set; }

        public int Map { get; set; }

        public int Dir { get; set; }
        /// <summary>
        /// 虽然是基于玩家生成，但还是使用的已经预设的NpcId
        /// </summary>

        public int NpcId { get; set; }

        public int Fh { get; set; }

        public int Rx0 { get; set; }

        public int Rx1 { get; set; }

        public int OverallRank { get; set; }

        public int JobRank { get; set; }

        public int Job { get; set; }
        public int PlayerId { get; set; }

        public bool IsHonor { get; set; }
        public List<PlayerNpcEquipObject> Equips { get; set; } = [];

        public PlayerNpc()
        {

        }

        public override MapObjectType getType()
        {
            return MapObjectType.PLAYER_NPC;
        }

        public override void sendSpawnData(IChannelClient client)
        {
            client.sendPacket(PlayerNPCPacketCreator.SpawnPlayerNPCController(this));
            client.sendPacket(PlayerNPCPacketCreator.GetPlayerNPC(this));
        }

        public override void sendDestroyData(IChannelClient client)
        {
            client.sendPacket(PlayerNPCPacketCreator.RemoveNPCController(this.getObjectId()));
            client.sendPacket(PlayerNPCPacketCreator.RemovePlayerNPC(this.getObjectId()));
        }

        public override string GetName()
        {
            return Name;
        }

        public override int GetSourceId()
        {
            return NpcId;
        }

        public override int getObjectId()
        {
            return Id;
        }

        public override Point getPosition()
        {
            return new Point(X, Cy);
        }
        public void updatePlayerNPCPosition(IMap map, Point newPos)
        {
            setPosition(newPos);
            Rx0 = newPos.X + 50;
            Rx1 = newPos.X - 50;
            Cy = newPos.Y;
            Fh = map.Footholds.FindBelowFoothold(newPos)!.getId();
        }
    }
}
