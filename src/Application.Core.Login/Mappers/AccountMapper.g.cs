using System;
using Application.Core.Login.Mappers;
using Application.EF.Entities;
using Dto;
using Google.Protobuf;

namespace Application.Core.Login.Mappers
{
    public partial class AccountMapper : IAccountMapper
    {
        public AccountGameDto MapToDto(AccountEntity p1)
        {
            return p1 == null ? null : new AccountGameDto()
            {
                Id = p1.Id,
                NxCredit = p1.NxCredit,
                MaplePoint = p1.MaplePoint,
                NxPrepaid = p1.NxPrepaid,
                Data = funcMain1(AccountGameDataProto.Parser.ParseFrom(p1.Blob))
            };
        }
        public AccountEntity MapToExisting(AccountGameDto p3, AccountEntity p4)
        {
            if (p3 == null)
            {
                return null;
            }
            AccountEntity result = p4 ?? new AccountEntity();
            
            result.Id = p3.Id;
            result.NxCredit = p3.NxCredit;
            result.MaplePoint = p3.MaplePoint;
            result.NxPrepaid = p3.NxPrepaid;
            result.Blob = funcMain2(p3.Data.ToByteArray(), result.Blob);
            return result;
            
        }
        
        private AccountGameDataProto funcMain1(AccountGameDataProto p2)
        {
            return p2 == null ? null : new AccountGameDataProto()
            {
                Storage = p2.Storage == null ? null : new StorageDto()
                {
                    OwnerId = p2.Storage.OwnerId,
                    Slots = p2.Storage.Slots,
                    Meso = p2.Storage.Meso
                },
                QuickSlot = p2.QuickSlot == null ? null : new QuickSlotDto() {LongValue = p2.QuickSlot.LongValue}
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