using Microsoft.Xna.Framework;
using PKPhysics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PKPhysicsTestProject
{
    public static class PKConverter
    {
        public static Vector2 ToVector2(PKVector vector)
        {
            return new Vector2(vector.X, vector.Y);
        }

        public static PKVector ToPKVector2(Vector2 vector)
        {
            return new PKVector(vector.X, vector.Y);
        }

        public static void ToVector2Array(PKVector[] src, ref Vector2[] dst)
        {
            if (dst is null || src.Length != dst.Length)
            {
                dst = new Vector2[src.Length];
            }

            for (int i = 0; i < src.Length; i++)
            {
                dst[i].X = src[i].X;
                dst[i].Y = src[i].Y;
            }
        }
    }
}
