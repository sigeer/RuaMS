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


using Application.Core.model;
using client.inventory.manipulator;
using constants.game;
using Microsoft.EntityFrameworkCore;
using server;
using server.movement;
using tools;

namespace client.inventory;

/**
 * @author Matze
 */
public class Pet : Item
{
    private string name;
    private int uniqueid;
    private int tameness = 0;
    private byte level = 1;
    private int fullness = 100;
    private int Fh;
    private Point pos;
    private int stance;
    private bool summoned;
    private int petAttribute = 0;


    private Pet(int id, short position, int uniqueid) : base(id, position, 1)
    {

        this.uniqueid = uniqueid;
        this.pos = new Point(0, 0);
    }

    public static Pet loadFromDb(int itemid, short position, int petid)
    {
        Pet ret = new Pet(itemid, position, petid);
        try
        { // Get the pet details...
            using var dbContext = new DBContext();
            var dbModel = dbContext.Pets.FirstOrDefault(x => x.Petid == petid);
            if (dbModel != null)
            {
                ret.setName(dbModel.Name);
                ret.setTameness(Math.Min(dbModel.Closeness, 30000));
                ret.setLevel((byte)Math.Min(dbModel.Level, 30));
                ret.setFullness(Math.Min(dbModel.Fullness, 100));
                ret.setSummoned(dbModel.Summoned);
                ret.setPetAttribute(dbModel.Flag);
            }
            return ret;
        }
        catch (Exception e)
        {
            Log.Logger.Error(e.ToString());
            return null;
        }
    }

    public static void deleteFromDb(IPlayer owner, int petid)
    {
        try
        {
            using var dbContext = new DBContext();
            dbContext.Pets.Where(x => x.Petid == petid).ExecuteDelete();

            owner.resetExcluded(petid);
            CashIdGenerator.freeCashId(petid);
        }
        catch (Exception ex)
        {
            Log.Logger.Error(ex.ToString());
        }
    }

    public void saveToDb()
    {
        try
        {
            using var dbContext = new DBContext();
            dbContext.Pets.Where(x => x.Petid == getUniqueId()).ExecuteUpdate(x =>
                x.SetProperty(y => y.Flag, getPetAttribute())
                .SetProperty(y => y.Name, getName())
                .SetProperty(y => y.Level, getLevel())
                .SetProperty(y => y.Closeness, getTameness())
                .SetProperty(y => y.Fullness, getFullness())
                .SetProperty(y => y.Summoned, isSummoned()));
        }
        catch (Exception e)
        {
            Log.Logger.Error(e.ToString());
        }
    }

    public static int createPet(int itemid)
    {
        return createPet(itemid, 1, 0, 100);
    }

    public static int createPet(int itemid, byte level, int tameness, int fullness)
    {
        try
        {
            using var dbContext = new DBContext();
            var dbModel = new DB_Pet
            {
                Petid = CashIdGenerator.generateCashId(),
                Name = ItemInformationProvider.getInstance().getName(itemid),
                Level = level,
                Closeness = tameness,
                Fullness = fullness,
                Summoned = false,
                Flag = 0
            };
            dbContext.Pets.Add(dbModel);
            dbContext.SaveChanges();
            return dbModel.Petid;
        }
        catch (Exception e)
        {
            Log.Logger.Error(e.ToString());
            return -1;
        }
    }

    public string getName()
    {
        return name;
    }

    public void setName(string name)
    {
        this.name = name;
    }

    public int getUniqueId()
    {
        return uniqueid;
    }

    public void setUniqueId(int id)
    {
        this.uniqueid = id;
    }

    public int getTameness()
    {
        return tameness;
    }

    public void setTameness(int tameness)
    {
        this.tameness = tameness;
    }

    public byte getLevel()
    {
        return level;
    }

    public void gainTamenessFullness(IPlayer owner, int incTameness, int incFullness, int type)
    {
        gainTamenessFullness(owner, incTameness, incFullness, type, false);
    }

