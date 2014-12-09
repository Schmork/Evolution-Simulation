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

        private Random _rnd;
        private Brain _brain;

        private int _direction;
        public int Direction
        {
            get { return _direction; }
            set { _direction = value % 4; }
        }        

        public Creature(XY pos, Random rnd) : this(pos, rnd, null)
        {
        }

        private Creature(XY pos, Random rnd, Creature parent)
        {            
            if (parent == null)
            {
                Energy = 100;
                Diet = 50;
                _brain = new Brain(rnd, null);
            }
            else
            {
                Energy = parent.Energy / 2;
                Diet = parent.Diet;
                _brain = new Brain(rnd, parent._brain);
            }

            Pos = pos;
            _rnd = rnd;
            Color = GetColor();
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
                Diet = Normalizer.StayBelow(Diet, 100);
            }
        }

        public XY GetNextPos()
        {
            return GetNextPos(Direction, 1);
        }

        public XY GetNextPos(int range)
        {
            return GetNextPos(Direction, range);
        }

        public XY GetNextPos(int direction, int range)
        {
            return Grid.directionToXY(Pos, direction, range);
        }

        public ActionVector UseBrain(Cell[] sensors, MainForm mainForm)
        {
            _brain.UpdateInputs(new InputVector(this, Energy, Age, sensors));
            return new ActionVector(_brain.GetOutput(), _rnd, mainForm);
        }

        public System.Drawing.Color GetColor()
        {
            var max = 255;
            var blue = Diet * max / 100;
            var red = (100 - Diet) * max / 100;
            var alpha = Energy / 2 + 40;
            if (alpha > max) alpha = max;
            if (alpha < 70) alpha = 70;
            return System.Drawing.Color.FromArgb(alpha, red, 0, blue);
        }

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

        public double GeneticDistance(Creature c)
        {
            return _brain.GeneticDistance(c._brain);
        }
    }
}
