using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Evolution_Simulation
{
    /// <summary>
    /// Basically an Array with logic how to fill it. Takes data from a creature and it's surrounding world and prepares the data so it can be used as input for the creature's brain.
    /// </summary>
    public class InputVector
    {
        private double _scalingFactor = 500;
        /// <summary>
        /// Either 0 or 1 for sensor data, or between 0 and 1 for continuous values like energy and age.
        /// </summary>
        public double[] InputsForBrain;

        /// <summary>
        /// All types a creature can see - Lava, Plant, Creature and DeadBody
        /// </summary>
        public static Type[] Types = System.Reflection.Assembly.GetCallingAssembly().GetTypes()
                .Where(t => t.IsSubclassOf(typeof(Cell)) && !t.IsAbstract)
                .OrderBy(o => o.Name).ToArray();

        public InputVector(Creature caller, int energy, int age, Cell[] sensors)
        {
            InputsForBrain = new double[sensors.Length * Types.Length + 2];

            int n = 0;
            for (int i = 0; i < sensors.Length; i++)        // cycle sensors array
            {
                for (int c = 0; c < Types.Count(); c++)     // cycle possible types to check which type this sensor sees
                {
                    n++;

                    var spotted = sensors[i] as Creature;
                    // special treatment if we spotted a creature - friend or foe? eat or run?
                    if (spotted != null)
                    {
                        InputsForBrain[n] = Creature.GetGeneticDistance(caller, spotted);
                    }
                    else
                    {
                        if (sensors[i] != null && sensors[i].GetType() == Types[c]) InputsForBrain[n] = 1;
                    }
                }
            }

            InputsForBrain[n++] = (energy / _scalingFactor);
            InputsForBrain[n++] = (age / _scalingFactor);
        }
    }
}
