using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;

namespace Durability
{
    class DurabilityProj : GlobalProjectile
    {
        public int timeLeft;
        
        public override bool InstancePerEntity => true;

        public override void SetDefaults(Projectile projectile)
        {
            var conf = ModContent.GetInstance<DurabilityConfig>();
            timeLeft = conf.summonTime * 60;
            /*
            if (projectile.minion)
            {
                Durability.log += $"\nAdded proj {projectile.minionSlots}";
            }
            */
        }

        public override void PostAI(Projectile projectile)
        {
            if (projectile.minion)
            {
                if (--timeLeft < 0)
                {
                    //Durability.log += $"\nClearing proj {projectile.minionSlots}";
                    projectile.Kill(); // Unfortunately stardust dragon gets obliterated entirely :(
                }
            }
        }
    }
}
