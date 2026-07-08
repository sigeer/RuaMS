/*
	This file is part of the OdinMS Maple Story Server
    Copyright (C) 2008 Patrick Huy <patrick.huy@frz.cc>
		       Matthias Butz <matze@odinms.de>
		       Jan Christian Meyer <vimes@odinms.de>

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU Affero General Public License as
    published by the Free Software Foundation version 3 as published by
    the Free Software Foundation. You may not use, modify or distribute
    this program under any other version of the GNU Affero General Public
    License.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU Affero General Public License for more details.

    You should have received a copy of the GNU Affero General Public License
    along with this program.  If not, see <http://www.gnu.org/licenses/>.
*/


using Application.Core.Channel.DataProviders;
using Application.Core.Game.Maps.AnimatedObjects;
using Application.Core.tools.RandomUtils;
using Application.Templates.Item.Pet;
using client.inventory;

namespace Application.Core.Game.Items;

/**
 * @author Matze
 */
public class Pet : Item
{
    public string Name { get; set; }
    public int Fullness { get; set; } = MaxFullness;
    public int Tameness { get; set; }
    public byte Level { get; set; } = 1;
    public bool Summoned => MapPet != null;
    public MapPet? MapPet => PlayerInventory?.Owner?.GetPetById(PetId);
    public int PetAttribute { get; set; }

    public const int MaxFullness = 100;
    public const int MaxTameness = 30000;
    public const int MaxLevel = 30;

    public override PetItemTemplate SourceTemplate { get; }

    public Pet(PetItemTemplate template, short position, long uniqueid) : base(template.TemplateId, position, 1)
    {
        SourceTemplate = template;
        log = LogFactory.GetLogger(LogType.Pet);
        this.PetId = uniqueid;
        Name = ItemInformationProvider.getInstance().getName(id) ?? "";
    }

    public override Item copy()
    {
        var copyPet = new Pet(SourceTemplate, getPosition(), PetId);
        copyPet.Name = Name;
        copyPet.PetAttribute = PetAttribute;
        copyPet.Fullness = Fullness;
        copyPet.Tameness = Tameness;
        copyPet.Level = Level;

        CopyItemProps(copyPet);

        return copyPet;
    }

    public long getUniqueId()
    {
        return PetId;
    }

    public override long getCashId()
    {
        return PetId;
    }

    public override sbyte getItemType()
    {
        return 3;
    }

    public async Task addPetAttribute(Player owner, PetAttribute flag)
    {
        PetAttribute |= (int)flag;

        var petz = owner.getInventory(InventoryType.CASH).getItem(getPosition());
        if (petz != null)
        {
            await owner.forceUpdateItem(petz);
        }
    }

    public async Task removePetAttribute(Player owner, PetAttribute flag)
    {
        PetAttribute &= (int)(0xFFFFFFFF ^ (int)flag);

        var petz = owner.getInventory(InventoryType.CASH).getItem(getPosition());
        if (petz != null)
        {
            await owner.forceUpdateItem(petz);
        }
    }

    public override void setExpiration(long expire)
    {
        this.expiration = SourceTemplate.Permanent ? long.MaxValue : expire;
    }

    public Pet? EvolvePet(Player owner)
    {
        if (!SourceTemplate.CanEvol)
            return null;

        var nextPet = new LotteryMachine<int>(SourceTemplate.Evols.Select((x, idx) => new LotteryMachinItem<int>(x, SourceTemplate.EvolProbs[idx])))
            .GetRandomItem();
        var nextPetTemplate = ItemInformationProvider.getInstance().GetItemTemplate(nextPet) as PetItemTemplate;
        if (nextPetTemplate == null)
            return null;

        var evolved = new Pet(nextPetTemplate, 0, Yitter.IdGenerator.YitIdHelper.NextId());

        var fromDefaultName = owner.Client.CurrentCulture.GetItemName(getItemId()) ?? Name;
        var nextDefaultName = owner.Client.CurrentCulture.GetItemName(nextPet) ?? Name;
        evolved.Name = Name == fromDefaultName ? nextDefaultName : Name;
        evolved.Tameness = Tameness;
        evolved.Fullness = Fullness;
        evolved.Level = Level;
        evolved.setExpiration(owner.Client.CurrentServer.Node.GetCurrentTimeDateTimeOffset().AddDays(nextPetTemplate.Life).ToUnixTimeMilliseconds());

        return evolved;
    }

}

