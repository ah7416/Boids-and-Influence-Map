using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Boids
{
    class Tile
    {
        int width, height;
        Vector2 pos;
        Rectangle destRect;

        public Tile(int size, Vector2 pos)
        {

            this.pos = pos;
            this.width = size;
            this.height = size;
            this.destRect = new Rectangle((int)pos.X, (int)pos.Y, size, size);
        }

        public void Draw(SpriteBatch sb, Color color)
        {
            sb.Draw(DrawingExtensions.tileTex, destRect, color);
        }
    }
}
