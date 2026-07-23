using System;
using System.Runtime.Serialization;
using Application.Core.Game.Players;
using Application.Core.Mappers;
using Application.Shared.Constants.Inventory;
using Application.Shared.Constants.Job;
using Dto;
using Google.Protobuf.WellKnownTypes;

namespace Application.Core.Mappers
{
    public partial class PlayerMapper : IPlayerMapper
    {
        public CharacterDto MapToDto(Player p1)
        {
            return p1 == null ? null : new CharacterDto()
            {
                Id = p1.Id,
                AccountId = p1.AccountId,
                Name = p1.Name,
                Level = p1.Level,
                Exp = p1.ExpValue.get(),
                Gachaexp = p1.GachaExpValue.get(),
                Str = p1.Str,
                Dex = p1.Dex,
                Luk = p1.Luk,
                Int = p1.Int,
                Hp = p1.HP,
                Mp = p1.MP,
                Maxhp = p1.MaxHP,
                Maxmp = p1.MaxMP,
                Meso = p1.MesoValue.get(),
                HpMpUsed = p1.HpMpUsed,
                JobId = p1.JobId,
                Skincolor = p1.Skincolor,
                Gender = p1.Gender,
                Fame = p1.Fame,
                Fquest = p1.Fquest,
                Hair = p1.Hair,
                Face = p1.Face,
                Ap = p1.Ap,
                Sp = string.Join<int>(",", p1.RemainingSp),
                Party = p1.Party,
                BuddyCapacity = funcMain1(p1.BuddyList != null ? (int?)p1.BuddyList.Capacity : null),
                CreateDate = Timestamp.FromDateTimeOffset(p1.CreateDate),
                Rank = p1.Rank,
                RankMove = p1.RankMove,
                JobRank = p1.JobRank,
                JobRankMove = p1.JobRankMove,
                GuildId = p1.GuildId,
                GuildRank = p1.GuildRank,
                MountLevel = p1.MountModel == null ? 1 : p1.MountModel.getLevel(),
                MountExp = p1.MountModel == null ? 0 : p1.MountModel.getExp(),
                Mounttiredness = p1.MountModel == null ? 0 : p1.MountModel.getTiredness(),
                Omokwins = p1.Omokwins,
                Omoklosses = p1.Omoklosses,
                Omokties = p1.Omokties,
                Matchcardwins = p1.Matchcardwins,
                Matchcardlosses = p1.Matchcardlosses,
                Matchcardties = p1.Matchcardties,
                Equipslots = (int)p1.Bag[InventoryType.EQUIP].getSlotLimit(),
                Useslots = (int)p1.Bag[InventoryType.USE].getSlotLimit(),
                Setupslots = (int)p1.Bag[InventoryType.SETUP].getSlotLimit(),
                Etcslots = (int)p1.Bag[InventoryType.ETC].getSlotLimit(),
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
        public Player MapToExisting(CharacterDto p3, Player p4)
        {
            if (p3 == null)
            {
                return null;
            }
            Player result = p4 ?? (Player)FormatterServices.GetUninitializedObject(typeof(Player));
            
            result.JobModel = JobFactory.GetById(p3.JobId);
            result.PendantOfSpiritEquippedTime = p3.PendantOfSpiritEquippedTime;
            result.Id = p3.Id;
            result.AccountId = p3.AccountId;
            result.Name = p3.Name;
            result.Level = p3.Level;
            result.Str = p3.Str;
            result.Dex = p3.Dex;
            result.Luk = p3.Luk;
            result.Int = p3.Int;
            result.HpMpUsed = p3.HpMpUsed;
            result.Skincolor = p3.Skincolor;
            result.Gender = p3.Gender;
            result.Fame = p3.Fame;
            result.Fquest = p3.Fquest;
            result.Hair = p3.Hair;
            result.Face = p3.Face;
            result.Ap = p3.Ap;
            result.Map = p3.Map;
            result.Party = p3.Party;
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
            result.Reborns = p3.Reborns;
            result.Pqpoints = p3.Pqpoints;
            result.DataString = p3.DataString;
            result.LastLogoutTime = p3.LastLogoutTime.ToDateTimeOffset();
            result.LastExpGainTime = p3.LastExpGainTime.ToDateTimeOffset();
            result.PartySearch = p3.PartySearch;
            result.Jailexpire = p3.Jailexpire;
            result.HpAlert = p3.HpAlert;
            result.MpAlert = p3.MpAlert;
            result.RemainingSp = funcMain2(ProtoMapper.TranslateArray(p3.Sp), result.RemainingSp);
            return result;
            
        }
        
        private int funcMain1(int? p2)
        {
            return p2 == null ? 0 : (int)p2;
        }
        
        private int[] funcMain2(int[] p5, int[] p6)
        {
            if (p5 == null)
            {
                return null;
            }
            int[] result = new int[p5.Length];
            Array.Copy(p5, 0, result, 0, p5.Length);
            return result;
            
        }
    }
}