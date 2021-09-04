using Terraria.ID;
using Terraria.ModLoader;

namespace WeaponDurability
{
    class DepleatedEoCShield : ModItem
    {
        public override string Texture => "Terraria/Item_" + ItemID.EoCShield;

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Consumed Shield of Cthulhu");
        }

        public override void SetDefaults()
        {
            item.CloneDefaults(ItemID.EoCShield);
            item.damage = 1;
        }

    }
}
