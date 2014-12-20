using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Evolution_Simulation
{
    public class XY
    {
        public int X, Y;

        public XY(int x, int y)
        {
            X = x;
            Y = y;
        }

        public static XY operator +(XY a, XY b)
        {
            var x = a.X + b.X;
            var y = a.Y + b.Y;
            return new XY(x, y);
        }

        public static XY operator -(XY a, XY b)
        {
            var x = a.X - b.X;
            var y = a.Y - b.Y;
            return new XY(x, y);
        }
    }
}
