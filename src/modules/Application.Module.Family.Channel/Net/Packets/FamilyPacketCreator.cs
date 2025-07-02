using Application.Module.Family.Channel.Models;
using Application.Module.Family.Common;
using Application.Shared.Net;
using Application.Utility;

namespace Application.Module.Family.Channel.Net.Packets
{
    internal class FamilyPacketCreator
    {
        public static Packet loadFamily()
        {
            OutPacket p = OutPacket.create(SendOpcode.FAMILY_PRIVILEGE_LIST);
            var allItems = EnumClassCache<FamilyEntitlement>.Values;
            p.writeInt(allItems.Length);
            for (int i = 0; i < allItems.Length; i++)
            {
                FamilyEntitlement entitlement = allItems[i];
                p.writeByte(i <= 1 ? 1 : 2); //type
                p.writeInt(entitlement.getRepCost());
                p.writeInt(entitlement.getUsageLimit());
                p.writeString(entitlement.getName());
                p.writeString(entitlement.getDescription());
            }
            return p;
        }

        /**
         * Family Result Message
         * <p>
         * Possible values for <code>type</code>:<br>
         * 64: You cannot add this character as a junior.
         * 65: The name could not be found or is not online.
         * 66: You belong to the same family.
         * 67: You do not belong to the same family.<br>
         * 69: The character you wish to add as\r\na Junior must be in the same
         * map.<br>
         * 70: This character is already a Junior of another character.<br>
         * 71: The Junior you wish to add\r\nmust be at a lower rank.<br>
         * 72: The gap between you and your\r\njunior must be within 20 levels.<br>
         * 73: Another character has requested to add this character.\r\nPlease try
         * again later.<br>
         * 74: Another character has requested a summon.\r\nPlease try again
         * later.<br>
         * 75: The summons has failed. Your current location or state does not allow
         * a summons.<br>
         * 76: The family cannot extend more than 1000 generations from above and
         * below.<br>
         * 77: The Junior you wish to add\r\nmust be over Level 10.<br>
         * 78: You cannot add a Junior \r\nthat has requested to change worlds.<br>
         * 79: You cannot add a Junior \r\nsince you've requested to change
         * worlds.<br>
         * 80: Separation is not possible due to insufficient Mesos.\r\nYou will
         * need %d Mesos to\r\nseparate with a Senior.<br>
         * 81: Separation is not possible due to insufficient Mesos.\r\nYou will
         * need %d Mesos to\r\nseparate with a Junior.<br>
         * 82: The Entitlement does not apply because your level does not match the
         * corresponding area.<br>
         *
         * @param type The type
         * @return Family Result packet
         */
        public static Packet sendFamilyMessage(int type, int mesos)
        {
            OutPacket p = OutPacket.create(SendOpcode.FAMILY_RESULT);
            p.writeInt(type);
            p.writeInt(mesos);
            return p;
        }

        public static Packet getFamilyInfo(FamilyEntry? f)
        {
            if (f == null)
            {
                return getEmptyFamilyInfo();
            }

            OutPacket p = OutPacket.create(SendOpcode.FAMILY_INFO_RESULT);
            p.writeInt(f.Reputation); // cur rep left
            p.writeInt(f.TotalReputation); // tot rep left
            p.writeInt(f.TodayReputation); // todays rep
            p.writeShort(f.getJuniorCount()); // juniors added
            p.writeShort(2); // juniors allowed
            p.writeShort(0); //Unknown
            p.writeInt(f.Family.getLeader().Id); // Leader ID (Allows setting message)
            p.writeString(f.Family.getName());
            p.writeString(f.Family.getMessage()); //family message
            p.writeInt(FamilyEntitlement.All.Count); //Entitlement info count
            foreach (var entitlement in FamilyEntitlement.All.Values)
            {
                p.writeInt(entitlement.Value); //ID
                p.writeInt(f.getEntitlementUsageCount(entitlement)); //Used count
            }
            return p;
        }

        private static Packet getEmptyFamilyInfo()
        {
            OutPacket p = OutPacket.create(SendOpcode.FAMILY_INFO_RESULT);
            p.writeInt(0); // cur rep left
            p.writeInt(0); // tot rep left
            p.writeInt(0); // todays rep
            p.writeShort(0); // juniors added
            p.writeShort(2); // juniors allowed
            p.writeShort(0); //Unknown
            p.writeInt(0); // Leader ID (Allows setting message)
            p.writeString("");
            p.writeString(""); //family message
            p.writeInt(0);
            return p;
        }

