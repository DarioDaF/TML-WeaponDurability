using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;

namespace WeaponDurability
{
    public static class Drawing
    {
		public static void DrawRect(SpriteBatch spriteBatch, Rectangle rect, Color color)
		{
			var btt = Main.blackTileTexture;
			spriteBatch.Draw(btt, rect, color);
		}
	}
}
