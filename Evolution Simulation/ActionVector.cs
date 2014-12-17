using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Evolution_Simulation
{
    public class ActionVector
    {
        public static int OutputCount { get; set; }
        public bool Left, Right, Move, Stay, Eat, Split;

        public ActionVector(double[] outputs, Random rnd, MainForm mainForm)       // outputs has to be normalized: each value between 0 and 1 (including)
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

            /*if (Left) mainForm.left++;
            if (Right) mainForm.right++;
            if (Move) mainForm.move++;
            if (Stay) mainForm.stay++;
            if (Eat) mainForm.eat++;
            if (Split) mainForm.split++;*/
        }
    }
}