        public static PedigreeResult BuildPedigree(
            List<FamilyEntry> allMembers,
            int currentCid)
        {
            var byId = allMembers.ToDictionary(m => m.Id);
            var childrenMap = allMembers
                .Where(m => m.SeniorId != 0)
                .GroupBy(m => m.SeniorId)
                .ToDictionary(g => g.Key, g => g.ToList());

            var parentMap = allMembers
                .Where(m => m.SeniorId != 0)
                .ToDictionary(m => m.Id, m => m.SeniorId);

            var result = new PedigreeResult();
            var ordered = result.OrderedMembers;

            if (!byId.TryGetValue(currentCid, out var entry))
                return result;

            // TotalSeniors
            int cid = currentCid;
            while (parentMap.TryGetValue(cid, out var parentId))
            {
                result.TotalSeniors++;
                cid = parentId;
            }

            // Leader (top-most ancestor)
            FamilyEntry? leader = null;
            cid = currentCid;
            while (parentMap.TryGetValue(cid, out var parentId2))
            {
                if (!byId.TryGetValue(parentId2, out var parent))
                    break;
                leader = parent;
                cid = parentId2;
            }
            if (leader != null)
                ordered.Add(leader);

            // Grandparent
            if (parentMap.TryGetValue(entry.Id, out var fatherId) &&
                parentMap.TryGetValue(fatherId, out var grandId) &&
                byId.TryGetValue(grandId, out var grand))
            {
                ordered.Add(grand);
            }

            // Parent
            FamilyEntry? parentNode = null;
            if (parentMap.TryGetValue(entry.Id, out var parentId3) &&
                byId.TryGetValue(parentId3, out parentNode))
            {
                ordered.Add(parentNode);

                // Siblings (parent's other juniors)
                if (childrenMap.TryGetValue(parentNode.Id, out var siblings))
                {
                    foreach (var s in siblings)
                    {
                        if (s.Id != entry.Id)
                            ordered.Add(s);
                    }
                }
            }

            // Juniors
            if (childrenMap.TryGetValue(entry.Id, out var juniors))
            {
                result.JuniorCount = juniors.Count;

                foreach (var child in juniors)
                {
                    ordered.Add(child);

                    // Super juniors
                    if (childrenMap.TryGetValue(child.Id, out var superJuniors))
                    {
                        foreach (var g in superJuniors)
                        {
                            ordered.Add(g);
                            int count = childrenMap.TryGetValue(g.Id, out var gg) ? gg.Count : 0;
                            result.SuperJuniors.Add((g.Id, count));
                        }
                    }
                }
            }

            return result;
        }

        public static Packet showPedigree(Models.Family family, int currentPlayerId)
        {
            OutPacket p = OutPacket.create(SendOpcode.FAMILY_CHART_RESULT);
            p.writeInt(currentPlayerId); //ID of viewed player's pedigree, can't be leader?

            var familyMembers = family.Members.Values.ToList();
            var pedigreeList = BuildPedigree(familyMembers, currentPlayerId);
            foreach (var item in pedigreeList.OrderedMembers)
            {
                addPedigreeEntry(p, item);
            }
            p.writeInt(2 + pedigreeList.SuperJuniors.Count); //member info count
            p.writeInt(-1);// 0 = total seniors, -1 = total members, otherwise junior count of ID
            p.writeInt(familyMembers.Count);
            p.writeInt(0);
            p.writeInt(pedigreeList.TotalSeniors); //client subtracts provided seniors
            foreach (var superJunior in pedigreeList.SuperJuniors)
            {
                p.writeInt(superJunior.ChrId);
                p.writeInt(superJunior.TotalJuniors);
            }
            p.writeInt(0); //another loop count (entitlements used)
                           //p.writeInt(1); //entitlement index
                           //p.writeInt(2); //times used
            p.writeShort(pedigreeList.JuniorCount >= 2 ? 0 : 2); //0 disables Add button (only if viewing own pedigree)
            return p;
        }

        private static void addPedigreeEntry(OutPacket p, FamilyEntry entry)
        {
            var isOnline = entry.Channel != 0;
            p.writeInt(entry.Id); //ID
            p.writeInt(entry.SeniorId); //parent ID
            p.writeShort(entry.JobId); //job id
            p.writeByte(entry.Level); //level
            p.writeBool(isOnline); //isOnline
            p.writeInt(entry.Reputation); //current rep
            p.writeInt(entry.TotalReputation); //total rep
            p.writeInt(entry.ReputationToSenior); //reps recorded to senior
            p.writeInt(entry.TodayReputation);
            p.writeInt(entry.Channel > 0 ? entry.Channel - 1 : -2);
            p.writeInt(isOnline ? (DateTimeOffset.UtcNow - DateTimeOffset.FromUnixTimeMilliseconds(entry.LoginTime)).Minutes : 0); //time online in minutes
            p.writeString(entry.Name); //name
        }

        public static Packet sendFamilyInvite(int playerId, string inviter)
        {
            OutPacket p = OutPacket.create(SendOpcode.FAMILY_JOIN_REQUEST);
            p.writeInt(playerId);
            p.writeString(inviter);
            return p;
        }

        public static Packet sendFamilySummonRequest(string familyName, string from)
        {
            OutPacket p = OutPacket.create(SendOpcode.FAMILY_SUMMON_REQUEST);
            p.writeString(from);
            p.writeString(familyName);
            return p;
        }

        public static Packet sendFamilyLoginNotice(string name, bool loggedIn)
        {
            OutPacket p = OutPacket.create(SendOpcode.FAMILY_NOTIFY_LOGIN_OR_LOGOUT);
            p.writeBool(loggedIn);
            p.writeString(name);
            return p;
        }

        public static Packet sendFamilyJoinResponse(bool accepted, string added)
        {
            OutPacket p = OutPacket.create(SendOpcode.FAMILY_JOIN_REQUEST_RESULT);
            p.writeByte(accepted ? 1 : 0);
            p.writeString(added);
            return p;
        }

        public static Packet getSeniorMessage(string name)
        {
            OutPacket p = OutPacket.create(SendOpcode.FAMILY_JOIN_ACCEPTED);
            p.writeString(name);
            p.writeInt(0);
            return p;
        }
    }
}
