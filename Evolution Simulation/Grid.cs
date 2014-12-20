using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Evolution_Simulation
{
    public class Grid
    {
        /// <summary>
        /// List of all creatures. They eat plants, move, split, have brains, do stuff or don't, and eventually die.
        /// </summary>
        public List<Creature> Creatures;
        /// <summary>
        /// List of all plants. They're constantly replenished based on "Plants added per tick", have some energy, and get eaten by creatures.
        /// </summary>
        public List<Plant> Plants;
        /// <summary>
        /// List of all lava tiles. Lava is static, cannot be created or destroyed, and kills Creatures instantly.
        /// </summary>
        public List<Lava> Lavas;
        /// <summary>
        /// List of all dead bodies. If DeadBodyCount > 0, dying creatures become dead bodys instead of simply vanishing. Dead bodies decay over time and take energy when eaten (opposite of plants, which give energy)
        /// </summary>
        public List<DeadBody> DeadBodies;
        
        /// <summary>
        /// Array of all cells or tiles of this world. Each can be empty (= null), a creature, a plant, a lava or a dead body.
        /// </summary>
        public Cell[,] Cells;

        /// <summary>
        /// Dimensions of this 2-dimensional world.
        /// </summary>
        public static int Width, Height;

        private Random _rnd;
        /// <summary>
        /// When searching for an unoccupied cell, this is the maximum of attempts to find one.
        /// </summary>
        private int _maxAttempts = 30;

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

        /// <summary>
        /// returns the position which is >range< cells away from >pos<, in direction >dir<. 
        /// </summary>
        public static XY directionToXY(XY pos, int dir, int range)
        {
            var d = new XY(0, 0);
            switch (dir)
            {
                case 0: d.Y = -range; break;
                case 1: d.X = +range; break;
                case 2: d.Y = +range; break;
                case 3: d.X = -range; break;
            }
            return Transform.WrapWorld(pos + d);
        }
    }
}
