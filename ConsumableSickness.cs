using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;

namespace Durability
{
    class ConsumableSickness : ModBuff
    {
        public override void SetDefaults()
        {
            DisplayName.SetDefault("Throwing sickness");
            Description.SetDefault("You can't throw stuff");
            Main.debuff[Type] = true;
            Main.buffNoSave[Type] = true;
            canBeCleared = false;
        }

    }
}
