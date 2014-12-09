using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Evolution_Simulation
{
    public abstract class LivingCell : Cell
    {
        public int Energy;

        public bool IsAlive {
            get {return Energy > 0;}
            set {if (!value) Energy = 0;}
        }
    }
}
