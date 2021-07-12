using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;

namespace Durability
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
