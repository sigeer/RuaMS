using Application.Core.Login.Models;
using Application.Shared.Constants;
using Application.Shared.Items;
using Application.Utility.Configs;
using Application.Utility.Extensions;
using Google.Protobuf.WellKnownTypes;

namespace Application.Core.Login.Services.PlayerCreator;

public class NewCharacterBuilder
{
    public Job Job { get; }
    public int Level { get; }
    public int Map { get; }
    public int Top { get; }
    public int Bottom { get; }
    public int Shoes { get; }
    public int Weapon { get; }
    public int Str { get; } = 4;
    public int Dex { get; } = 4;
    public int Int { get; } = 4;
    public int Luk { get; } = 4;
    public int MaxHP { get; } = 50;
    public int MaxMP { get; } = 5;
    public int AP { get; }
    public int SP { get; }
    public int Meso { get; set; }

    public int Gender { get; }
    public int SkinColor { get; set; }
    public int Hair { get; set; }
    public int Face { get; set; }
    public string Name { get; set; }

    public NewCharacterBuilder(string name, int gender, Job job, int level, int map, int top, int bottom, int shoes, int weapon)
    {
        Name = name;
        Gender = gender;
        Job = job;
        Level = level;
        Map = map;
        Top = top;
        Bottom = bottom;
        Shoes = shoes;
        Weapon = weapon;

        if (!YamlConfig.config.server.USE_STARTING_AP_4)
        {
            if (YamlConfig.config.server.USE_AUTOASSIGN_STARTERS_AP)
            {
                Str = 12;
                Dex = 5;
            }
            else
            {
                AP = 9;
            }
        }

    }
    public string GetRemainingSp()
    {
        var arr = new int[10];
        arr[GameConstants.getSkillBook(Job.Id)] = SP;
        return arr.AdpteSP();
    }

    public virtual NewCharacterPreview Build(AccountCtrl account)
    {
        var newCharacter = new ProtoModel.CharacterProto()
        {
            AccountId = account.Id,
            Hp = MaxHP,
            Mp = MaxMP,
            Maxhp = MaxHP,
            Maxmp = MaxMP,
            Str = Str,
            Dex = Dex,
            Int = Int,
            Luk = Luk,
            JobId = Job.Id,
            Level = Level,

            Skincolor = SkinColor,
            Gender = Gender,
            Name = Name,
            Hair = Hair,
            Face = Face,
            Map = Map,
            Ap = AP,
            Meso = Meso,
            Sp = GetRemainingSp(),

            DataString = "",
            CreateDate = DateTimeOffset.Now.ToTimestamp(),
            LastExpGainTime = DateTimeOffset.Now.ToTimestamp(),
            LastLogoutTime = DateTimeOffset.Now.ToTimestamp(),

            Equipslots = DefaultConfigs.BagSize,
            Useslots = DefaultConfigs.BagSize,
            Etcslots = DefaultConfigs.BagSize,
            Setupslots = DefaultConfigs.BagSize,
        };

        var data = new ProtoModel.CharacterDataProto() { GachaponStorage = new ProtoModel.StorageProto(), Bag = new ProtoModel.CharacterBagDataProto() };
        data.Bag.EquippedInv.Add(new ProtoModel.ItemProto
        {
            Position = -5,
            Quantity = 1,
            Expiration = -1,
            Itemid = Top,
        });
        data.Bag.EquippedInv.Add(new ProtoModel.ItemProto
        {
            Position = -6,
            Quantity = 1,
            Expiration = -1,
            Itemid = Bottom,
        });
        data.Bag.EquippedInv.Add(new ProtoModel.ItemProto
        {
            Position = -7,
            Quantity = 1,
            Expiration = -1,
            Itemid = Shoes,
        });
        data.Bag.EquippedInv.Add(new ProtoModel.ItemProto
        {
            Position = -11,
            Quantity = 1,
            Expiration = -1,
            Itemid = Weapon,
        });

        newCharacter.Data = data;
        return new NewCharacterPreview(account, newCharacter);
    }
}
