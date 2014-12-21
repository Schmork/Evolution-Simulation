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
    public partial class MainForm : Form
    {
        private Display _display;
        private World _world;
        private Random _rnd;
        public PopulationDynamicsGraph PopGraph;
        
        private double _tps;
        public double TpS { set { _tps = value; lblTpS.Text = _tps.ToString(); } }
        
        public MainForm()
        {
            InitializeComponent();
            _rnd = new Random();
            _display = new Display(trBarWidth.Value, trBarHeight.Value, pictureBox);
            
            PopGraph = new PopulationDynamicsGraph();
            grpBoxPopulationDynamics.Controls.Add(PopGraph);
            PopGraph.Dock = DockStyle.Fill;            

            initWorld();
        }

        private void pictureBox_SizeChanged(object sender, EventArgs e)
        {
            _world.ResizeDisplay();
        }

        internal void updateLabelWorldAge(long value)
        {
            lblWorldAge.Text = value.ToString();
        }

        private void btnPlayPause_Click(object sender, EventArgs e)
        {
            if (btnPlayPause.Text == "Pause")
            {
                btnPlayPause.Text = "Continue";
                _world.Timer.Stop();
            }
            else
            {
                btnPlayPause.Text = "Pause";
                _world.Timer.Start();                
            }
            if (Explorer.IsActive) _world.Explorer.BringToFront();
        }

        private void btnReset_Click(object sender, EventArgs e)
        {
            _world.Timer.Stop();
            initWorld();
        }

        private void trBarMinCreatures_Scroll(object sender, EventArgs e)
        {
            updateText();
            World.MinCreatures = trBarMinCreatures.Value;
        }

        private void trBarWidth_Scroll(object sender, EventArgs e)
        {
            updateText();
            btnReset.PerformClick();
        }

        private void trBarHeight_Scroll(object sender, EventArgs e)
        {
            updateText();
            btnReset.PerformClick();
        }

        private void trBarSpeed_Scroll(object sender, EventArgs e)
        {
            updateText();
            _world.Timer.Interval = getTimerInterval();
            World.SkipInterval = getSkipInterval();
        }

        private void trBarMutationChance_Scroll(object sender, EventArgs e)
        {
            updateText();
            Brain.MutationChance = trBarMutationChance.Value;
        }

        private void trBarMutationChange_Scroll(object sender, EventArgs e)
        {
            updateText();
            Brain.MutationChange = trBarMutationChange.Value;
        }

        private void trBarPlantsReplenishment_Scroll(object sender, EventArgs e)
        {
            updateText();
            World.PlantsReplenishment = trBarPlantsReplenishment.Value;
        }

        private void trBarPlantsEnergy_Scroll(object sender, EventArgs e)
        {
            updateText();
            Plant.InitialEnergy = trBarPlantsEnergy.Value;
        }

        private void trBarDeadBodyDecay_Scroll(object sender, EventArgs e)
        {
            updateText();
            World.DeadBodyDecay = trBarDeadBodyDecay.Value;
        }

        private void trBarMaxCreatures_Scroll(object sender, EventArgs e)
        {
            updateText();
            World.MaxCreatures = trBarMaxCreatures.Value;
        }

        private void initWorld()
        {
            updateText();

            Plant.InitialEnergy = trBarPlantsEnergy.Value;
            World.PlantsReplenishment = trBarPlantsReplenishment.Value;
            World.DeadBodyDecay = trBarDeadBodyDecay.Value;
            World.MinCreatures = trBarMinCreatures.Value;
            World.MaxCreatures = trBarMaxCreatures.Value;
            Brain.MutationChance = trBarMutationChance.Value;
            Brain.MutationChange = trBarMutationChange.Value;
            World.SkipInterval = getSkipInterval();

            _world = new World(trBarWidth.Value, trBarHeight.Value, _display, _rnd, this);
            _world.Timer.Interval = getTimerInterval();

            grpBoxPlants.BackColor = ControlPaint.Light(Cell.StandardColor(typeof(Plant)), 50);
            grpBoxCreatures.BackColor = ControlPaint.Light(Creature.GetColor(1000, 100), 70);
        }

        private void updateText()
        {
            grpBoxHeight.Text = grpBoxHeight.Tag.ToString() + trBarHeight.Value;
            grpBoxWidth.Text = grpBoxWidth.Tag.ToString() + trBarWidth.Value;
            grpBoxMinCreatures.Text = grpBoxMinCreatures.Tag.ToString() + trBarMinCreatures.Value;
            grpBoxMaxCreatures.Text = grpBoxMaxCreatures.Tag.ToString() + trBarMaxCreatures.Value;
            grpBoxMutationChange.Text = grpBoxMutationChange.Tag.ToString() + " up to +/- " + trBarMutationChange.Value * Brain.ChangeFactor;
            grpBoxMutationChance.Text = grpBoxMutationChance.Tag.ToString() + trBarMutationChance.Value + " / " + Brain.ChanceDivisor;
            grpBoxPlantsEnergy.Text = grpBoxPlantsEnergy.Tag.ToString() + trBarPlantsEnergy.Value;
            grpBoxPlantsReplenishment.Text = grpBoxPlantsReplenishment.Tag.ToString() + trBarPlantsReplenishment.Value;
            grpBoxBodyDecay.Text = grpBoxBodyDecay.Tag.ToString() + trBarDeadBodyDecay.Value;

            var delay = getTimerInterval();
            var skip = getSkipInterval();
            grpBoxSpeed.Text = grpBoxSpeed.Tag.ToString() + delay + "ms delay / draw " + skip + "th frame";
        }

        private int getTimerInterval()
        {
            var speed = trBarSpeed.Value;
            var middle = trBarSpeed.Maximum - trBarSpeed.Maximum / 2;

            var delay = middle - speed - 1;
            return Transform.PseudoLog(delay);
        }

        private int getSkipInterval()
        {
            var speed = trBarSpeed.Value;
            var middle = trBarSpeed.Maximum - trBarSpeed.Maximum / 2;

            var skip = speed - middle;
            return Transform.PseudoLog(skip);
        }

        private void MainForm_SizeChanged(object sender, EventArgs e)
        {
            if (this.WindowState != FormWindowState.Minimized) splitContainer.SplitterDistance = this.Width - 220;
        }
        
        /// <summary>
        /// Calls the Explorer to display details about the clicked cell.
        /// </summary>
        private void pictureBox_Click(object sender, EventArgs e)
        {
            var p = PointToClient(MousePosition);
            var xy = _display.fromDisplayToWorld(p);
            _world.SetExplorer(xy);
        }
    }
}
