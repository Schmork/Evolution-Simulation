using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Evolution_Simulation
{
    class InputVector
    {
        private double _scalingFactor = 500;
        public double[] InputsForBrain;

        public InputVector(Creature caller, int energy, int age, Cell[] sensors)
        {
            var types = System.Reflection.Assembly.GetCallingAssembly().GetTypes()
                .Where(t => t.IsSubclassOf(typeof(Cell)) && !t.IsAbstract)
                .OrderBy(o => o.Name).ToArray();

            InputsForBrain = new double[sensors.Length * types.Length + 2];

            int n = 0;
            for (int i = 0; i < sensors.Length; i++)
            {
                for (int c = 0; c < types.Count(); c++)
                {
                    n++;
                    if (sensors[i] != null && sensors[i].GetType() == types[c]) InputsForBrain[n] = 1;
                    /*if (sensors[i] != null && sensors[i].GetType() == typeof(Creature))
                    {
                        var creature = (Creature)caller;
                        var other = (Creature)sensors[i];
                        var distance = creature.GeneticDistance(other);
                        InputsForBrain[n] = Normalizer.StayInBounds(distance - 1, 1);
                        var z = 5;
                    }*/
                }
            }

            InputsForBrain[n++] = (energy / _scalingFactor);
            InputsForBrain[n++] = (age / _scalingFactor);
        }
    }
}
