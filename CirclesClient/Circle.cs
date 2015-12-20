using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CirclesClient
{
    [Serializable]
    public class Circle
    {
        public int Radius;
        public int Index;
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

        // Returns the distance from the center of this circle to the center of the given circle
        public double DistanceTo(Circle otherCirlce)
        {
            double OutDistance;


            Point DeltaPoint = new Point();
            DeltaPoint.X = otherCirlce.Center.X - this.Center.X;
            DeltaPoint.Y = otherCirlce.Center.Y - this.Center.Y;

            OutDistance = Math.Sqrt((DeltaPoint.X * DeltaPoint.X) + (DeltaPoint.Y * DeltaPoint.Y));

            return OutDistance;
        }
    }
}
