using Application.Core.Login.Mappers;
using Application.Core.Login.Models;
using Dto;
using Google.Protobuf.WellKnownTypes;

namespace Application.Core.Login.Mappers
{
    public partial class CharacterMapper : ICharacterMapper
    {
        public CharacterDto MapToDto(CharacterModel p1)
        {
            return p1 == null ? null : new CharacterDto()
            {
                Id = p1.Id,
                AccountId = p1.AccountId,
                World = p1.World,
                Name = p1.Name,
                Level = p1.Level,
                Exp = p1.Exp,
                Gachaexp = p1.Gachaexp,
                Str = p1.Str,
                Dex = p1.Dex,
                Luk = p1.Luk,
                Int = p1.Int,
                Hp = p1.Hp,
                Mp = p1.Mp,
                Maxhp = p1.Maxhp,
                Maxmp = p1.Maxmp,
                Meso = p1.Meso,
                HpMpUsed = p1.HpMpUsed,
                JobId = p1.JobId,
                Skincolor = p1.Skincolor,
                Gender = p1.Gender,
                Fame = p1.Fame,
                Fquest = p1.Fquest,
                Hair = p1.Hair,
                Face = p1.Face,
                Ap = p1.Ap,
                Sp = p1.Sp,
                Map = p1.Map,
                Spawnpoint = p1.Spawnpoint,
                Party = p1.Party,
                BuddyCapacity = p1.BuddyCapacity,
                CreateDate = Timestamp.FromDateTimeOffset(p1.CreateDate),
                Rank = p1.Rank,
                RankMove = p1.RankMove,
                JobRank = p1.JobRank,
                JobRankMove = p1.JobRankMove,
                GuildId = p1.GuildId,
                GuildRank = p1.GuildRank,
                MessengerId = p1.MessengerId,
                MessengerPosition = p1.MessengerPosition,
                MountLevel = p1.MountLevel,
                MountExp = p1.MountExp,
                Mounttiredness = p1.Mounttiredness,
                Omokwins = p1.Omokwins,
                Omoklosses = p1.Omoklosses,
                Omokties = p1.Omokties,
                Matchcardwins = p1.Matchcardwins,
                Matchcardlosses = p1.Matchcardlosses,
                Matchcardties = p1.Matchcardties,
                Equipslots = p1.Equipslots,
                Useslots = p1.Useslots,
                Setupslots = p1.Setupslots,
                Etcslots = p1.Etcslots,
                FamilyId = p1.FamilyId,
                Monsterbookcover = p1.Monsterbookcover,
                AllianceRank = p1.AllianceRank,
                VanquisherStage = p1.VanquisherStage,
                AriantPoints = p1.AriantPoints,
                DojoPoints = p1.DojoPoints,
                LastDojoStage = p1.LastDojoStage,
                FinishedDojoTutorial = p1.FinishedDojoTutorial,
                VanquisherKills = p1.VanquisherKills,
                SummonValue = p1.SummonValue,
                MarriageItemId = p1.MarriageItemId,
                Reborns = p1.Reborns,
                Pqpoints = p1.Pqpoints,
                DataString = p1.DataString,
                LastLogoutTime = Timestamp.FromDateTimeOffset(p1.LastLogoutTime),
                LastExpGainTime = Timestamp.FromDateTimeOffset(p1.LastExpGainTime),
                PartySearch = p1.PartySearch,
                Jailexpire = p1.Jailexpire,
                HpAlert = p1.HpAlert,
                MpAlert = p1.MpAlert,
                PendantOfSpiritEquippedTime = p1.PendantOfSpiritEquippedTime
            };
        }
        public CharacterModel MapToExisting(CharacterDto p2, CharacterModel p3)
        {
            if (p2 == null)
            {
                return null;
            }
            CharacterModel result = p3 ?? new CharacterModel();
            
            result.Id = p2.Id;
            result.AccountId = p2.AccountId;
            result.World = p2.World;
            result.Name = p2.Name;
            result.Level = p2.Level;
            result.Exp = p2.Exp;
            result.Gachaexp = p2.Gachaexp;
            result.Str = p2.Str;
            result.Dex = p2.Dex;
            result.Luk = p2.Luk;
            result.Int = p2.Int;
            result.Hp = p2.Hp;
            result.Mp = p2.Mp;
            result.Maxhp = p2.Maxhp;
            result.Maxmp = p2.Maxmp;
            result.Meso = p2.Meso;
            result.HpMpUsed = p2.HpMpUsed;
            result.JobId = p2.JobId;
            result.Skincolor = p2.Skincolor;
            result.Gender = p2.Gender;
            result.Fame = p2.Fame;
            result.Fquest = p2.Fquest;
            result.Hair = p2.Hair;
            result.Face = p2.Face;
            result.Ap = p2.Ap;
            result.Sp = p2.Sp;
            result.Map = p2.Map;
            result.Spawnpoint = p2.Spawnpoint;
            result.BuddyCapacity = p2.BuddyCapacity;
            result.CreateDate = p2.CreateDate.ToDateTimeOffset();
            result.Rank = p2.Rank;
            result.RankMove = p2.RankMove;
            result.JobRank = p2.JobRank;
            result.JobRankMove = p2.JobRankMove;
            result.MessengerId = p2.MessengerId;
            result.MessengerPosition = p2.MessengerPosition;
            result.MountLevel = p2.MountLevel;
            result.MountExp = p2.MountExp;
            result.Mounttiredness = p2.Mounttiredness;
            result.Omokwins = p2.Omokwins;
            result.Omoklosses = p2.Omoklosses;
            result.Omokties = p2.Omokties;
            result.Matchcardwins = p2.Matchcardwins;
            result.Matchcardlosses = p2.Matchcardlosses;
            result.Matchcardties = p2.Matchcardties;
            result.Equipslots = p2.Equipslots;
            result.Useslots = p2.Useslots;
            result.Setupslots = p2.Setupslots;
            result.Etcslots = p2.Etcslots;
            result.FamilyId = p2.FamilyId;
            result.Monsterbookcover = p2.Monsterbookcover;
            result.VanquisherStage = p2.VanquisherStage;
            result.AriantPoints = p2.AriantPoints;
            result.DojoPoints = p2.DojoPoints;
            result.LastDojoStage = p2.LastDojoStage;
            result.FinishedDojoTutorial = p2.FinishedDojoTutorial;
            result.VanquisherKills = p2.VanquisherKills;
            result.SummonValue = p2.SummonValue;
            result.MarriageItemId = p2.MarriageItemId;
            result.Reborns = p2.Reborns;
            result.Pqpoints = p2.Pqpoints;
            result.DataString = p2.DataString;
            result.LastLogoutTime = p2.LastLogoutTime.ToDateTimeOffset();
            result.LastExpGainTime = p2.LastExpGainTime.ToDateTimeOffset();
            result.PartySearch = p2.PartySearch;
            result.HpAlert = p2.HpAlert;
            result.MpAlert = p2.MpAlert;
            result.PendantOfSpiritEquippedTime = p2.PendantOfSpiritEquippedTime;
            return result;
            
        }
    }
}