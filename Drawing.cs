using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;

namespace WeaponDurability
{
    public static class Drawing
    {
		public static void DrawRect(SpriteBatch spriteBatch, Rectangle rect, Color color)
		{
			var btt = TextureAssets.BlackTile.Value;
			spriteBatch.Draw(btt, rect, color);
		}
	}
}
