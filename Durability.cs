using System.Collections.Generic;
using Terraria.ID;
using Terraria.ModLoader;

namespace WeaponDurability
{
	public class Durability : Mod
    {
        public static ModKeybind HotReload;
        public static ModKeybind FastSwap;

        public static Dictionary<int, float> oldYoyo = new Dictionary<int, float>();
        public static int oldSiltMode;

        public override void Load()
        {
            oldSiltMode = ItemID.Sets.ExtractinatorMode[ItemID.SiltBlock];
            ItemID.Sets.ExtractinatorMode[ItemID.SiltBlock] = ItemID.SiltBlock;

            var conf = ModContent.GetInstance<DurabilityConfig>();

            // @TODO: This should be done after all other mods items are loaded...
            for (int i = 0; i < ProjectileID.Sets.YoyosLifeTimeMultiplier.Length; ++i)
            {
                if ((ProjectileID.Sets.YoyosLifeTimeMultiplier[i] <= 0) || (ProjectileID.Sets.YoyosLifeTimeMultiplier[i] > conf.yoyoMaxTime))
                {
                    oldYoyo.Add(i, ProjectileID.Sets.YoyosLifeTimeMultiplier[i]);
                    ProjectileID.Sets.YoyosLifeTimeMultiplier[i] = conf.yoyoMaxTime; // Cap yoyo lifetime
                }
            }

            HotReload = KeybindLoader.RegisterKeybind(this, "Reload durability (CHEAT)", "P");
            FastSwap = KeybindLoader.RegisterKeybind(this, "Fast inventory swap", "Q");

            DurabilityProj.PrecomputeProjs();
        }

        public override void Unload()
        {
            HotReload = null;
            FastSwap = null;

            foreach(var k in oldYoyo)
            {
                ProjectileID.Sets.YoyosLifeTimeMultiplier[k.Key] = k.Value;
            }

            ItemID.Sets.ExtractinatorMode[ItemID.SiltBlock] = oldSiltMode;
        }

        //public static string log = "...";

        /*
        public override void PostDrawInterface(SpriteBatch spriteBatch)
        {
            Utils.DrawBorderString(spriteBatch, log, new Vector2(20, 80), Color.Red);
        }
        */

    }
}