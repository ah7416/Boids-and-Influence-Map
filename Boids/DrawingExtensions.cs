using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Boids
{
    public static class DrawingExtensions
    {
        public static Texture2D tileTex;

        public static Texture2D CreateSquare(GraphicsDevice gd, int dim)
        {
            Texture2D square = new Texture2D(gd, dim, dim);

            Microsoft.Xna.Framework.Color[] colorData = new Microsoft.Xna.Framework.Color[dim * dim];
            for (int i = 0; i < colorData.Length; i++)
            {
                colorData[i] = Microsoft.Xna.Framework.Color.White;
            }
            square.SetData<Microsoft.Xna.Framework.Color>(colorData);
            return square;
        }


        public static void DrawLineSegment(SpriteBatch spriteBatch, Vector2 point1, Vector2 point2, Microsoft.Xna.Framework.Color color, int lineWidth, Texture2D tex)
        {
            float angle = (float)Math.Atan2(point2.Y - point1.Y, point2.X - point1.X);
            float length = Vector2.Distance(point1, point2);
            spriteBatch.Draw(tex, point1, null, color,
            angle, Vector2.Zero, new Vector2(length, lineWidth),
            SpriteEffects.None, 0f);
        }



        public static void DrawPolygon(SpriteBatch spriteBatch, Vector2[] vertex, int count, Microsoft.Xna.Framework.Color color, int lineWidth, Texture2D tex)
        {
            if (count > 0)
            {
                for (int i = 0; i < count - 1; i++)
                {
                    DrawLineSegment(spriteBatch, vertex[i], vertex[i + 1], color, lineWidth, tex);
                }
                DrawLineSegment(spriteBatch, vertex[count - 1], vertex[0], color, lineWidth, tex);
            }
        }

        public static void DrawCircle(SpriteBatch spritbatch, Vector2 center, float radius, Microsoft.Xna.Framework.Color color, int lineWidth, Texture2D tex, int segments = 16)
        {

            Vector2[] vertex = new Vector2[segments];

            double increment = Math.PI * 2.0 / segments;
            double theta = 0.0;

            for (int i = 0; i < segments; i++)
            {
                vertex[i] = center + radius * new Vector2((float)Math.Cos(theta), (float)Math.Sin(theta));
                theta += increment;
            }

            DrawPolygon(spritbatch, vertex, segments, color, lineWidth, tex);
        }

        public static void DrawCone(SpriteBatch spritbatch, Vector2 center, float radius, Microsoft.Xna.Framework.Color color, int lineWidth, float angle, float rotation, Texture2D tex, int segments = 16)
        {
            Vector2[] vertex = new Vector2[segments + 3];
            double increment = angle / segments;
            double theta = 0.0;

            for (int i = 0; i < segments + 3; i++)
            {
                float cos = (float)Math.Cos(theta);
                float sin = (float)Math.Sin(theta);
                Vector2 drawPoint = center + (radius * new Vector2(cos, sin));
                drawPoint = RotateVector(rotation - (angle / 2), center, drawPoint);
                vertex[i] = drawPoint;

                if (i == segments + 1)
                {
                    vertex[i] = center;
                }
                else if (i == 0)
                {
                    vertex[i] = center;
                }
                theta += increment;
            }
            vertex[segments + 2] = center;

            DrawPolygon(spritbatch, vertex, segments, color, lineWidth, tex);
        }

        public static Vector2 RotateVector(float angle, Vector2 origin, Vector2 point)
        {
            Matrix r = new Matrix();
            r.Right = new Vector3((float)Math.Cos(angle), (float)-Math.Sin(angle), 0);
            r.Up = new Vector3((float)Math.Sin(angle), (float)Math.Cos(angle), 0);
            r.Backward = new Vector3(0, 0, 1);

            Matrix t = new Matrix();
            t.Right = new Vector3(1, 0, origin.X);
            t.Up = new Vector3(0, 1, origin.Y);
            t.Backward = new Vector3(0, 0, 1);

            Matrix negativeT = new Matrix();
            negativeT.Right = new Vector3(1, 0, -origin.X);
            negativeT.Up = new Vector3(0, 1, -origin.Y);
            negativeT.Backward = new Vector3(0, 0, 1);
            Matrix pointMatrix = new Matrix();
            pointMatrix.M11 = point.X;
            pointMatrix.M21 = point.Y;
            pointMatrix.M31 = 1;
            Matrix final = t * r * negativeT * pointMatrix;

            return new Vector2(final.M11, final.M21);
        }

        public static void DrawRectangle(SpriteBatch spriteBatch, Rectangle rectangle, Color color, int lineWidth, Texture2D tex)
        {
            Vector2[] vertex = new Vector2[4];
            vertex[0] = new Vector2(rectangle.Left, rectangle.Top);
            vertex[1] = new Vector2(rectangle.Right, rectangle.Top);
            vertex[2] = new Vector2(rectangle.Right, rectangle.Bottom);
            vertex[3] = new Vector2(rectangle.Left, rectangle.Bottom);

            DrawPolygon(spriteBatch, vertex, 4, color, lineWidth, tex);
        }

    }
}
