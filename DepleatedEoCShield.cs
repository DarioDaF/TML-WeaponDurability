using Terraria.ID;
using Terraria.ModLoader;

namespace WeaponDurability
{
    class DepleatedEoCShield : ModItem
    {
        public override string Texture => "Terraria/Images/Item_" + ItemID.EoCShield;

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Consumed Shield of Cthulhu");
        }

        public override void SetDefaults()
        {
            Item.CloneDefaults(ItemID.EoCShield);
            Item.damage = 1;
        }

    }
}
