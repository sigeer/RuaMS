using client.inventory;
using server.maps;
using tools;


namespace Application.Core.Game.Maps;

public class MapItem : AbstractMapObject
{
    protected IClient ownerClient;
    protected Item item;
    protected IMapObject dropper;
    protected int character_ownerid, party_ownerid, meso, questid = -1;
    protected byte type;
    protected bool pickedUp = false, playerDrop, partyDrop;
    protected long dropTime;
    private object itemLock = new object();

    public MapItem(Item item, Point position, IMapObject dropper, IPlayer owner, byte type, bool playerDrop)
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

    public MapItem(Item item, Point position, IMapObject dropper, IPlayer owner, byte type, bool playerDrop, int questid)
        : this(item, position, dropper, owner, type, playerDrop)
    {
        this.questid = questid;
    }

    public MapItem(int meso, Point position, IMapObject dropper, IPlayer owner, byte type, bool playerDrop)
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

    public IMapObject getDropper()
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

    public bool hasClientsideOwnership(IPlayer player)
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

    public bool canBePickedBy(IPlayer chr)
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

    public IClient? getOwnerClient()
    {
        return (ownerClient.isLoggedIn() && !ownerClient.OnlinedCharacter.isAwayFromWorld()) ? ownerClient : null;
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

    public override void sendSpawnData(IClient client)
    {
        var chr = client.OnlinedCharacter;

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

    public override void sendDestroyData(IClient client)
    {
        client.sendPacket(PacketCreator.removeItemFromMap(getObjectId(), 1, 0));
    }
}