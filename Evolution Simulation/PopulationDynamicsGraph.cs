using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Evolution_Simulation
{
    public partial class PopulationDynamicsGraph : UserControl
    {
        public PictureBox PictureBox;

        /// <summary>
        /// Internal margin - a border surrounding the Graph where we won't draw.
        /// </summary>
        private int _margin = 6;

        /// <summary>
        /// Stores the last quantity of creatures eating plants, everything, meat, and of plants and dead bodies.
        /// </summary>
        private int _vegi, _omni, _meat, _plants, _deadbodies;

        /// <summary>
        /// At which X to overwrite?
        /// </summary>
        private int _currentX;

        /// <summary>
        /// How many pixels should be cleared ahead of ongoing writing?
        /// </summary>
        private int _toClear = 10;

        /// <summary>
        /// Colors used to draw Plants, Creatures, Dead Bodies, ...
        /// </summary>
        private Color _plantCol, _vegiCol, _omniCol, _meatCol, _deadBCol;

        private Font _font = new Font("Helvetica", 10);
        private int _headlineHeight, _headlineWidth;

        public PopulationDynamicsGraph()
        {
            InitializeComponent();
            PictureBox = pictureBox;
            _currentX = _margin;
            _plantCol = Cell.StandardColor(typeof(Plant));
            _deadBCol = Cell.StandardColor(typeof(DeadBody));
            _vegiCol = Creature.GetColor(10000, 100);
            _omniCol = Creature.GetColor(10000, 50);
            _meatCol = Creature.GetColor(10000, 0);
        }

        public void Update(Grid grid)
        {
            clearAhead(_currentX, _currentX + _toClear);

            var scaledPlants = grid.Plants.Count / 15;
            drawLine(_plants, scaledPlants, _plantCol);
            _plants = scaledPlants;

            var vegi = grid.Creatures.Count(x => x.Diet > 66);
            var omni = grid.Creatures.Count(x => x.Diet <= 66 && x.Diet >= 33);
            var meat = grid.Creatures.Count(x => x.Diet < 33);

            drawLine(_vegi, vegi, _vegiCol);
            _vegi = vegi;
            drawLine(_omni, omni, _omniCol);
            _omni = omni;
            drawLine(_meat, meat, _meatCol);
            _meat = meat;

            drawLine(_deadbodies, grid.DeadBodies.Count, _deadBCol);
            _deadbodies = grid.DeadBodies.Count;

            _currentX++;
            if (_currentX > pictureBox.Width - _margin) _currentX = _margin;

            #region print numbers
            using (var g = Graphics.FromImage(pictureBox.Image))
            {
                #region clear previous text
                var size = new Size(new Point(pictureBox.Width, _headlineHeight * 2));
                var rec = new Rectangle(new Point(0, 0), size);
                g.FillRectangle(new SolidBrush(pictureBox.BackColor), rec);
                #endregion

                var brush = new SolidBrush(_plantCol);
                var text = grid.Plants.Count.ToString();
                var loc = new Point(0, _headlineHeight);
                g.DrawString(text, _font, brush, loc);

                brush.Color = _vegiCol;
                text = vegi.ToString();
                loc.X += _headlineWidth;
                g.DrawString(text, _font, brush, loc);

                brush.Color = _omniCol;
                text = omni.ToString();
                loc.X += _headlineWidth;
                g.DrawString(text, _font, brush, loc);
                
                brush.Color = _meatCol;
                text = meat.ToString();
                loc.X += _headlineWidth;
                g.DrawString(text, _font, brush, loc);
                
                brush.Color = _deadBCol;
                text = grid.DeadBodies.Count.ToString();
                loc.X += _headlineWidth;
                g.DrawString(text, _font, brush, loc);
            }
            #endregion
            writeHeadlines();

            pictureBox.Refresh();
        }

        private void writeHeadlines()
        {
            _headlineWidth = pictureBox.Width / 5;
            using (var g = Graphics.FromImage(pictureBox.Image))
            {
                var brush = new SolidBrush(_plantCol);
                var text = "plant";
                var loc = new Point(0, 0);
                g.DrawString(text, _font, brush, loc);

                brush.Color = _vegiCol;
                loc.X += _headlineWidth;
                text = "vegi";
                g.DrawString(text, _font, brush, loc);

                brush.Color = _omniCol;
                loc.X += _headlineWidth;
                text = "omni";
                g.DrawString(text, _font, brush, loc);

                brush.Color = _meatCol;
                loc.X += _headlineWidth;
                text = "meat";
                g.DrawString(text, _font, brush, loc);

                brush.Color = _deadBCol;
                loc.X += _headlineWidth;
                text = "dead";
                g.DrawString(text, _font, brush, loc);

                var size = g.MeasureString(text, _font);
                _headlineHeight = (int)size.Height;
            }
        }

        /// <summary>
        /// Clears a thin Rectangle ahead of the current writing position for better readability
        /// </summary>
        private void clearAhead(int start, int end)
        {
            // if the to-be-cleared rectangle extends beyond the pictureBox1, clear two rectangles; one to the right, one to the left.
            if (end > pictureBox.Width - _margin) {
                clearAhead(_margin - 1, end - (pictureBox.Width - _margin));
                end = pictureBox.Width - _margin + 1;
            }

            using (var g = Graphics.FromImage(pictureBox.Image))
            {
                var brush = new SolidBrush(pictureBox.BackColor);
                var rect = new Rectangle(start, _headlineHeight * 2, end - start, pictureBox.Height);

                g.FillRectangle(brush, rect);
            }
        }

        private void drawLine(int oldValue, int newValue, Color color)
        {
            using (var g = Graphics.FromImage(pictureBox.Image))
            {
                var pen = new Pen(color);
                g.DrawLine(pen, _currentX - 1, getYFromValue(oldValue), _currentX, getYFromValue(newValue));
            }
        }

        private int getYFromValue(int value)
        {
            return pictureBox.Height - (value * pictureBox.Height - _margin * 2) / 800;
        }

        /// <summary>
        /// Headlines have to be redrawn and their horizontal spacing needs recalculation.
        /// </summary>
        private void pictureBox_SizeChanged(object sender, EventArgs e)
        {
            pictureBox.Image = new Bitmap(pictureBox.Width, pictureBox.Height);
            writeHeadlines();
        }
    }
}
