using System;
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
        [Range(5, 40)]
        [DefaultValue(16)]
        public int yoyoMaxTime;

        [Label("Flail max duration [s]")]
        [Range(5, 40)]
        [DefaultValue(16)]
        public int flailMaxTime;

        [Label("Patch stack mixing with no stacking")]
        [DefaultValue(true)]
        public bool noStacking;

        [Label("Ex probability [1/x]")]
        [Range(1, 1000)]
        [DefaultValue(400)]
        public int exProb;

        [NonSerialized]
        private bool _fixItemCheck = false;
        [BackgroundColor(255, 0, 0)]
        [Label("[BREAKS TML] RESTORE VANILLA ITEMCHECK")]
        [Tooltip("DO NOT ENABLE, BREAKS TML LOGIC, WILL BE REMOVED IN FUTURE VERSIONS")]
        [ReloadRequired]
        [DefaultValue(false)]
        public bool fixItemCheck {
            set {
                if (!ItemCheckPatcher.CantPatch)
                    _fixItemCheck = value;
            }
            get => !ItemCheckPatcher.CantPatch && _fixItemCheck;
        }
    }
}
