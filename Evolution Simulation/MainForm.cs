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

        private int[] _value;

        private double _tickFrequency;

        public double TickFrequency
        {
            get { return _tickFrequency; }
            set { _tickFrequency = value; lblTickFrequency.Text = _tickFrequency.ToString() + lblTickFrequency.Tag; }
        }

        public int move
        {
            get { return _value[0]; }
            set { _value[0] = value; lbl1.Text = _value[0].ToString(); }
        }
        public int right
        {
            get { return _value[1]; }
            set
            {
                _value[1] = value;
                lbl2.Text = _value[1].ToString();
            }
        }
        public int eat
        {
            get { return _value[2]; }
            set
            {
                _value[2] = value;
                lbl3.Text = _value[2].ToString();
            }
        }
        public int left
        {
            get { return _value[3]; }
            set
            {
                _value[3] = value;
                lbl4.Text = _value[3].ToString();
            }
        }
        public int split
        {
            get { return _value[4]; }
            set
            {
                _value[4] = value;
                lbl5.Text = _value[4].ToString();
            }
        }

        public int stay
        {
            get { return _value[5]; }
            set
            {
                _value[5] = value;
                lbl6.Text = _value[5].ToString();
            }
        }

        public MainForm()
        {
            InitializeComponent();
            _value = new int[6];

            _rnd = new Random();
            _display = new Display(trBarWidth.Value, trBarHeight.Value, pictureBox);
            initWorld();
        }

        void pictureBox_MouseWheel(object sender, MouseEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void pictureBox_SizeChanged(object sender, EventArgs e)
        {
            _world.ResizeDisplay();
        }

        internal void updateLabelCreatureCount(int value)
        {
            lblCreatureCount.Text = value.ToString();
        }

        internal void updateLabelWorldAge(int value)
        {
            lblWorldAge.Text = value.ToString();
        }

        internal void updateLabelPlantsCount(int value)
        {
            lblPlantsCount.Text = value.ToString();
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

            var dummyPlant = Plant.Create(new XY(0, 0));
            grpBoxPlants.BackColor = ControlPaint.Light(dummyPlant.Color, 50);

            var dummyCreature = new Creature(new XY(0, 0), _rnd);
            dummyCreature.Diet = 100;
            dummyCreature.Color = dummyCreature.GetColor();
            grpBoxCreatures.BackColor = ControlPaint.Light(dummyCreature.Color, 70);
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
            return pseudoLog(delay);
        }

        private int getSkipInterval()
        {
            var speed = trBarSpeed.Value;
            var middle = trBarSpeed.Maximum - trBarSpeed.Maximum / 2;

            var skip = speed - middle;
            return pseudoLog(skip);
        }

        private void MainForm_SizeChanged(object sender, EventArgs e)
        {
            if (this.WindowState != FormWindowState.Minimized) splitContainer.SplitterDistance = this.Width - 220;
        }

        private int pseudoLog(int value)
        {
            if (value <= 1) return 1;
            var factor = (value / 5) * 0.1;
            int ret = (int)(factor * value);
            if (ret < 1) ret = 1;
            return ret;
        }

        private void pictureBox_Click(object sender, EventArgs e)
        {
            var p = PointToClient(MousePosition);
            var xy = _display.fromDisplayToWorld(p);
            _world.SetExplorer(xy);
        }
    }
}
