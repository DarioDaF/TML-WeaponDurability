using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ID;
using Terraria.ModLoader;

namespace Durability
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
