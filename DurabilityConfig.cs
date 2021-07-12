using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ModLoader.Config;

namespace Durability
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

        [Label("Ex probability [1/x]")]
        [Range(1, 100)]
        [DefaultValue(60)]
        public int exProb;
    }
}
