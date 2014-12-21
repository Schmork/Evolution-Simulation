using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Evolution_Simulation
{
    /// <summary>
    /// A structure indicating a creature's next action. The public bool fields are all set to false, except one, which is true. This action is meant to be taken next.
    /// </summary>
    public class ActionVector
    {
        public static int OutputCount { get; set; }
        /// <summary>
        /// One of a creature's possible actions
        /// </summary>
        public bool Left, Right, Move, Stay, Eat, Split;

        public ActionVector(double[] outputs, Random rnd)
        {
            var sum = outputs.Sum() * rnd.NextDouble();                         // randomly scaled sum of all outputs

            var n = 0;
            for (; n < OutputCount - 1; n++)
            {
                sum -= outputs[n];
                if (sum < 0) break;                                             // select one index n
            }
            
            var fields = this.GetType().GetFields().OrderBy(x => x.Name);
            fields.ElementAt(n).SetValue(this, true);                           // set n-th action := true (others remain false)
        }
    }
}
