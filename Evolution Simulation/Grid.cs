using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Evolution_Simulation
{
    public class Grid
    {
        public List<Creature> Creatures;
        public List<Plant> Plants;
        public List<Lava> Lavas;
        public List<DeadBody> DeadBodies;

        public Cell[,] Cells;

        public static int Width, Height;

        private Random _rnd;
        private int _maxAttempts = 30;          // how many times to try to random a free point

        public Grid(int width, int height, Random rnd)
        {
            Width = width;
            Height = height;
            _rnd = rnd;
            Creatures = new List<Creature>();
            Plants = new List<Plant>();
            Lavas = new List<Lava>();
            DeadBodies = new List<DeadBody>();
            Cells = new Cell[Width, Height];
        }

        public Cell Get(XY pos)
        {
            return Cells[pos.X, pos.Y];
        }

        public void Add(Type type)
        {
            var pos = tryFreePoint();
            if (IsEmpty(pos)) add(type, pos);
        }
        
        private void add(Type type, XY pos)
        {
            if (type == typeof(Plant))
            {
                addPlant(pos);
            }
            else if (type == typeof(Creature))
            {
                addCreature(pos);
            }
            else if (type == typeof(DeadBody))
            {
                addDeadBody(pos);
            }
            else if (type == typeof(Lava))
            {
                addLava(pos);
            }
        }

        public void Add(Creature c)
        {
            addToCells(c);
            Creatures.Add(c);
        }

        private void addCreature(XY pos)
        {
            var c = new Creature(pos, _rnd);
            addToCells(c);
            Creatures.Add(c);
        }
        
        private void addPlant(XY pos)
        {
            var p = Plant.Create(pos);
            if (p != null)
            {
                addToCells(p);
                Plants.Add(p);
            }
        }

        private void addDeadBody(XY pos)
        {
            var d = new DeadBody(pos, 0);
            addToCells(d);
            DeadBodies.Add(d);
        }

        private void addDeadBody(DeadBody deadBody)
        {
            addToCells(deadBody);
            DeadBodies.Add(deadBody);
        }

        private void addLava(XY pos)
        {
            var l = new Lava(pos);
            addToCells(l);
            Lavas.Add(l);
        }

        private void addToCells(Cell cell)
        {
            Cells[cell.Pos.X, cell.Pos.Y] = cell;
        }

        private XY tryFreePoint()
        {
            int x, y;
            var n = 0;
            do
            {
                x = _rnd.Next(Width);
                y = _rnd.Next(Height);
            } while (n++ < _maxAttempts && !isEmpty(x, y));
            return new XY(x, y);
        }

        private void remove(Cell cell)
        {
            Plants.Remove(cell as Plant);
            Creatures.Remove(cell as Creature);
            DeadBodies.Remove(cell as DeadBody);

            Cells[cell.Pos.X, cell.Pos.Y] = null;
        }

        private bool isEmpty(int x, int y)
        {
            return Cells[x, y] == null;
        }

        public bool IsEmpty(XY pos)
        {
            return isEmpty(pos.X, pos.Y);
        }

        public void RemoveDeadCreatures(int DeadBodyDecay)
        {
            var morituri = Creatures.Where(c => !c.IsAlive).ToList();

            foreach (var corpse in morituri)
            {
                if (DeadBodyDecay > 0)
                {
                    var zombie = new DeadBody(corpse.Pos, corpse.Energy);
                    addDeadBody(zombie);
                }
                else
                {
                    Cells[corpse.Pos.X, corpse.Pos.Y] = null;
                }

                Creatures.Remove(corpse);
            }
        }

        public void RemoveDeadPlants()
        {
            var morituri = Plants.Where(c => !c.IsAlive).ToList();

            foreach (var deadPlant in morituri)
            {
                remove(deadPlant);
                Plants.Remove(deadPlant);
            }
        }

        public void RemoveRottenCorpses()
        {
            var goners = DeadBodies.Where(d => !d.IsAlive).ToList();
            var stayers = DeadBodies.Where(d => d.IsAlive).ToList();

            foreach (var rotten in goners)
            {
                remove(rotten);
            }

            DeadBodies = stayers;
        }

        public void Move(Creature c)
        {
            Cells[c.Pos.X, c.Pos.Y] = null;
            c.Pos = c.GetNextPos();
            Cells[c.Pos.X, c.Pos.Y] = c;
        }

        public XY GetFreeAdjacentPoint(XY xy)
        {
            XY pos;
            for (int n = 0; n < _maxAttempts; n++)
            {
                pos = directionToXY(xy, _rnd.Next(4), 1);
                if (IsEmpty(pos)) return pos;
            }
            return null;
        }

        public static XY directionToXY(XY pos, int dir, int range)
        {
            int dx = 0, dy = 0;
            switch (dir)
            {
                case 0: dy = -range; break;
                case 1: dx = +range; break;
                case 2: dy = +range; break;
                case 3: dx = -range; break;
            }
            var x = pos.X + dx;
            var y = pos.Y + dy;

            return Normalizer.WrapWorld(new XY(x, y));
        }
    }
}
