using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;

namespace Evolution_Simulation
{
    public class Display
    {
        private int _cellMargin = 1;
        private int _worldMargin = 3;

        private PictureBox _pictureBox;
        private int _width;
        private int _height;
        private double _scale;

        private Color _freeCellColor;

        public Display(int width, int height, PictureBox pictureBox)
        {
            _pictureBox = pictureBox;
            _width = width;
            _height = height;
            _freeCellColor = _pictureBox.BackColor;

            _pictureBox.Image = new Bitmap(_pictureBox.Width, _pictureBox.Height);
            _scale = getScale(_pictureBox.Width, _pictureBox.Height, _width, _height);
        }

        public void fillCell(XY pos, Color color)
        {   
            var brush = new SolidBrush(color);
            using (var g = Graphics.FromImage(_pictureBox.Image))
            {
                g.FillRectangle(brush, getRect(pos));
            }
        }

        /// <summary>
        /// Outlines the tracked cell for better visibility
        /// </summary>
        public void markCell(XY pos)
        {
            if (pos == null) return; 
            var pen = new Pen(Color.Cyan);
            using (var g = Graphics.FromImage(_pictureBox.Image))
            {
                g.DrawRectangle(pen, getRect(pos));
            }
        }

        private Rectangle getRect(XY pos)
        {
            var point = fromWorldToDisplay(pos);
            var length = _scale - _cellMargin;
            var size = new Size((int)length, (int)length);
            return new Rectangle(point, size);
        }

        private Point fromWorldToDisplay(XY pos)
        {
            var x = (int)(pos.X * _scale) + _worldMargin;
            var y = (int)(pos.Y * _scale) + _worldMargin;
            return new Point(x,y);
        }

        public XY fromDisplayToWorld(Point p)
        {
            var x = (p.X - _worldMargin) / _scale;
            var y = (p.Y - _worldMargin) / _scale;
            return new XY((int)x, (int)y);
        }

        private double getScale(int imageWidth, int imageHeight, int worldWidth, int worldHeight)
        {
            var scaleX = (double)imageWidth / (double)worldWidth;
            var scaleY = (double)imageHeight / (double)worldHeight;
            return (scaleX < scaleY) ? scaleX : scaleY;
        }

        public void Clear()
        {
            _pictureBox.Image = new Bitmap(_pictureBox.Width, _pictureBox.Height);
        }

        public void DrawAll(Grid grid, XY trackedPoint)
        {
            drawCreatures(grid.Creatures);
            drawPlants(grid.Plants);
            drawDeadBodies(grid.DeadBodies);
            drawLava(grid.Lavas);
            markCell(trackedPoint);
            refresh();
        }

        private void drawCreatures(List<Creature> creatures)
        {
            foreach (var creature in creatures)
            {
                fillCell(creature.Pos, creature.Color);
            }
        }

        private void drawPlants(List<Plant> plants)
        {
            foreach (var plant in plants)
            {
                fillCell(plant.Pos, plant.Color);
            }
        }

        private void drawDeadBodies(List<DeadBody> deadBodies)
        {
            foreach (var db in deadBodies)
            {
                fillCell(db.Pos, db.Color);
            }
        }

        private void drawLava(List<Lava> lavas)
        {
            foreach (var l in lavas)
            {
                fillCell(l.Pos, l.Color);
            }
        }

        public void refresh()
        {   
            _pictureBox.Refresh();
        }

        public void resize(Grid grid, XY trackedPoint)
        {
            _pictureBox.Image = new Bitmap(_pictureBox.Width, _pictureBox.Height);
            _scale = getScale(_pictureBox.Width, _pictureBox.Height, Grid.Width, Grid.Height);
            DrawAll(grid, trackedPoint);
            refresh();
        }
    }
}
