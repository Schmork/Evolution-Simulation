using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Evolution_Simulation
{
    public class Plant : LivingCell
    {
        public static int InitialEnergy;

        private Plant(XY pos)
        {
            Energy = InitialEnergy;
            Pos = pos;
        }

        public static Plant Create(XY pos)
        {
            if (InitialEnergy > 0)
            {
                return new Plant(pos);
            }
            else return null;
        }
    }
}
