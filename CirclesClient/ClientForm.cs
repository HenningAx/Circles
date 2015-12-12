using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CirclesClient
{
    enum DrawingMode
    {
        none,
        draw,
        move,
        delete,
    }

    public partial class ClientForm : Form
    {
        private List<Circle> CircleList = new List<Circle>();
        private DrawingMode DrawMode = DrawingMode.draw;
        Point DrawStartPoint = new Point();
        bool bIsDrawingCircle = false;

        int CurrentCircleIndex = 0;

        public ClientForm()
        {
            InitializeComponent();
        }

        private void bt_Move_Click(object sender, EventArgs e)
        {
            DrawMode = DrawingMode.move;
            lb_ModeDisplay.Text = "Move Mode";
        }

        private void bt_Delete_Click(object sender, EventArgs e)
        {
            DrawMode = DrawingMode.delete;
            lb_ModeDisplay.Text = "Delete Mode";
        }

        private void bt_Draw_Click(object sender, EventArgs e)
        {
            DrawMode = DrawingMode.draw;
            lb_ModeDisplay.Text = "Draw Mode";
        }

        private Rectangle ComputeCircleBounds(Point Center, int Radius)
        {
            Rectangle OutRect = new Rectangle();

            OutRect.X = Center.X - Radius;
            OutRect.Y = Center.Y - Radius;
            OutRect.Width = Radius * 2;
            OutRect.Height = Radius * 2;

            return OutRect;
        }

        private void pn_DrawingPanel_Paint(object sender, PaintEventArgs e)
        {
            foreach (Circle CircleToDraw in CircleList)
            {
                Rectangle CircleBounds = ComputeCircleBounds(CircleToDraw.Center, CircleToDraw.Radius);
                Pen CirclePen = new Pen(CircleToDraw.GetCircleColor());

                e.Graphics.DrawEllipse(CirclePen, CircleBounds);
            }
        }

        private void pn_DrawingPanel_MouseDown(object sender, MouseEventArgs e)
        {
            if (DrawMode == DrawingMode.draw && !bIsDrawingCircle)
            {
                bIsDrawingCircle = true;
                DrawStartPoint = e.Location;

                Circle NewCircle = new Circle(0, DrawStartPoint, Color.Red);
                CircleList.Add(NewCircle);

                CurrentCircleIndex = CircleList.Count - 1;
                pn_DrawingPanel.Invalidate();
            }
        }

        private void pn_DrawingPanel_MouseMove(object sender, MouseEventArgs e)
        {
            if (DrawMode == DrawingMode.draw && bIsDrawingCircle)
            {
                Point DeltaPoint = new Point();
                DeltaPoint.X = e.X - DrawStartPoint.X;
                DeltaPoint.Y = e.Y - DrawStartPoint.Y;

                double MouseMoveDistance = 0;
                MouseMoveDistance = Math.Sqrt((DeltaPoint.X * DeltaPoint.X) + (DeltaPoint.Y * DeltaPoint.Y));

                CircleList[CurrentCircleIndex].Radius = (int) MouseMoveDistance;
                pn_DrawingPanel.Invalidate();
            }
        }

        private void pn_DrawingPanel_MouseUp(object sender, MouseEventArgs e)
        {
            if(DrawMode == DrawingMode.draw && bIsDrawingCircle)
            {
                bIsDrawingCircle = false;
            }
        }
    }
}
