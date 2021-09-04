using Terraria;
using Terraria.ModLoader;

namespace WeaponDurability
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
