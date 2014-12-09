using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Evolution_Simulation
{
    public class DeadBody : LivingCell
    {
        public DeadBody(XY pos, int energy)
        {
            Pos = pos;
            if (energy > 0)
            {
                Energy = energy;
            }
            else Energy = World.DeadBodyDecay;
        }
    }
}
