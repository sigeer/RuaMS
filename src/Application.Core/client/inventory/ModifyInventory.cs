namespace client.inventory;

/// <summary>
/// @author kevin
/// <para>0: add, 1: update, 2: move, 3: remove</para>
/// </summary>
public class ModifyInventory
{

    /// <summary>
    /// 0: add, 1: update, 2: move, 3: remove
    /// </summary>
    private int mode;
    private Item item;
    private short oldPos;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="mode">0: add, 1: update, 2: move, 3: remove</param>
    /// <param name="item"></param>
    /// <param name="oldPos"></param>
    public ModifyInventory(int mode, Item item, short oldPos = 0)
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
    }
}