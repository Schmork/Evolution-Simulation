using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

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
            BringToFront();
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

        public void Actualize()
        {
            if (!IsActive) return;

            resetLabels();
            if (_world.TrackedPoint != null)
            {
                var trackedPos = _world.TrackedPoint;
                SetCell(_world.Grid.Get(trackedPos));

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
                _display.refresh();
            }
            else
            {
                lblPosX.Text = "-";
                lblPosY.Text = "-";
            }
        }
    }
}
