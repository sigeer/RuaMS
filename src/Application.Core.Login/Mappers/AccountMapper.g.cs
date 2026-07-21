using Application.Core.Login.Mappers;
using Application.Core.Login.Models;
using Dto;
using Google.Protobuf.Collections;

namespace Application.Core.Login.Mappers
{
    public partial class AccountMapper : IAccountMapper
    {
        public AccountGame MapToObject(AccountGameDto p1)
        {
            return p1 == null ? null : new AccountGame()
            {
                Id = p1.Id,
                NxCredit = p1.NxCredit,
                MaplePoint = p1.MaplePoint,
                NxPrepaid = p1.NxPrepaid,
                QuickSlot = p1.QuickSlot == null ? null : new QuickSlotModel() {LongValue = p1.QuickSlot.LongValue},
                Storage = funcMain1(p1.Storage),
                CashExplorerItems = funcMain3(p1.CashExplorerItems),
                CashCygnusItems = funcMain4(p1.CashCygnusItems),
                CashAranItems = funcMain5(p1.CashAranItems),
                CashOverallItems = funcMain6(p1.CashOverallItems)
            };
        }
        
        private StorageModel funcMain1(StorageDto p2)
        {
            return p2 == null ? null : new StorageModel()
            {
                OwnerId = p2.OwnerId,
                Slots = (byte)p2.Slots,
                Meso = p2.Meso,
                Items = funcMain2(p2.Items)
            };
        }
        
        private ItemModel[] funcMain3(RepeatedField<ItemDto> p4)
        {
            if (p4 == null)
            {
                return null;
            }
            ItemModel[] result = new ItemModel[p4.Count];
            
            int v = 0;
            
            int i = 0;
            int len = p4.Count;
            
            while (i < len)
            {
                ItemDto item = p4[i];
                result[v++] = item == null ? null : new ItemModel()
                {
                    Type = (byte)item.Type,
                    Characterid = item.Characterid == null ? null : (int?)(int)item.Characterid,
                    Accountid = item.Accountid == null ? null : (int?)(int)item.Accountid,
                    Itemid = item.Itemid,
                    InventoryType = (sbyte)item.InventoryType,
                    Position = (short)item.Position,
                    Quantity = (short)item.Quantity,
                    Owner = item.Owner,
                    Flag = (short)item.Flag,
                    Expiration = item.Expiration,
                    GiftFrom = item.GiftFrom,
                    UniqueId = item.UniqueId,
                    EquipInfo = item.EquipInfo == null ? null : new EquipModel()
                    {
                        Id = item.EquipInfo.Id,
                        Upgradeslots = item.EquipInfo.Upgradeslots,
                        Level = (byte)item.EquipInfo.Level,
                        Str = item.EquipInfo.Str,
                        Dex = item.EquipInfo.Dex,
                        Int = item.EquipInfo.Int,
                        Luk = item.EquipInfo.Luk,
                        Hp = item.EquipInfo.Hp,
                        Mp = item.EquipInfo.Mp,
                        Watk = item.EquipInfo.Watk,
                        Matk = item.EquipInfo.Matk,
                        Wdef = item.EquipInfo.Wdef,
                        Mdef = item.EquipInfo.Mdef,
                        Acc = item.EquipInfo.Acc,
                        Avoid = item.EquipInfo.Avoid,
                        Hands = item.EquipInfo.Hands,
                        Speed = item.EquipInfo.Speed,
                        Jump = item.EquipInfo.Jump,
                        Locked = item.EquipInfo.Locked,
                        Vicious = item.EquipInfo.Vicious,
                        Itemlevel = (byte)item.EquipInfo.Itemlevel,
                        Itemexp = item.EquipInfo.Itemexp,
                        RingId = item.EquipInfo.RingId,
                        RingSourceInfo = item.EquipInfo.RingSourceInfo == null ? null : new RingSourceModel()
                        {
                            Id = item.EquipInfo.RingSourceInfo.Id,
                            ItemId = item.EquipInfo.RingSourceInfo.ItemId,
                            RingId1 = item.EquipInfo.RingSourceInfo.RingId1,
                            RingId2 = item.EquipInfo.RingSourceInfo.RingId2,
                            CharacterId1 = item.EquipInfo.RingSourceInfo.CharacterId1,
                            CharacterId2 = item.EquipInfo.RingSourceInfo.CharacterId2
                        }
                    },
                    PetInfo = item.PetInfo == null ? null : new PetModel()
                    {
                        Petid = item.PetInfo.Petid,
                        Name = item.PetInfo.Name,
                        Level = item.PetInfo.Level,
                        Closeness = item.PetInfo.Closeness,
                        Fullness = item.PetInfo.Fullness,
                        Summoned = item.PetInfo.Summoned,
                        Flag = item.PetInfo.Flag
                    },
                    Properties = item.Properties
                };
                i++;
            }
            return result;
            
        }
        
