using Terraria;
using Terraria.ModLoader;

namespace WeaponDurability
{
    class ConsumableDizzyness : ModBuff
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Throwing dizziness");
            Description.SetDefault("You feel dizzy for throwing stuff");
            Main.debuff[Type] = true;
            Main.buffNoSave[Type] = true;
            //CanBeCleared = false;
        }

        public override bool ReApply(Player player, int time, int buffIndex)
        {
            var p = player.GetModPlayer<DurabilityPlayer>();
            var conf = ModContent.GetInstance<DurabilityConfig>();
            p.thrownCount += 1;
            if (p.thrownCount >= conf.maxThrow)
            {
                // Make this worse
                player.DelBuff(buffIndex);
                player.AddBuff(ModContent.BuffType<ConsumableSickness>(), 60*conf.sickThrow);
                return false;
            }
            player.buffTime[buffIndex] = time;
            return true;
        }

    }
}
