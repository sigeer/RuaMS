/*
	This file is part of the OdinMS Maple Story Server
    Copyright (C) 2008 ~ 2010 Patrick Huy <patrick.huy@frz.cc>
                       Matthias Butz <matze@odinms.de>
                       Jan Christian Meyer <vimes@odinms.de>

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU Affero General Public License version 3
    as published by the Free Software Foundation. You may not use, modify
    or distribute this program under any other version of the
    GNU Affero General Public License.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU Affero General Public License for more details.

    You should have received a copy of the GNU Affero General Public License
    along with this program.  If not, see <http://www.gnu.org/licenses/>.
*/


using client;
using client.inventory;
using tools;


namespace server.maps;

public class MapItem : AbstractMapObject
{
    protected Client ownerClient;
    protected Item item;
    protected MapObject dropper;
    protected int character_ownerid, party_ownerid, meso, questid = -1;
    protected byte type;
    protected bool pickedUp = false, playerDrop, partyDrop;
    protected long dropTime;
    private object itemLock = new object();

    public MapItem(Item item, Point position, MapObject dropper, Character owner, Client ownerClient, byte type, bool playerDrop)
    {
        setPosition(position);
        this.item = item;
        this.dropper = dropper;
        this.character_ownerid = owner.getId();
        this.party_ownerid = owner.getPartyId();
        this.partyDrop = this.party_ownerid != -1;
        this.ownerClient = owner.getClient();
        this.meso = 0;
        this.type = type;
        this.playerDrop = playerDrop;
    }

    public MapItem(Item item, Point position, MapObject dropper, Character owner, Client ownerClient, byte type, bool playerDrop, int questid)
    {
        setPosition(position);
        this.item = item;
        this.dropper = dropper;
        this.character_ownerid = owner.getId();
        this.party_ownerid = owner.getPartyId();
        this.partyDrop = this.party_ownerid != -1;
        this.ownerClient = owner.getClient();
        this.meso = 0;
        this.type = type;
        this.playerDrop = playerDrop;
        this.questid = questid;
    }

    public MapItem(int meso, Point position, MapObject dropper, Character owner, Client ownerClient, byte type, bool playerDrop)
    {
        setPosition(position);
        this.item = null;
        this.dropper = dropper;
        this.character_ownerid = owner.getId();
        this.party_ownerid = owner.getPartyId();
        this.partyDrop = this.party_ownerid != -1;
        this.ownerClient = owner.getClient();
        this.meso = meso;
        this.type = type;
        this.playerDrop = playerDrop;
    }

    public Item getItem()
    {
        return item;
    }

    public int getQuest()
    {
        return questid;
    }

    public int getItemId()
    {
        if (meso > 0)
        {
            return meso;
        }
        return item.getItemId();
    }

    public MapObject getDropper()
    {
        return dropper;
    }

    public int getOwnerId()
    {
        return character_ownerid;
    }

    public int getPartyOwnerId()
    {
        return party_ownerid;
    }

    public void setPartyOwnerId(int partyid)
    {
        party_ownerid = partyid;
    }

    public int getClientsideOwnerId()
    {   // thanks nozphex (RedHat) for noting an issue with collecting party items
        if (this.party_ownerid == -1)
        {
            return this.character_ownerid;
        }
        else
        {
            return this.party_ownerid;
        }
    }

    public bool hasClientsideOwnership(Character player)
    {
        return this.character_ownerid == player.getId() || this.party_ownerid == player.getPartyId() || hasExpiredOwnershipTime();
    }

    public bool isFFADrop()
    {
        return type == 2 || type == 3 || hasExpiredOwnershipTime();
    }

    public bool hasExpiredOwnershipTime()
    {
        return DateTimeOffset.Now.ToUnixTimeMilliseconds() - dropTime >= 15 * 1000;
    }

    public bool canBePickedBy(Character chr)
    {
        if (character_ownerid <= 0 || isFFADrop())
        {
            return true;
        }

        if (party_ownerid == -1)
        {
            if (chr.getId() == character_ownerid)
            {
                return true;
            }
            else if (chr.isPartyMember(character_ownerid))
            {
                party_ownerid = chr.getPartyId();
                return true;
            }
        }
        else
        {
            if (chr.getPartyId() == party_ownerid)
            {
                return true;
            }
            else if (chr.getId() == character_ownerid)
            {
                party_ownerid = chr.getPartyId();
                return true;
            }
        }

        return hasExpiredOwnershipTime();
    }

    public Client getOwnerClient()
    {
        return (ownerClient.isLoggedIn() && !ownerClient.getPlayer().isAwayFromWorld()) ? ownerClient : null;
    }

    public int getMeso()
    {
        return meso;
    }

    public bool isPlayerDrop()
    {
        return playerDrop;
    }

    public bool isPickedUp()
    {
        return pickedUp;
    }

    public void setPickedUp(bool pickedUp)
    {
        this.pickedUp = pickedUp;
    }

    public long getDropTime()
    {
        return dropTime;
    }

    public void setDropTime(long time)
    {
        this.dropTime = time;
    }

    public byte getDropType()
    {
        return type;
    }

    public void lockItem()
    {
        Monitor.Enter(itemLock);
    }

    public void unlockItem()
    {
        Monitor.Exit(itemLock);
    }

    public override MapObjectType getType()
    {
        return MapObjectType.ITEM;
    }

    public override void sendSpawnData(Client client)
    {
        var chr = client.getPlayer();

        if (chr.needQuestItem(questid, getItemId()))
        {
            this.lockItem();
            try
            {
                client.sendPacket(PacketCreator.dropItemFromMapObject(chr, this, null, getPosition(), 2));
            }
            finally
            {
                this.unlockItem();
            }
        }
    }

    public override void sendDestroyData(Client client)
    {
        client.sendPacket(PacketCreator.removeItemFromMap(getObjectId(), 1, 0));
    }
}