        private ItemModel[] funcMain4(RepeatedField<ItemDto> p5)
        {
            if (p5 == null)
            {
                return null;
            }
            ItemModel[] result = new ItemModel[p5.Count];
            
            int v = 0;
            
            int i = 0;
            int len = p5.Count;
            
            while (i < len)
            {
                ItemDto item = p5[i];
                result[v++] = item == null ? null : new ItemModel()
                {
                    Type = (byte)item.Type,
                    Characterid = item.Characterid == null ? null : (int?)(int)item.Characterid,
                    Accountid = item.Accountid == null ? null : (int?)(int)item.Accountid,
                    Itemid = item.Itemid,
                    InventoryType = (sbyte)item.InventoryType,
                    Position = (short)item.Position,
                    Quantity = (short)item.Quantity,
                    Owner = item.Owner,
                    Flag = (short)item.Flag,
                    Expiration = item.Expiration,
                    GiftFrom = item.GiftFrom,
                    UniqueId = item.UniqueId,
                    EquipInfo = item.EquipInfo == null ? null : new EquipModel()
                    {
                        Id = item.EquipInfo.Id,
                        Upgradeslots = item.EquipInfo.Upgradeslots,
                        Level = (byte)item.EquipInfo.Level,
                        Str = item.EquipInfo.Str,
                        Dex = item.EquipInfo.Dex,
                        Int = item.EquipInfo.Int,
                        Luk = item.EquipInfo.Luk,
                        Hp = item.EquipInfo.Hp,
                        Mp = item.EquipInfo.Mp,
                        Watk = item.EquipInfo.Watk,
                        Matk = item.EquipInfo.Matk,
                        Wdef = item.EquipInfo.Wdef,
                        Mdef = item.EquipInfo.Mdef,
                        Acc = item.EquipInfo.Acc,
                        Avoid = item.EquipInfo.Avoid,
                        Hands = item.EquipInfo.Hands,
                        Speed = item.EquipInfo.Speed,
                        Jump = item.EquipInfo.Jump,
                        Locked = item.EquipInfo.Locked,
                        Vicious = item.EquipInfo.Vicious,
                        Itemlevel = (byte)item.EquipInfo.Itemlevel,
                        Itemexp = item.EquipInfo.Itemexp,
                        RingId = item.EquipInfo.RingId,
                        RingSourceInfo = item.EquipInfo.RingSourceInfo == null ? null : new RingSourceModel()
                        {
                            Id = item.EquipInfo.RingSourceInfo.Id,
                            ItemId = item.EquipInfo.RingSourceInfo.ItemId,
                            RingId1 = item.EquipInfo.RingSourceInfo.RingId1,
                            RingId2 = item.EquipInfo.RingSourceInfo.RingId2,
                            CharacterId1 = item.EquipInfo.RingSourceInfo.CharacterId1,
                            CharacterId2 = item.EquipInfo.RingSourceInfo.CharacterId2
                        }
                    },
                    PetInfo = item.PetInfo == null ? null : new PetModel()
                    {
                        Petid = item.PetInfo.Petid,
                        Name = item.PetInfo.Name,
                        Level = item.PetInfo.Level,
                        Closeness = item.PetInfo.Closeness,
                        Fullness = item.PetInfo.Fullness,
                        Summoned = item.PetInfo.Summoned,
                        Flag = item.PetInfo.Flag
                    },
                    Properties = item.Properties
                };
                i++;
            }
            return result;
            
        }
        
