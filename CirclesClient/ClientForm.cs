using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

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
        Point LastClickPoint = new Point();

        // Original center of a circle when moving it
        Point OriginalCenter = new Point();

        //True if a circle is being drawn
        bool bIsDrawingCircle = false;

        //True if a circle is being moved
        bool bIsMovingCircle = false;

        //Index of the currently selected circle
        private int SelectedCircleIndex = -1;

        int CurrentDrawCircleIndex = 0;

        // Are we drawing a ghost circle because the current circle is colliding
        bool bDrawGhostCircle = false;

        Circle GhostCircle = new Circle();

        // Socket connection

        IPHostEntry ipHostInfo;
        IPAddress ipAddress;
        IPEndPoint remoteEP;
        Socket senderSocket;

        bool bIsConnected = false;

        public ClientForm()
        {
            InitializeComponent();

            typeof(Panel).InvokeMember("DoubleBuffered", BindingFlags.SetProperty | BindingFlags.Instance | BindingFlags.NonPublic, null, pn_DrawingPanel, new object[] { true });

            SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
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

                Pen CirclePen = new Pen(Color.Black);
                SolidBrush CircleBrush = new SolidBrush(CircleToDraw.GetCircleColor());

                e.Graphics.FillEllipse(CircleBrush, CircleBounds);
                e.Graphics.DrawEllipse(CirclePen, CircleBounds);
            }
            if (bDrawGhostCircle)
            {
                Rectangle GhostBounds = ComputeCircleBounds(GhostCircle.Center, GhostCircle.Radius);
                Color GhostColor = Color.FromArgb(128, 0, 0, 0);

                SolidBrush GhostBrush = new SolidBrush(GhostColor);

                e.Graphics.FillEllipse(GhostBrush, GhostBounds);
            }
        }

        private void pn_DrawingPanel_MouseDown(object sender, MouseEventArgs e)
        {
            if (bIsConnected)
            {
                if (DrawMode == DrawingMode.draw && !bIsDrawingCircle)
                {
                    LastClickPoint = e.Location;
                    Circle NewCircle = new Circle(0, LastClickPoint, pn_ColorSelectPanel.BackColor);
                    NewCircle.Index = CircleList.Count;

                    if (SendCircleRequest(NewCircle))
                    {
                        bIsDrawingCircle = true;
                        CircleList.Add(NewCircle);

                        CurrentDrawCircleIndex = NewCircle.Index;
                        pn_DrawingPanel.Invalidate();
                    }

                }
                else if (DrawMode == DrawingMode.move)
                {
                    SelectedCircleIndex = SelectCircle(e.Location);
                    if (SelectedCircleIndex > -1)
                    {
                        OriginalCenter = CircleList[SelectedCircleIndex].Center;
                    }
                    LastClickPoint = e.Location;
                }
                else if (DrawMode == DrawingMode.delete)
                {
                    SelectedCircleIndex = SelectCircle(e.Location);
                    if (SelectedCircleIndex > -1)
                    {
                        int CacheRadius = CircleList[SelectedCircleIndex].Radius;
                        CircleList[SelectedCircleIndex].Radius = -1;

                        if (SendCircleRequest(CircleList[SelectedCircleIndex]))
                        {
                            CircleList.RemoveAt(SelectedCircleIndex);

                            // Reset the index variable of the remaining circles   
                            for (int i = 0; i < CircleList.Count; i++)
                            {
                                CircleList[i].Index = i;
                            }

                            SelectedCircleIndex = -1;
                            pn_DrawingPanel.Invalidate();
                        }
                        else
                        {
                            CircleList[SelectedCircleIndex].Radius = CacheRadius;
                            SelectedCircleIndex = -1;
                        }

                    }
                }
            }
        }

        private void pn_DrawingPanel_MouseMove(object sender, MouseEventArgs e)
        {
            if (DrawMode == DrawingMode.draw && bIsDrawingCircle)
            {
                int CacheRadius = CircleList[CurrentDrawCircleIndex].Radius;
                double MouseMoveDistance = 0;
                MouseMoveDistance = Distance(e.Location, LastClickPoint);

                CircleList[CurrentDrawCircleIndex].Radius = (int)MouseMoveDistance;

                if (!SendCircleRequest(CircleList[CurrentDrawCircleIndex]))
                {
                    GhostCircle.Center = CircleList[CurrentDrawCircleIndex].Center;
                    GhostCircle.Radius = CircleList[CurrentDrawCircleIndex].Radius;

                    CircleList[CurrentDrawCircleIndex].Radius = CacheRadius;
                    bDrawGhostCircle = true;
                    pn_DrawingPanel.Invalidate();
                }
                else
                {
                    bDrawGhostCircle = false;
                    pn_DrawingPanel.Invalidate();
                }
            }
            else if (DrawMode == DrawingMode.move && SelectedCircleIndex > -1)
            {
                Point MouseMovePoint = new Point();
                MouseMovePoint.X = e.X - LastClickPoint.X;
                MouseMovePoint.Y = e.Y - LastClickPoint.Y;

                Point NewCenter = new Point();
                NewCenter.X = OriginalCenter.X + MouseMovePoint.X;
                NewCenter.Y = OriginalCenter.Y + MouseMovePoint.Y;

                Point CacheCenter = CircleList[SelectedCircleIndex].Center;
                CircleList[SelectedCircleIndex].Center = NewCenter;

                if (!SendCircleRequest(CircleList[SelectedCircleIndex]))
                {
                    GhostCircle.Center = CircleList[SelectedCircleIndex].Center;
                    GhostCircle.Radius = CircleList[SelectedCircleIndex].Radius;

                    bDrawGhostCircle = true;

                    CircleList[SelectedCircleIndex].Center = CacheCenter;
                    pn_DrawingPanel.Invalidate();
                }
                else
                {
                    bDrawGhostCircle = false;
                    pn_DrawingPanel.Invalidate();
                }
            }
        }

        private void pn_DrawingPanel_MouseUp(object sender, MouseEventArgs e)
        {
            if (DrawMode == DrawingMode.draw && bIsDrawingCircle)
            {
                bIsDrawingCircle = false;
                bDrawGhostCircle = false;
                pn_DrawingPanel.Invalidate();
            }
            else if (DrawMode == DrawingMode.move && SelectedCircleIndex > -1)
            {
                SelectedCircleIndex = -1;
                bDrawGhostCircle = false;
                pn_DrawingPanel.Invalidate();
            }
        }

        //Returns the index of the circle under the Selection Point, returns -1 if there is no circle
        private int SelectCircle(Point SelectionPoint)
        {
            int OutIndex = -1;
            for (int i = 0; i < CircleList.Count; i++)
            {
                double MouseToCircleDistance;

                MouseToCircleDistance = Distance(SelectionPoint, CircleList[i].Center);

                if (MouseToCircleDistance < CircleList[i].Radius)
                {
                    OutIndex = i;
                    break;
                }
            }
            return OutIndex;
        }

        private double Distance(Point p1, Point p2)
        {
            double OutDistance;


            Point DeltaPoint = new Point();
            DeltaPoint.X = p2.X - p1.X;
            DeltaPoint.Y = p2.Y - p1.Y;

            OutDistance = Math.Sqrt((DeltaPoint.X * DeltaPoint.X) + (DeltaPoint.Y * DeltaPoint.Y));

            return OutDistance;
        }

        private void pn_ColorSelectPanel_MouseDown(object sender, MouseEventArgs e)
        {
            colorDialog_drawColor.ShowDialog();
            pn_ColorSelectPanel.BackColor = colorDialog_drawColor.Color;
        }

        private void ClientForm_Load(object sender, EventArgs e)
        {
            ipHostInfo = Dns.GetHostEntry("");
            ipAddress = ipHostInfo.AddressList[0];
            remoteEP = new IPEndPoint(ipAddress, 11000);         
        }

        private void bt_Connect_Click(object sender, EventArgs e)
        {
            try
            {
                senderSocket = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                senderSocket.Connect(remoteEP);

                MessageBox.Show("Socket connected to {0}" + senderSocket.RemoteEndPoint.ToString());
                bt_Connect.Enabled = false;
                bt_Disconnect.Enabled = true;
                bIsConnected = true;
                bt_Delete.Enabled = true;
                bt_Draw.Enabled = true;
                bt_Move.Enabled = true;

                byte[] bufferBytes = new byte[1];
                int bytesRec = senderSocket.Receive(bufferBytes);

                bool recBool = BitConverter.ToBoolean(bufferBytes, 0);

                if(recBool)
                {
                    byte[] amountBuffer = new byte[sizeof(int)];
                    int amountBytesRec = senderSocket.Receive(amountBuffer);
                    CircleList.Clear();

                    int CirclesToRec = BitConverter.ToInt32(amountBuffer, 0);
                    MessageBox.Show(CirclesToRec.ToString() + " circles loaded");

                    for(int i = 0; i < CirclesToRec; i++)
                    {
                        byte[] circleBuffer = new byte[441];
                        int circleBytesRec = senderSocket.Receive(circleBuffer);

                        Circle receivedCircle;
                        BinaryFormatter formatter = new BinaryFormatter();
                        using (var ms = new MemoryStream(circleBuffer))
                        {
                            receivedCircle = (Circle)formatter.Deserialize(ms);
                        }

                        CircleList.Add(receivedCircle);
                    }
                }

                pn_DrawingPanel.Invalidate();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }

        }

        private bool SendCircleRequest(Circle circleToSend)
        {
            bool outBool = false;

            byte[] bufferBytes = new byte[1];
            byte[] sendBytes = SerializeCircle(circleToSend);

            int bytesSent = senderSocket.Send(sendBytes);
            int bytesRec = senderSocket.Receive(bufferBytes);

            outBool = BitConverter.ToBoolean(bufferBytes, 0);

            return outBool;
        }

        private byte[] SerializeCircle(Circle circleToSerialize)
        {
            BinaryFormatter formatter = new BinaryFormatter();
            byte[] outData;

            using (var ms = new MemoryStream())
            {
                formatter.Serialize(ms, circleToSerialize);
                outData = ms.ToArray();
            }

            return outData;
        }

        private void bt_Disconnect_Click(object sender, EventArgs e)
        {
            senderSocket.Send(Encoding.ASCII.GetBytes("Shutdown"));
            senderSocket.Shutdown(SocketShutdown.Both);
            senderSocket.Close();
            bt_Connect.Enabled = true;
            bt_Disconnect.Enabled = false;
        }
    }
}
