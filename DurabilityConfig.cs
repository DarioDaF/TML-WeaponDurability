using System.ComponentModel;
using Terraria.ModLoader.Config;

namespace WeaponDurability
{
    class DurabilityConfig : ModConfig
    {
        public override ConfigScope Mode => ConfigScope.ServerSide;

        [Label("Usages for each weapon")]
        [Range(1, 300)]
        [DefaultValue(10)]
        public int uses;

        [Label("Summon time [s]")]
        [Range(10, 120)]
        [DefaultValue(40)]
        public int summonTime;

        [Label("Turret time [s]")]
        [Range(10, 120)]
        [DefaultValue(40)]
        public int turretTime;

        [Label("Throw count")]
        [Range(1, 300)]
        [DefaultValue(10)]
        public int maxThrow;

        [Label("Throw cooldown [s]")]
        [Range(1, 120)]
        [DefaultValue(15)]
        public int coolThrow;

        [Label("Throw sickness [s]")]
        [Range(10, 120)]
        [DefaultValue(90)]
        public int sickThrow;

        [Label("Yoyo max duration [s]")]
        [Range(5, 25)]
        [DefaultValue(16)]
        public int yoyoMaxTime;

        [Label("Patch stack mixing with no stacking")]
        [DefaultValue(true)]
        public bool noStacking;

        [Label("Ex probability [1/x]")]
        [Range(1, 1000)]
        [DefaultValue(400)]
        public int exProb;

    }
}