        private ItemModel[] funcMain5(RepeatedField<ItemDto> p6)
        {
            if (p6 == null)
            {
                return null;
            }
            ItemModel[] result = new ItemModel[p6.Count];
            
            int v = 0;
            
            int i = 0;
            int len = p6.Count;
            
            while (i < len)
            {
                ItemDto item = p6[i];
                result[v++] = item == null ? null : new ItemModel()
                {
                    Type = (byte)item.Type,
                    Characterid = item.Characterid == null ? null : (int?)(int)item.Characterid,
                    Accountid = item.Accountid == null ? null : (int?)(int)item.Accountid,
                    Itemid = item.Itemid,
                    InventoryType = (sbyte)item.InventoryType,
                    Position = (short)item.Position,
                    Quantity = (short)item.Quantity,
                    Owner = item.Owner,
                    Flag = (short)item.Flag,
                    Expiration = item.Expiration,
                    GiftFrom = item.GiftFrom,
                    UniqueId = item.UniqueId,
                    EquipInfo = item.EquipInfo == null ? null : new EquipModel()
                    {
                        Id = item.EquipInfo.Id,
                        Upgradeslots = item.EquipInfo.Upgradeslots,
                        Level = (byte)item.EquipInfo.Level,
                        Str = item.EquipInfo.Str,
                        Dex = item.EquipInfo.Dex,
                        Int = item.EquipInfo.Int,
                        Luk = item.EquipInfo.Luk,
                        Hp = item.EquipInfo.Hp,
                        Mp = item.EquipInfo.Mp,
                        Watk = item.EquipInfo.Watk,
                        Matk = item.EquipInfo.Matk,
                        Wdef = item.EquipInfo.Wdef,
                        Mdef = item.EquipInfo.Mdef,
                        Acc = item.EquipInfo.Acc,
                        Avoid = item.EquipInfo.Avoid,
                        Hands = item.EquipInfo.Hands,
                        Speed = item.EquipInfo.Speed,
                        Jump = item.EquipInfo.Jump,
                        Locked = item.EquipInfo.Locked,
                        Vicious = item.EquipInfo.Vicious,
                        Itemlevel = (byte)item.EquipInfo.Itemlevel,
                        Itemexp = item.EquipInfo.Itemexp,
                        RingId = item.EquipInfo.RingId,
                        RingSourceInfo = item.EquipInfo.RingSourceInfo == null ? null : new RingSourceModel()
                        {
                            Id = item.EquipInfo.RingSourceInfo.Id,
                            ItemId = item.EquipInfo.RingSourceInfo.ItemId,
                            RingId1 = item.EquipInfo.RingSourceInfo.RingId1,
                            RingId2 = item.EquipInfo.RingSourceInfo.RingId2,
                            CharacterId1 = item.EquipInfo.RingSourceInfo.CharacterId1,
                            CharacterId2 = item.EquipInfo.RingSourceInfo.CharacterId2
                        }
                    },
                    PetInfo = item.PetInfo == null ? null : new PetModel()
                    {
                        Petid = item.PetInfo.Petid,
                        Name = item.PetInfo.Name,
                        Level = item.PetInfo.Level,
                        Closeness = item.PetInfo.Closeness,
                        Fullness = item.PetInfo.Fullness,
                        Summoned = item.PetInfo.Summoned,
                        Flag = item.PetInfo.Flag
                    },
                    Properties = item.Properties
                };
                i++;
            }
            return result;
            
        }
        
        private ItemModel[] funcMain6(RepeatedField<ItemDto> p7)
        {
            if (p7 == null)
            {
                return null;
            }
            ItemModel[] result = new ItemModel[p7.Count];
            
            int v = 0;
            
            int i = 0;
            int len = p7.Count;
            
            while (i < len)
            {
                ItemDto item = p7[i];
                result[v++] = item == null ? null : new ItemModel()
                {
                    Type = (byte)item.Type,
                    Characterid = item.Characterid == null ? null : (int?)(int)item.Characterid,
                    Accountid = item.Accountid == null ? null : (int?)(int)item.Accountid,
                    Itemid = item.Itemid,
                    InventoryType = (sbyte)item.InventoryType,
                    Position = (short)item.Position,
                    Quantity = (short)item.Quantity,
                    Owner = item.Owner,
                    Flag = (short)item.Flag,
                    Expiration = item.Expiration,
                    GiftFrom = item.GiftFrom,
                    UniqueId = item.UniqueId,
                    EquipInfo = item.EquipInfo == null ? null : new EquipModel()
                    {
                        Id = item.EquipInfo.Id,
                        Upgradeslots = item.EquipInfo.Upgradeslots,
                        Level = (byte)item.EquipInfo.Level,
                        Str = item.EquipInfo.Str,
                        Dex = item.EquipInfo.Dex,
                        Int = item.EquipInfo.Int,
                        Luk = item.EquipInfo.Luk,
                        Hp = item.EquipInfo.Hp,
                        Mp = item.EquipInfo.Mp,
                        Watk = item.EquipInfo.Watk,
                        Matk = item.EquipInfo.Matk,
                        Wdef = item.EquipInfo.Wdef,
                        Mdef = item.EquipInfo.Mdef,
                        Acc = item.EquipInfo.Acc,
                        Avoid = item.EquipInfo.Avoid,
                        Hands = item.EquipInfo.Hands,
                        Speed = item.EquipInfo.Speed,
                        Jump = item.EquipInfo.Jump,
                        Locked = item.EquipInfo.Locked,
                        Vicious = item.EquipInfo.Vicious,
                        Itemlevel = (byte)item.EquipInfo.Itemlevel,
                        Itemexp = item.EquipInfo.Itemexp,
                        RingId = item.EquipInfo.RingId,
                        RingSourceInfo = item.EquipInfo.RingSourceInfo == null ? null : new RingSourceModel()
                        {
                            Id = item.EquipInfo.RingSourceInfo.Id,
                            ItemId = item.EquipInfo.RingSourceInfo.ItemId,
                            RingId1 = item.EquipInfo.RingSourceInfo.RingId1,
                            RingId2 = item.EquipInfo.RingSourceInfo.RingId2,
                            CharacterId1 = item.EquipInfo.RingSourceInfo.CharacterId1,
                            CharacterId2 = item.EquipInfo.RingSourceInfo.CharacterId2
                        }
                    },
                    PetInfo = item.PetInfo == null ? null : new PetModel()
                    {
                        Petid = item.PetInfo.Petid,
                        Name = item.PetInfo.Name,
                        Level = item.PetInfo.Level,
                        Closeness = item.PetInfo.Closeness,
                        Fullness = item.PetInfo.Fullness,
                        Summoned = item.PetInfo.Summoned,
                        Flag = item.PetInfo.Flag
                    },
                    Properties = item.Properties
                };
                i++;
            }
            return result;
            
        }
        
