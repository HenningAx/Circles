using System;
using System.Collections.Generic;
using System.Windows;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CirclesClient
{
    class Circle
    {
        public int Radius;
        public Point Center;
        private Color CircleColor;

        public Circle(int r, Point p, Color col)
        {
            Radius = r;
            Center = p;
            CircleColor = col;
        }

        public Circle()
        {
            Radius = 50;
            Center = new Point(200, 200);
            CircleColor = Color.Red;
        }

        public Color GetCircleColor()
        {
            return CircleColor;
        }
    }
}
