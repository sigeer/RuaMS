using Application.Core.Game.Relation;
using server;
using System.Net.NetworkInformation;
using tools;

namespace Application.Core.Game.Players
{
    public partial class Player
    {
        private Ring? marriageRing = null;

        public Ring? getMarriageRing()
        {
            return marriageRing;
        }

        public Ring? GetRingBySourceId(int sourceId)
        {
            foreach (Ring ring in getCrushRings())
            {
                if (ring.SourceId == sourceId)
                {
                    return ring;
                }
            }
            foreach (Ring ring in getFriendshipRings())
            {
                if (ring.SourceId == sourceId)
                {
                    return ring;
                }
            }

            if (marriageRing?.SourceId == sourceId)
            {
                return marriageRing;
            }

            return null;
        }

        public Ring? getRingById(long id)
        {
            foreach (Ring ring in getCrushRings())
            {
                if (ring.getRingId() == id)
                {
                    return ring;
                }
            }
            foreach (Ring ring in getFriendshipRings())
            {
                if (ring.getRingId() == id)
                {
                    return ring;
                }
            }

            if (marriageRing != null)
            {
                if (marriageRing.getRingId() == id)
                {
                    return marriageRing;
                }
            }

            return null;
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
            var guild = this.getGuild();
            if (guild != null)
            {
                guild.broadcast(PacketCreator.marriageMessage(0, Name));
            }
            // TODO: 结婚系统重构后处理
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
