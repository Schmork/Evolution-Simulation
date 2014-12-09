using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Evolution_Simulation
{
    class Normalizer
    {
        public static double StayInBounds(double x, int bound)              // double, int          -b <= x <= b
        {
            if (x < -bound) return -bound;
            if (x > bound) return bound;
            return x;
        }

        public static double StayInBounds(double x, double bound)           // double, double       -b <= x <= b
        {
            if (x < -bound) return -bound;
            if (x > bound) return bound;
            return x;
        }

        public static double StayBelow(double x, int bound)                 // double, int           0 <= x <= b
        {
            if (x < 0) return 0;
            if (x > bound) return bound;
            return x;
        }

        public static int StayBelow(int x, int bound)                       // int, int              0 <= x <= b
        {
            if (x < 0) return 0;
            if (x > bound) return bound;
            return x;
        }

        public static XY WrapWorld(XY xy)
        {
            xy.X = wrapValue(xy.X, Grid.Width);
            xy.Y = wrapValue(xy.Y, Grid.Height);
            return xy;
        }

        public static int WrapWorld(int x, int bound)
        {
            return wrapValue(x, bound);
        }

        private static int wrapValue(int x, int bound)
        {
            if (x < 0 || x >= bound)
            {
                var remainder = x % bound;
                return remainder < 0 ? remainder + bound : remainder;
            }
            if (x < 0) return bound - 1;
            if (x >= bound) return 0;
            return x;
        }
    }
}
