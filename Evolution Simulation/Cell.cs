using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace Evolution_Simulation
{
    public class Cell
    {
        public Color Color;
        public XY Pos;

        public Cell()
        {
            Color = StandardColor(GetType());
        }

        public static Color StandardColor(Type type)
        {
            if (type == typeof(Plant)) return Color.Green;
            if (type == typeof(Creature)) return Color.Magenta;
            if (type == typeof(DeadBody)) return Color.DarkSlateGray;
            if (type == typeof(Lava)) return Color.Yellow;
            return Color.HotPink;
        }
    }
}
