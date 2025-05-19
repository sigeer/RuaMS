namespace Application.Core.EF.Entities.Items
{
    public class ItemEntityPair
    {
        public ItemEntityPair(Inventoryitem item, EquipEntityPair? equip, PetEntity? pet)
        {
            Item = item;
            Equip = equip;
            Pet = pet;
        }

        public Inventoryitem Item { get; set; }
        public EquipEntityPair? Equip { get; set; }
        public PetEntity? Pet { get; set; }
    }

    public class EquipEntityPair
    {
        public EquipEntityPair(Inventoryequipment equip, Ring_Entity? ring)
        {
            Equip = equip;
            Ring = ring;
        }

        public Inventoryequipment Equip { get; set; }
        public Ring_Entity? Ring { get; set; }
    }
}
