namespace client.inventory;

/**
 * @author kevin
 */
public class ModifyInventory
{

    private int mode;
    private Item item;
    private short oldPos;

    public ModifyInventory(int mode, Item item)
    {
        this.mode = mode;
        this.item = item.copy();
    }

    public ModifyInventory(int mode, Item item, short oldPos)
    {
        this.mode = mode;
        this.item = item.copy();
        this.oldPos = oldPos;
    }

    public int getMode()
    {
        return mode;
    }

    public int getInventoryType()
    {
        return item.getInventoryType().getType();
    }

    public short getPosition()
    {
        return item.getPosition();
    }

    public short getOldPosition()
    {
        return oldPos;
    }

    public short getQuantity()
    {
        return item.getQuantity();
    }

    public Item getItem()
    {
        return item;
    }

    public void clear()
    {
        this.item = null;
    }
}