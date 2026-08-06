using Application.Core.Game.Relation;

namespace Application.Core.Game.Players
{
    public partial class Player
    {
        private Ring? marriageRing = null;

        public Ring? getMarriageRing()
        {
            return marriageRing;
        }

        public ProtoModel.RingProto? GetRingBySourceId(int sourceId)
        {
            return Rings.FirstOrDefault(x => x.Id == sourceId);
        }

        public ProtoModel.RingProto? getRingById(long id)
        {
            return Rings.FirstOrDefault(x => x.RingId1 == id || x.RingId2 == id);
        }

        public void addMarriageRing(Ring? r)
        {
            marriageRing = r;
        }

        public bool hasJustMarried()
        {
            var eim = getEventInstance();
            if (eim != null)
            {
                var prop = eim.getProperty("groomId");

                if (prop != null)
                {
                    var curMapId = getMapId();
                    return (int.Parse(prop) == Id || eim.getIntProperty("brideId") == Id) &&
                            (curMapId == MapId.CHAPEL_WEDDING_ALTAR || curMapId == MapId.CATHEDRAL_WEDDING_ALTAR);
                }
            }

            return false;
        }

        public void broadcastMarriageMessage()
        {
            // TODO: 结婚系统重构后处理

            //var guild = this.getGuild();
            //if (guild != null)
            //{
            //    guild.broadcast(PacketCreator.marriageMessage(0, Name));
            //}

            //var family = this.getFamily();
            //if (family != null)
            //{
            //    family.broadcast(PacketCreator.marriageMessage(1, Name));
            //}
        }

        public void CheckMarriageData()
        {
            //if (MarriageItemId > 0 && PartnerId <= 0)
            //{
            //    MarriageItemId = -1;
            //}
            //else if (PartnerId > 0 && EffectMarriageId <= 0)
            //{
            //    MarriageItemId = -1;
            //    PartnerId = -1;
            //}
        }
    }
}
