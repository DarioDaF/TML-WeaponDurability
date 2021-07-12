using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Durability
{
	public class Durability : Mod
	{
        public static ModHotKey HotReload;
        
        public static int oldSiltMode;

        public override void Load()
        {
            oldSiltMode = ItemID.Sets.ExtractinatorMode[ItemID.SiltBlock];
            ItemID.Sets.ExtractinatorMode[ItemID.SiltBlock] = ItemID.SiltBlock;
            HotReload = RegisterHotKey("Reload durability (CHEAT)", "Z");
        }

        public override void Unload()
        {
            HotReload = null;
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