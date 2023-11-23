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
    }
}
