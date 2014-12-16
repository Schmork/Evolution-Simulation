using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics; // for stopwatch
using System.Windows.Forms; // for timer

namespace Evolution_Simulation
{
    public class World
    {
        public Timer Timer;
        public Grid Grid;
        private Display _display;
        private Random _rnd;
        private MainForm _mainForm;
        private Explorer _explorer;
        private Stopwatch _stopwatch;

        public static int SkipInterval, PlantsReplenishment, DeadBodyDecay, MinCreatures, MaxCreatures;
        private int _worldAge;
        private int _punishment = 10;       // this amount of energy is substracted when creatures attempt illegal actions (like eating when there is nothing to eat, or moving when blocked)
        public static int Horizon = 2;      // creatures can see so many pixels in three directions

        private Creature _trackedCreature;  // stores the answer on two questions: do we track a creature? if so, which one?
        private XY _trackedPoint;
        public XY TrackedPoint              // the seperate explorer window will track this point. If initialized on a Creature, TrackedPoint is meant to move with that creature.
        {
            get { return _trackedPoint; }
            set
            {
                if (value == null)
                {
                    _trackedCreature = null;
                    _trackedPoint = null;
                }
                else
                {
                    _trackedPoint = value;
                }
            }
        }

        public World(int width, int height, Display display, Random rnd, MainForm mainForm)
        {
            _display = display;
            _rnd = rnd;
            _mainForm = mainForm;

            ActionVector.OutputCount = typeof(ActionVector).GetFields().Count();

            Grid = new Grid(width, height, _rnd);
            populate(MinCreatures, 0);
            ResizeDisplay();

            _stopwatch = new Stopwatch();
            _stopwatch.Start();

            Timer = new Timer();
            Timer.Tick += Timer_Tick;
            Timer.Start();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            _worldAge++;

            for (int n = Grid.Creatures.Count - 1; n >= 0; n--)
            {
                var c = Grid.Creatures[n];

                c.Age++;
                c.Energy--;

                var sensors = new Cell[3 * Horizon];
                XY ahead, right, left;

                var i = 0;
                for (int s = 0; s < Horizon; s++)
                {
                    ahead = c.GetNextPos(s);
                    left = c.GetNextPos(c.Direction - 1, s);
                    right = c.GetNextPos(c.Direction + 1, s);

                    sensors[i++] = Grid.Get(ahead);
                    sensors[i++] = Grid.Get(left);
                    sensors[i++] = Grid.Get(right);
                }

                var action = c.UseBrain(sensors, _mainForm);

                if (action.Left) c.Direction--;
                if (action.Right) c.Direction++;
                if (action.Stay) { } // do nothing 
                if (action.Split) splitCreature(c);
                if (action.Move) moveCreature(c);
                if (action.Eat) eatSomething(c);

                c.Color = c.GetColor();
            }

            foreach (var body in Grid.DeadBodies)
            {
                body.Energy--;
            }

            Grid.RemoveDeadCreatures(DeadBodyDecay);
            Grid.RemoveDeadPlants();
            Grid.RemoveRottenCorpses();

            for (int n = 0; n < PlantsReplenishment; n++) Grid.Add(typeof(Plant));
            while (Grid.Creatures.Count < MinCreatures
                && Grid.Creatures.Count < MaxCreatures) Grid.Add(typeof(Creature));

            if (_worldAge % SkipInterval == 0)
            {
                _display.Clear();
                _display.DrawAll(Grid, TrackedPoint);
                _display.refresh();

                if (_trackedCreature != null) TrackedPoint = _trackedCreature.Pos;
                if (_explorer != null) _explorer.Actualize();

                _mainForm.updateLabelCreatureCount(Grid.Creatures.Count);
                _mainForm.updateLabelPlantsCount(Grid.Plants.Count);
                _mainForm.updateLabelWorldAge(_worldAge);
                if (_stopwatch.ElapsedMilliseconds != 0)
                {
                    _mainForm.TpS = SkipInterval * 1000 / _stopwatch.ElapsedMilliseconds;
                    _stopwatch.Restart();
                }
            }
        }

        private void eatSomething(Creature c)
        {
            var food = Grid.Get(c.GetNextPos());
            if (food == null)
            {
                c.Energy -= _punishment;
            }
            else if (food.GetType() == typeof(Creature))
            {
                var prey = (Creature)food;
                c.Eat(prey);
            }
            else if (food.GetType() == typeof(Plant))
            {
                var plant = (Plant)food;
                c.Eat(plant);
            }
            else if (food.GetType() == typeof(DeadBody))
            {
                var corpse = (DeadBody)food;
                c.Eat(corpse);
            }
            else c.Energy -= _punishment;
        }

        private void moveCreature(Creature c)
        {
            if (Grid.IsEmpty(c.GetNextPos()))
            {
                Grid.Move(c);
            }
            else
            {
                c.Energy -= _punishment;
            }
        }

        private void splitCreature(Creature c)
        {
            var freeAdjacentPoint = Grid.GetFreeAdjacentPoint(c.Pos);
            if (freeAdjacentPoint == null || Grid.Creatures.Count > MaxCreatures) return;

            var child = c.Split(freeAdjacentPoint);
            Grid.Add(child);
        }

        public void ResizeDisplay()
        {
            if (Explorer.IsActive) _explorer.Close();
            TrackedPoint = null;
            _display.resize(Grid, TrackedPoint);
        }

        private void populate(int creatures, int lavas)
        {
            for (int i = 0; i < creatures; i++)
            {
                Grid.Add(typeof(Creature));
            }
            for (int i = 0; i < Grid.Width * Grid.Height / 10; i++)
            {
                Grid.Add(typeof(Plant));
            }

            for (int i = 0; i < lavas; i++)
            {
                Grid.Add(typeof(Lava));
            }
        }

        public void SetExplorer(XY pos)
        {
            TrackedPoint = pos;
            _trackedCreature = Grid.Get(pos) as Creature;

            if (Explorer.IsActive)
            {
                _explorer.SetCell(pos);
            }
            else
            {
                _explorer = new Explorer(this);
            }
        }
    }
}