    public void gainTamenessFullness(IPlayer owner, int incTameness, int incFullness, int type, bool forceEnjoy)
    {
        sbyte slot = owner.getPetIndex(this);
        bool enjoyed;

        //will NOT increase pet's tameness if tried to feed pet with 100% fullness
        // unless forceEnjoy == true (cash shop)
        if (fullness < 100 || incFullness == 0 || forceEnjoy)
        {   //incFullness == 0: command given
            int newFullness = fullness + incFullness;
            if (newFullness > 100)
            {
                newFullness = 100;
            }
            fullness = newFullness;

            if (incTameness > 0 && tameness < 30000)
            {
                int newTameness = tameness + incTameness;
                if (newTameness > 30000)
                {
                    newTameness = 30000;
                }

                tameness = newTameness;
                while (newTameness >= ExpTable.getTamenessNeededForLevel(level))
                {
                    level += 1;
                    owner.sendPacket(PacketCreator.showOwnPetLevelUp(slot));
                    owner.getMap().broadcastMessage(PacketCreator.showPetLevelUp(owner, slot));
                }
            }

            enjoyed = true;
        }
        else
        {
            int newTameness = tameness - 1;
            if (newTameness < 0)
            {
                newTameness = 0;
            }

            tameness = newTameness;
            if (level > 1 && newTameness < ExpTable.getTamenessNeededForLevel(level - 1))
            {
                level -= 1;
            }

            enjoyed = false;
        }

        owner.getMap().broadcastMessage(PacketCreator.petFoodResponse(owner.getId(), slot, enjoyed, false));
        saveToDb();

        var petz = owner.getInventory(InventoryType.CASH).getItem(getPosition());
        if (petz != null)
        {
            owner.forceUpdateItem(petz);
        }
    }

    public void setLevel(byte level)
    {
        this.level = level;
    }

    public int getFullness()
    {
        return fullness;
    }

    public void setFullness(int fullness)
    {
        this.fullness = fullness;
    }

    public int getFh()
    {
        return Fh;
    }

    public void setFh(int Fh)
    {
        this.Fh = Fh;
    }

    public Point getPos()
    {
        return pos;
    }

    public void setPos(Point pos)
    {
        this.pos = pos;
    }

    public int getStance()
    {
        return stance;
    }

    public void setStance(int stance)
    {
        this.stance = stance;
    }

    public bool isSummoned()
    {
        return summoned;
    }

    public void setSummoned(bool yes)
    {
        this.summoned = yes;
    }

    public int getPetAttribute()
    {
        return this.petAttribute;
    }

    private void setPetAttribute(int flag)
    {
        this.petAttribute = flag;
    }

    public void addPetAttribute(IPlayer owner, PetAttribute flag)
    {
        this.petAttribute |= (int)flag;
        saveToDb();

        Item? petz = owner.getInventory(InventoryType.CASH).getItem(getPosition());
        if (petz != null)
        {
            owner.forceUpdateItem(petz);
        }
    }

    public void removePetAttribute(IPlayer owner, PetAttribute flag)
    {
        this.petAttribute &= (int)(0xFFFFFFFF ^ (int)flag);
        saveToDb();

        Item? petz = owner.getInventory(InventoryType.CASH).getItem(getPosition());
        if (petz != null)
        {
            owner.forceUpdateItem(petz);
        }
    }

    public PetCanConsumePair canConsume(int itemId)
    {
        return ItemInformationProvider.getInstance().canPetConsume(this.getItemId(), itemId);
    }

    public void updatePosition(List<LifeMovementFragment> movement)
    {
        foreach (LifeMovementFragment move in movement)
        {
            if (move is LifeMovement)
            {
                if (move is AbsoluteLifeMovement)
                {
                    this.setPos(move.getPosition());
                }
                this.setStance(((LifeMovement)move).getNewstate());
            }
        }
    }
}

public enum PetAttribute
{
    OWNER_SPEED = 0x01
}