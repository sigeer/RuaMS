using Application.Core.Game.Maps;
using Application.Core.Game.Relation;
using Application.Shared.Team;
using TeamProto;

namespace Application.Core.Channel.Net.Packets
{
    internal class TeamPacketCreator
    {
        public static Packet PartyCreated(int teamId, Player creator)
        {
            OutPacket p = OutPacket.create(SendOpcode.PARTY_OPERATION);
            p.writeByte(8);
            p.writeInt(teamId);

            var door = creator.getPlayerDoor();
            if (door != null)
            {
                DoorObject mdo = door.getAreaDoor();
                p.writeInt(mdo.getTo().getId());
                p.writeInt(mdo.getFrom().getId());
                p.writeInt(mdo.getPosition().X);
                p.writeInt(mdo.getPosition().Y);
            }
            else
            {
                p.writeInt(MapId.NONE);
                p.writeInt(MapId.NONE);
                p.writeInt(0);
                p.writeInt(0);
            }
            return p;
        }

        public static Packet partyInvite(int fromTeamId, string fromPlayerName)
        {
            OutPacket p = OutPacket.create(SendOpcode.PARTY_OPERATION);
            p.writeByte(4);
            p.writeInt(fromTeamId);
            p.writeString(fromPlayerName);
            p.writeByte(0);
            return p;
        }

        public static Packet partySearchInvite(Player from)
        {
            OutPacket p = OutPacket.create(SendOpcode.PARTY_OPERATION);
            p.writeByte(4);
            p.writeInt(from.getPartyId());
            p.writeString("PS: " + from.getName());
            p.writeByte(0);
            return p;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message">
        /// <para>10: A beginner can't create a party.</para>
        /// <para>1/5/6/11/14/19: Your request for a party didn't work due to an unexpected error.</para>
        /// <para>12: Quit as leader of the party. </para>
        /// <para>13: You have yet to join a party.</para>
        /// <para>16: Already have joined a party.</para>
        /// <para>17: The party you're trying to join is already in full capacity. </para>
        /// <para>19: Unable to find the requested character in this channel.</para>
        /// <para>25: Cannot kick another user in this map.</para>
        /// <para>28/29: Leadership can only be given to a party member in the vicinity.</para>
        /// <para>30: Change leadership only on same channel.</para>
        /// </param>
        /// <returns></returns>
        public static Packet partyStatusMessage(int message)
        {
            OutPacket p = OutPacket.create(SendOpcode.PARTY_OPERATION);
            p.writeByte(message);
            return p;
        }

        public static Packet BeginnerCannotCreateTeam() => partyStatusMessage(10);
        public static Packet AlreadInTeam() => partyStatusMessage(16);
        public static Packet TeamFullCapacity() => partyStatusMessage(17);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message">
        /// <para>21: Player is blocking any party invitations</para>
        /// <para>22: Player is taking care of another invitation</para>
        /// <para>23: Player have denied request to the party.</para>
        /// </param>
        /// <param name="charname"></param>
        /// <returns></returns>
        public static Packet partyStatusMessage(int message, string charname)
        {
            OutPacket p = OutPacket.create(SendOpcode.PARTY_OPERATION);
            p.writeByte(message);
            p.writeString(charname);
            return p;
        }

        private static void AddPartyStatus(WorldChannel forchannel, TeamDto party, OutPacket p)
        {
            List<(TeamMemberDto? RemotePlayer, Player? ChannelPlayer)> members = [];
            for (int i = 0; i < Limits.MaxTeamMember; i++)
            {
                if (i < party.Members.Count)
                    members.Add((party.Members[i], forchannel.getPlayerStorage().getCharacterById(party.Members[i].Id)));
                else
                    members.Add((null, null));
            }

            foreach (var partychar in members)
            {
                p.writeInt(partychar.RemotePlayer?.Id ?? 0);
            }
            foreach (var partychar in members)
            {
                p.writeFixedString(partychar.RemotePlayer?.Name ?? "");
            }
            foreach (var partychar in members)
            {
                p.writeInt(partychar.RemotePlayer?.Job ?? 0);
            }
            foreach (var partychar in members)
            {
                p.writeInt(partychar.RemotePlayer?.Level ?? 0);
            }
            foreach (var partychar in members)
            {
                if (partychar.ChannelPlayer?.Channel > 0)
                    p.writeInt(partychar.ChannelPlayer.Channel - 1);
                else
                    p.writeInt(-2);
            }
            p.writeInt(party.LeaderId);
            foreach (var partychar in members)
            {
                p.writeInt(partychar.ChannelPlayer?.Map ?? 0);
            }

            foreach (var partychar in members)
            {
                var memberChr = partychar.Item2;
                if (memberChr != null)
                {
                    var door = memberChr.getPlayerDoor();
                    if (door != null)
                    {
                        DoorObject mdo = door.getTownDoor();
                        p.writeInt(mdo.getTown().getId());
                        p.writeInt(mdo.getArea().getId());
                        p.writeInt(mdo.getPosition().X);
                        p.writeInt(mdo.getPosition().Y);
                    }
                    else
                    {
                        p.writeInt(MapId.NONE);
                        p.writeInt(MapId.NONE);
                        p.writeInt(0);
                        p.writeInt(0);
                    }
                }
                else
                {
                    p.writeInt(MapId.NONE);
                    p.writeInt(MapId.NONE);
                    p.writeInt(0);
                    p.writeInt(0);
                }
            }
        }


        public static Packet UpdateParty(WorldChannel forChannel, TeamDto party, PartyOperation op, int targetId, string targetName)
        {
            OutPacket p = OutPacket.create(SendOpcode.PARTY_OPERATION);
            switch (op)
            {
                case PartyOperation.DISBAND:
                case PartyOperation.EXPEL:
                case PartyOperation.LEAVE:
                    p.writeByte(0x0C);
                    p.writeInt(party.Id);
                    p.writeInt(targetId);
                    if (op == PartyOperation.DISBAND)
                    {
                        p.writeByte(0);
                        p.writeInt(party.Id);
                    }
                    else
                    {
                        p.writeByte(1);
                        if (op == PartyOperation.EXPEL)
                        {
                            p.writeByte(1);
                        }
                        else
                        {
                            p.writeByte(0);
                        }
                        p.writeString(targetName);
                        AddPartyStatus(forChannel, party, p);
                    }
                    break;
                case PartyOperation.JOIN:
                    p.writeByte(0xF);
                    p.writeInt(party.Id);
                    p.writeString(targetName);
                    AddPartyStatus(forChannel, party, p);
                    break;
                case PartyOperation.SILENT_UPDATE:
                case PartyOperation.LOG_ONOFF:
                    p.writeByte(0x7);
                    p.writeInt(party.Id);
                    AddPartyStatus(forChannel, party, p);
                    break;
                case PartyOperation.CHANGE_LEADER:
                    p.writeByte(0x1B);
                    p.writeInt(targetId);
                    p.writeByte(0);
                    break;
            }
            return p;
        }

        public static Packet partyPortal(int townId, int targetId, Point position)
        {
            OutPacket p = OutPacket.create(SendOpcode.PARTY_OPERATION);
            p.writeShort(0x23);
            p.writeInt(townId);
            p.writeInt(targetId);
            p.writePos(position);
            return p;
        }

        public static Packet updatePartyMemberHP(int cid, int curhp, int maxhp)
        {
            OutPacket p = OutPacket.create(SendOpcode.UPDATE_PARTYMEMBER_HP);
            p.writeInt(cid);
            p.writeInt(curhp);
            p.writeInt(maxhp);
            return p;
        }
    }
}
