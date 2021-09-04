using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace WeaponDurability
{
    class DurabilityProj : GlobalProjectile
    {
        public int timeLeft;
        
        public override bool InstancePerEntity => true;

        public static List<int> FancyGunProjs = new List<int>();
        //public static Projectile countingSpawns = null;
        public static int countingSpawns = -1;

        public static void PrecomputeProjs()
        {
            int i = 0;
            foreach (var gp in ItemID.Sets.gunProj)
            {
                var item = new Item();
                item.SetDefaults(i);
                FancyGunProjs.Add(item.shoot);
                ++i;
            }
        }

        public override void SetDefaults(Projectile projectile)
        {
            /*
            // That's too much multishot get a boom
            if (countingSpawns != null)
            {
                // We are fancy, the creation of a proj is a use!
                var player = Main.player[countingSpawns.owner];
                var item = player.HeldItem;
                DurabilityItem.ConsumeDurability(item, player);
            }
            */
            if (countingSpawns >= 0)
            {
                countingSpawns += 1;
            }

            timeLeft = -1; // Unaffected, use 0 if you want 1 frame time
            var conf = ModContent.GetInstance<DurabilityConfig>();
            if (projectile.minion)
            {
                timeLeft = conf.summonTime * 60;
            }
            if (projectile.sentry)
            {
                timeLeft = conf.turretTime * 60;
            }
            if (projectile.type == ProjectileID.FlyingKnife)
            {
                timeLeft = conf.yoyoMaxTime * 60; // I guess it's like a yoyo???
            }
            /*
            if (projectile.minion)
            {
                Durability.log += $"\nAdded proj {projectile.minionSlots}";
            }
            */
        }

        public override bool PreAI(Projectile projectile)
        {
            /*
            countingSpawns = null;
            if (FancyGunProjs.Contains(projectile.type))
                countingSpawns = projectile;
            */
            countingSpawns = FancyGunProjs.Contains(projectile.type) ? 0 : -1;
            return true;
        }

        public override void PostAI(Projectile projectile)
        {
            if (countingSpawns > 0)
            {
                // Spawned projs consume only one for pity
                var player = Main.player[projectile.owner];
                var item = player.HeldItem;
                if (!DurabilityItem.ConsumeDurability(item, player)) {
                    projectile.Kill();
                }
            }
            countingSpawns = -1;
            //countingSpawns = null;

            if (timeLeft >= 0) // If affected
            {
                if (timeLeft == 0)
                {
                    //Durability.log += $"\nClearing proj {projectile.minionSlots}";
                    projectile.Kill(); // Unfortunately stardust dragon gets obliterated entirely :(
                }
                else
                {
                    --timeLeft; // Don't wanna accidental < 0 stuff that was affected
                }
            }
        }
    }
}
