using System;
using Application.Core.Login.Mappers;
using Application.EF.Entities;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using ProtoModel;

namespace Application.Core.Login.Mappers
{
    public partial class CharacterMapper : ICharacterMapper
    {
        public CharacterProto MapToDto(CharacterEntity p1)
        {
            return p1 == null ? null : new CharacterProto()
            {
                Id = p1.Id,
                AccountId = p1.AccountId,
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
                BuddyCapacity = p1.BuddyCapacity,
                CreateDate = Timestamp.FromDateTimeOffset(p1.CreateDate),
                Rank = p1.Rank,
                RankMove = p1.RankMove,
                JobRank = p1.JobRank,
                JobRankMove = p1.JobRankMove,
                GuildId = p1.GuildId,
                GuildRank = p1.GuildRank,
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
                Pqpoints = p1.Pqpoints,
                DataString = p1.DataString,
                LastLogoutTime = Timestamp.FromDateTimeOffset(p1.LastLogoutTime),
                LastExpGainTime = Timestamp.FromDateTimeOffset(p1.LastExpGainTime),
                PartySearch = p1.PartySearch,
                Jailexpire = p1.Jailexpire,
                HpAlert = p1.HpAlert,
                MpAlert = p1.MpAlert,
                Data = funcMain1(CharacterDataProto.Parser.ParseFrom(p1.Blob))
            };
        }
        public CharacterEntity MapToExisting(CharacterProto p3, CharacterEntity p4)
        {
            if (p3 == null)
            {
                return null;
            }
            CharacterEntity result = p4 ?? new CharacterEntity();
            
            result.Id = p3.Id;
            result.AccountId = p3.AccountId;
            result.Name = p3.Name;
            result.Level = p3.Level;
            result.Exp = p3.Exp;
            result.Gachaexp = p3.Gachaexp;
            result.Str = p3.Str;
            result.Dex = p3.Dex;
            result.Luk = p3.Luk;
            result.Int = p3.Int;
            result.Hp = p3.Hp;
            result.Mp = p3.Mp;
            result.Maxhp = p3.Maxhp;
            result.Maxmp = p3.Maxmp;
            result.Meso = p3.Meso;
            result.HpMpUsed = p3.HpMpUsed;
            result.JobId = p3.JobId;
            result.Skincolor = p3.Skincolor;
            result.Gender = p3.Gender;
            result.Fame = p3.Fame;
            result.Fquest = p3.Fquest;
            result.Hair = p3.Hair;
            result.Face = p3.Face;
            result.Ap = p3.Ap;
            result.Sp = p3.Sp;
            result.Map = p3.Map;
            result.Spawnpoint = p3.Spawnpoint;
            result.BuddyCapacity = p3.BuddyCapacity;
            result.CreateDate = p3.CreateDate.ToDateTimeOffset();
            result.Rank = p3.Rank;
            result.RankMove = p3.RankMove;
            result.JobRank = p3.JobRank;
            result.JobRankMove = p3.JobRankMove;
            result.GuildId = p3.GuildId;
            result.GuildRank = p3.GuildRank;
            result.MountLevel = p3.MountLevel;
            result.MountExp = p3.MountExp;
            result.Mounttiredness = p3.Mounttiredness;
            result.Omokwins = p3.Omokwins;
            result.Omoklosses = p3.Omoklosses;
            result.Omokties = p3.Omokties;
            result.Matchcardwins = p3.Matchcardwins;
            result.Matchcardlosses = p3.Matchcardlosses;
            result.Matchcardties = p3.Matchcardties;
            result.Equipslots = p3.Equipslots;
            result.Useslots = p3.Useslots;
            result.Setupslots = p3.Setupslots;
            result.Etcslots = p3.Etcslots;
            result.FamilyId = p3.FamilyId;
            result.Monsterbookcover = p3.Monsterbookcover;
            result.AllianceRank = p3.AllianceRank;
            result.VanquisherStage = p3.VanquisherStage;
            result.AriantPoints = p3.AriantPoints;
            result.DojoPoints = p3.DojoPoints;
            result.LastDojoStage = p3.LastDojoStage;
            result.FinishedDojoTutorial = p3.FinishedDojoTutorial;
            result.VanquisherKills = p3.VanquisherKills;
            result.SummonValue = p3.SummonValue;
            result.Pqpoints = p3.Pqpoints;
            result.DataString = p3.DataString;
            result.LastLogoutTime = p3.LastLogoutTime.ToDateTimeOffset();
            result.LastExpGainTime = p3.LastExpGainTime.ToDateTimeOffset();
            result.PartySearch = p3.PartySearch;
            result.Jailexpire = p3.Jailexpire;
            result.HpAlert = p3.HpAlert;
            result.MpAlert = p3.MpAlert;
            result.Blob = funcMain2(p3.Data.ToByteArray(), result.Blob);
            return result;
            
        }
        
        private CharacterDataProto funcMain1(CharacterDataProto p2)
        {
            return p2 == null ? null : new CharacterDataProto()
            {
                Bag = p2.Bag,
                GachaponStorage = p2.GachaponStorage == null ? null : new StorageProto()
                {
                    OwnerId = p2.GachaponStorage.OwnerId,
                    Slots = p2.GachaponStorage.Slots,
                    Meso = p2.GachaponStorage.Meso
                }
            };
        }
        
        private byte[] funcMain2(byte[] p5, byte[] p6)
        {
            if (p5 == null)
            {
                return null;
            }
            byte[] result = new byte[p5.Length];
            Array.Copy(p5, 0, result, 0, p5.Length);
            return result;
            
        }
    }
}