        private ItemModel[] funcMain2(RepeatedField<ItemDto> p3)
        {
            if (p3 == null)
            {
                return null;
            }
            ItemModel[] result = new ItemModel[p3.Count];
            
            int v = 0;
            
            int i = 0;
            int len = p3.Count;
            
            while (i < len)
            {
                ItemDto item = p3[i];
                result[v++] = item == null ? null : new ItemModel()
                {
                    Type = (byte)item.Type,
                    Characterid = item.Characterid == null ? null : (int?)(int)item.Characterid,
                    Accountid = item.Accountid == null ? null : (int?)(int)item.Accountid,
                    Itemid = item.Itemid,
                    InventoryType = (sbyte)item.InventoryType,
                    Position = (short)item.Position,
                    Quantity = (short)item.Quantity,
                    Owner = item.Owner,
                    Flag = (short)item.Flag,
                    Expiration = item.Expiration,
                    GiftFrom = item.GiftFrom,
                    UniqueId = item.UniqueId,
                    EquipInfo = item.EquipInfo == null ? null : new EquipModel()
                    {
                        Id = item.EquipInfo.Id,
                        Upgradeslots = item.EquipInfo.Upgradeslots,
                        Level = (byte)item.EquipInfo.Level,
                        Str = item.EquipInfo.Str,
                        Dex = item.EquipInfo.Dex,
                        Int = item.EquipInfo.Int,
                        Luk = item.EquipInfo.Luk,
                        Hp = item.EquipInfo.Hp,
                        Mp = item.EquipInfo.Mp,
                        Watk = item.EquipInfo.Watk,
                        Matk = item.EquipInfo.Matk,
                        Wdef = item.EquipInfo.Wdef,
                        Mdef = item.EquipInfo.Mdef,
                        Acc = item.EquipInfo.Acc,
                        Avoid = item.EquipInfo.Avoid,
                        Hands = item.EquipInfo.Hands,
                        Speed = item.EquipInfo.Speed,
                        Jump = item.EquipInfo.Jump,
                        Locked = item.EquipInfo.Locked,
                        Vicious = item.EquipInfo.Vicious,
                        Itemlevel = (byte)item.EquipInfo.Itemlevel,
                        Itemexp = item.EquipInfo.Itemexp,
                        RingId = item.EquipInfo.RingId,
                        RingSourceInfo = item.EquipInfo.RingSourceInfo == null ? null : new RingSourceModel()
                        {
                            Id = item.EquipInfo.RingSourceInfo.Id,
                            ItemId = item.EquipInfo.RingSourceInfo.ItemId,
                            RingId1 = item.EquipInfo.RingSourceInfo.RingId1,
                            RingId2 = item.EquipInfo.RingSourceInfo.RingId2,
                            CharacterId1 = item.EquipInfo.RingSourceInfo.CharacterId1,
                            CharacterId2 = item.EquipInfo.RingSourceInfo.CharacterId2
                        }
                    },
                    PetInfo = item.PetInfo == null ? null : new PetModel()
                    {
                        Petid = item.PetInfo.Petid,
                        Name = item.PetInfo.Name,
                        Level = item.PetInfo.Level,
                        Closeness = item.PetInfo.Closeness,
                        Fullness = item.PetInfo.Fullness,
                        Summoned = item.PetInfo.Summoned,
                        Flag = item.PetInfo.Flag
                    },
                    Properties = item.Properties
                };
                i++;
            }
            return result;
            
        }
    }
}