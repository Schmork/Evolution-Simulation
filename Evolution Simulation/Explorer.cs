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
        public Cell TargetCell;
        public static bool IsActive;
        private Display _display;
        private String _title = "Explorer - ";
        private int _horizon;

        public Explorer(Cell cell)
        {
            initExplorer();
            SetCell(cell);
        }

        public Explorer(XY pos)
        {
            initExplorer();
            SetCell(pos);
        }
        
        private void initExplorer()
        {
            InitializeComponent();
            _horizon = World.Horizon * 2;
            resetLabels();
            _display = new Display(_horizon * 2 + 1, _horizon * 2 + 1, pictureBoxSurrounding);
            Show();
        }
        
        public void SetCell(Cell cell)
        {
            resetLabels();
            if (cell == null) return;
            SetCell(cell.Pos);
            TargetCell = cell;
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
        }

        public void SetCell(XY pos)
        {
            TargetCell = null;
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
        }

        public void Update(Cell target, Grid grid)
        {
            resetLabels();
            if (target != null)
            {
                SetCell(target);

                _display.Clear();
                for (int dx = -_horizon; dx <= +_horizon; dx++)
                {
                    for (int dy = -_horizon; dy <= +_horizon; dy++)
                    {
                        var x = target.Pos.X + dx;
                        var y = target.Pos.Y + dy;
                        var pos = Normalizer.WrapWorld(new XY(x, y));
                        var cell = grid.Get(pos);

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
