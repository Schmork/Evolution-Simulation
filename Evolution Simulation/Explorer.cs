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
        private XY _centre;

        public Explorer(World world)
        {
            IsActive = true;
            InitializeComponent();
            _world = world;
            resetLabels();
            var size = (World.Horizon + 1) * 2 + 1;
            _centre = new XY(size / 2, size / 2);
            _display = new Display(size, size, pictureBoxSurrounding);
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
            Text = _title + "empty Cell";
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
                for (int dx = -_centre.X; dx <= _centre.X; dx++)
                {
                    for (int dy = -_centre.Y; dy <= _centre.Y; dy++)
                    {
                        var d = new XY(dx, dy);
                        var pos = Transform.WrapWorld(trackedPos + d);
                        var cell = _world.Grid.Get(pos);

                        var color = (cell == null) ? pictureBoxSurrounding.BackColor : cell.Color;

                        _display.fillCell(_centre + d, color);
                    }
                }
                _display.MarkCell(_centre);  // mark the center

                var c = trackedCell as Creature;
                if (c != null)  // is it a creature?
                {
                    for (int s = 1; s <= World.Horizon; s++)
                    {
                        var ahead = c.GetNextPos(s) - c.Pos;
                        var left = c.GetNextPos(c.Direction - 1, s) - c.Pos;
                        var right = c.GetNextPos(c.Direction + 1, s) - c.Pos;
                        _display.MarkCell(_centre + ahead);
                        _display.MarkCell(_centre + left);
                        _display.MarkCell(_centre + right);
                    }
                }
                drawBrain(_world.TrackedCreature);              // should be called even if we do not track a creature: to clear the brain view in this case.

                _display.refresh();
            }
        }

        /// <summary>
        /// Plots the neuronal network of a given creature.
        /// </summary>
        private void drawBrain(Creature creature)
        {
            pictureBoxBrain.Image = new Bitmap(pictureBoxBrain.Width, pictureBoxBrain.Height);      // clear anyways
            if (creature == null) return;                                                           // but only draw brain if we're looking at a creature

            var layerCount = Brain.LayerCount + 1;
            var nodes = new Rectangle[layerCount][];

            var defaultX = 9;

            var y = 10;
            var x = defaultX;

            var height = defaultX * 5 / 2;
            var width = height;

            var layerGap = (pictureBoxBrain.Height - height * layerCount) / layerCount;

            var length = Brain.InputNeuronsCount;
            var inputNodes = new Rectangle[length];
            var neuronGap = (pictureBoxBrain.Width - width * length) / (length + 2);

            for (int i = 0; i < length; i++)
            {
                inputNodes[i] = new Rectangle(x, y, width, height);
                x += width + neuronGap;
            }
            nodes[0] = inputNodes;

            for (int i = 0; i < Brain.LayerCount; i++)
            {
                x = defaultX;
                y += height + layerGap;

                length = Brain.NeuronsPerLayerCount;
                var webNodes = new Rectangle[length];
                neuronGap = (pictureBoxBrain.Width - width * length) / (length + 2);
                for (int j = 0; j < length; j++)
                {
                    webNodes[j] = new Rectangle(x, y, width, height);
                    x += width + neuronGap;
                }
                nodes[i + 1] = webNodes;
            }

            x = defaultX;
            length = Brain.OutputNeuronsCount;
            var outputNodes = new Rectangle[length];
            neuronGap = (pictureBoxBrain.Width - width * length) / (length + 2);
            for (int i = 0; i < length; i++)
            {
                outputNodes[i] = new Rectangle(x, y, width, height);
                x += width + neuronGap;
            }
            nodes[layerCount - 1] = outputNodes;

            for (int layer = 0; layer < layerCount; layer++)
            {
                for (int neuron = 0; neuron < nodes[layer].Length; neuron++)
                {
                    using (var g = Graphics.FromImage(pictureBoxBrain.Image))
                    {
                        var pen = new Pen(Color.Gray);

                        if (layer == 0) // color input neurons according to what sensors see
                        {
                            var p = creature.InputVector.InputsForBrain[neuron];
                            if (p == 1.0) pen.Color = Color.Red;
                        }
                        g.DrawEllipse(pen, nodes[layer][neuron]);
                    }
                }
            }
        }

        private void pictureBoxSurrounding_Click(object sender, EventArgs e)
        {
            var p = PointToClient(MousePosition);
            var xy = _display.fromDisplayToWorld(p) + _world.TrackedPoint - _centre;
            xy = Transform.WrapWorld(xy);
            _world.SetExplorer(xy);
        }
    }
}
