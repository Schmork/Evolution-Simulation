using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Evolution_Simulation
{
    public class Creature : LivingCell
    {
        public int Age, Diet, Splits;
        /// <summary>
        /// Used to display brain input data on explorer window.
        /// </summary>
        public InputVector InputVector;

        private Random _rnd;
        private Brain _brain;

        private int _direction;
        public int Direction
        {
            get { return _direction; }
            set { _direction = Transform.Modulo(value, 4);; }
        }        

        public Creature(XY pos, Random rnd) : this(pos, rnd, null) {}

        private Creature(XY pos, Random rnd, Creature parent)
        {            
            if (parent == null)     // create a random creature
            {
                Energy = 120;
                Diet = 50;
                _brain = new Brain(rnd, null);
            }
            else                    // create a child, based on it's parent
            {
                Energy = parent.Energy / 2;
                Diet = parent.Diet;
                _brain = new Brain(rnd, parent._brain);
            }

            Pos = pos;
            _rnd = rnd;
            Color = GetColor(Energy, Diet);
        }

        public void Eat(LivingCell food) 
        {
            if (food.GetType() == typeof(DeadBody))
            {
                var corpse = (DeadBody)food;
                if (corpse.Energy > Energy)
                {
                    corpse.Energy -= Energy;
                    Energy = 0;
                }
                else
                {
                    Energy -= corpse.Energy;
                    corpse.Energy = 0;
                }
            }
            else
            {
                if (food.GetType() == typeof(Plant))
                {
                    Diet++;
                }
                else if (food.GetType() == typeof(Creature))
                {
                    Diet--;
                    Energy--;       // to prevent perpetuum eating each other
                }

                Energy += food.Energy;
                food.IsAlive = false;
                Diet = Transform.StayBelow(Diet, 100);
            }
        }

        /// <summary>
        /// Gets coordinates of where this creature would be in the next step
        /// </summary>
        public XY GetNextPos()
        {
            return GetNextPos(Direction, 1);
        }

        /// <summary>
        /// Gets coordinates of where this creature would be in §range steps
        /// </summary>
        public XY GetNextPos(int range)
        {
            return GetNextPos(Direction, range);
        }

        /// <summary>
        /// Gets coordinates of where this creature would be in §range steps, if walking in §direction
        /// </summary>
        public XY GetNextPos(int direction, int range)
        {
            direction = Transform.Modulo(direction, 4);
            return Grid.directionToXY(Pos, direction, range);
        }

        /// <summary>
        /// Does 3 things: 1) updates brain's InputNeurons, 2) runs the neuronal network, 3) uses the network's output to generate an ActionVector
        /// </summary>
        /// <param name="sensors"></param>
        public ActionVector UseBrain(Cell[] sensors)
        {
            InputVector = new InputVector(this, Energy, Age, sensors);
            _brain.UpdateInputs(InputVector);
            return new ActionVector(_brain.GetOutput(), _rnd);
        }

        /// <summary>
        /// Calculates a creature's color based on Energy (more = brighter) and Diet (0 / blue = plants only, 100 / red = meat only).
        /// </summary>
        public static System.Drawing.Color GetColor(int energy, int diet)
        {
            var max = 255;
            var blue = diet * max / 100;
            var red = (100 - diet) * max / 100;
            var alpha = energy / 2 + 40;
            if (alpha > max) alpha = max;
            if (alpha < 70) alpha = 70;
            return System.Drawing.Color.FromArgb(alpha, red, 0, blue);
        }

        /// <summary>
        /// This is how creatures reproduce: Asexual, by splitting themselves into two, like bacteria. Adjustable mutation included.
        /// </summary>
        public Creature Split(XY freePos)
        {
            Splits++;
            Energy--;
            var child = new Creature(freePos, _rnd, this);
            Energy -= child.Energy;
            /*var distance = GeneticDistance(child);
            if (distance > 0.00001)
            {
                var z = 5;
            }/*/
            return child;
        }

        /// <summary>
        /// Performant hack to check how closely related two creatures are (0 = identical, 1 = max apart). Based on diet only, so not very accurate.
        /// </summary>
        public static double GetGeneticDistance(Creature a, Creature b)
        {
            return (b.Diet - a.Diet) / 100.0;
        }
    }
}
