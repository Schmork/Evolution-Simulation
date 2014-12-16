using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;       // to draw the Brain

namespace Evolution_Simulation
{
    public partial class Explorer : Form
    {
        public static bool IsActive;
        private World _world;
        private Display _display;
        private String _title = "Explorer - ";
        private int _horizon;

        public Explorer(World world)
        {
            IsActive = true;
            InitializeComponent();
            _world = world;
            _horizon = World.Horizon * 2;
            resetLabels();
            _display = new Display(_horizon * 2 + 1, _horizon * 2 + 1, pictureBoxSurrounding);
            Show();
        }

        private void SetCell(Cell cell)
        {
            if (cell == null) return;

            SetCell(cell.Pos);
            #region label display logic
            if (cell.GetType() == typeof(Creature))
            {
                var creature = (Creature)cell;
                Text = _title + creature.GetType().Name;
                lblAge.Text = creature.Age.ToString();
                lblDiet.Text = creature.Diet.ToString();
                lblEnergy.Text = creature.Energy.ToString();
                lblSplits.Text = creature.Splits.ToString();
            }
            else if (cell.GetType() == typeof(Plant))
            {
                var plant = (Plant)cell;
                Text = _title + plant.GetType().Name;
                lblEnergy.Text = plant.Energy.ToString();
            }
            else if (cell.GetType() == typeof(DeadBody))
            {
                var corpse = (DeadBody)cell;
                Text = _title + corpse.GetType().Name;
                lblEnergy.Text = corpse.Energy.ToString();
            }
            else if (cell.GetType() == typeof(Lava))
            {
                var lava = (Lava)cell;
                Text = _title + lava.GetType().Name;
            }
            #endregion
        }

        public void SetCell(XY pos)
        {
            resetLabels();
            lblPosX.Text = pos.X.ToString();
            lblPosY.Text = pos.Y.ToString();
        }

        private void resetLabels()
        {
            Text = _title + "null";
            lblAge.Text = lblAge.Tag.ToString();
            lblEnergy.Text = lblEnergy.Tag.ToString();
            lblDiet.Text = lblDiet.Tag.ToString();
            lblSplits.Text = lblSplits.Tag.ToString();
        }

        private void Explorer_FormClosed(object sender, FormClosedEventArgs e)
        {
            IsActive = false;
            _world.TrackedPoint = null;
        }

        /// <summary>
        /// Updates the explorer (graphics and labels).
        /// </summary>
        public void Actualize()
        {
            if (!IsActive) return;

            resetLabels();
            if (_world.TrackedPoint == null)        // can be null when a creature or plant was tracked which ceased to exist.
            {
                lblPosX.Text = "-";
                lblPosY.Text = "-";
            }
            else
            {
                var trackedPos = _world.TrackedPoint;
                var trackedCell = _world.Grid.Get(trackedPos);

                if (trackedCell == null)
                {
                    SetCell(trackedPos);
                }
                else SetCell(trackedCell);

                _display.Clear();
                for (int dx = -_horizon; dx <= +_horizon; dx++)
                {
                    for (int dy = -_horizon; dy <= +_horizon; dy++)
                    {
                        var x = trackedPos.X + dx;
                        var y = trackedPos.Y + dy;
                        var pos = Normalizer.WrapWorld(new XY(x, y));
                        var cell = _world.Grid.Get(pos);

                        var color = (cell == null) ? pictureBoxSurrounding.BackColor : cell.Color;

                        _display.fillCell(new XY(dx + _horizon, dy + _horizon), color);
                    }
                }
                _display.markCell(new XY(_horizon, _horizon));
                _display.refresh();

                drawBrain(_world.TrackedCreature);
            }
        }

        private void drawBrain(Creature creature)
        {
            if (creature == null) return;

            var layerCount = Brain.LayerCount + 1;
            var nodes = new Rectangle[layerCount][];
            var inputNodes = new Rectangle[Brain.InputNeuronsCount];
            nodes[0] = inputNodes;

            for (int i = 0; i < Brain.LayerCount; i++)
            {
                var webNodes = new Rectangle[Brain.NeuronsPerLayerCount];
                nodes[i + 1] = webNodes;
            }

            var outputNodes = new Rectangle[Brain.OutputNeuronsCount];
            nodes[layerCount - 1] = outputNodes;

            pictureBoxBrain.Image = new Bitmap(pictureBoxBrain.Width, pictureBoxBrain.Height);

            var layerGap = pictureBoxBrain.Height / (layerCount * 3 / 2 + (layerCount - 1));
            var height = layerGap * 3 / 2;

            var y = 10;

            for (int layer = 0; layer < layerCount; layer++)
            {
                var neuronGap = pictureBoxBrain.Width / (layerCount * 3 / 2 + (layerCount - 1));
                var width = neuronGap * 3 / 2;

                y += height + layerGap;

                var x = 10;

                for (int neuron = 0; neuron < nodes[layer].Length; neuron++)
                {
                    x += width + neuronGap;

                    using (var g = Graphics.FromImage(pictureBoxBrain.Image))
                    {
                        var pen = new Pen(Color.Gray);
                        g.DrawEllipse(pen, nodes[layer][neuron]);
                    }
                }
            }
        }

        private void pictureBoxSurrounding_Click(object sender, EventArgs e)
        {
            var p = PointToClient(MousePosition);
            var xy = _display.fromDisplayToWorld(p) + _world.TrackedPoint;
            xy += new XY(-_horizon, -_horizon);
            xy = Normalizer.WrapWorld(xy);
            _world.SetExplorer(xy);
        }
    }
}
