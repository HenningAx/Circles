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
        // Minimum size of a circle
        private int MinCircleSize = 5;

        // Size of a circle object in bytes
        private int CircleObjectSize = 441;

        // List of all circles currently on the screen
        private List<Circle> CircleList = new List<Circle>();

        // The current drawing mode
        private DrawingMode DrawMode = DrawingMode.draw;

        // Last where the user performed a mouse down event
        Point LastClickPoint = new Point();

        // Original center of a circle when moving it
        Point OriginalCenter = new Point();

        //True if a circle is being drawn
        bool bIsDrawingCircle = false;

        //True if a circle is being moved
        bool bIsMovingCircle = false;

        //Index of the currently selected circle
        private int SelectedCircleIndex = -1;

        // Index of the circle that is currently drawn by the user
        int CurrentDrawCircleIndex = 0;

        // Are we drawing a ghost circle because the current circle is colliding
        bool bDrawGhostCircle = false;

        // Circle to draw if the action is blocked by the server to show the user input
        Circle GhostCircle = new Circle();

        // Socket connection

        IPHostEntry ipHostInfo;
        IPAddress ipAddress;
        IPEndPoint remoteEP;
        Socket senderSocket;

        // True if the client is connected to the server
        bool bIsConnected = false;

        public ClientForm()
        {
            InitializeComponent();

            // Set the panel to use double buffer to stop flickering
            typeof(Panel).InvokeMember("DoubleBuffered", BindingFlags.SetProperty | BindingFlags.Instance | BindingFlags.NonPublic, null, pn_DrawingPanel, new object[] { true });

            SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
        }

        // Set the drawing mode to move if the user presses the move button
        private void bt_Move_Click(object sender, EventArgs e)
        {
            DrawMode = DrawingMode.move;
            lb_ModeDisplay.Text = "Move Mode";
        }

        // Set the drawing mode to delete if the user presses the delete button
        private void bt_Delete_Click(object sender, EventArgs e)
        {
            DrawMode = DrawingMode.delete;
            lb_ModeDisplay.Text = "Delete Mode";
        }

        // Set the drawing mode to draw if the user presses the draw button
        private void bt_Draw_Click(object sender, EventArgs e)
        {
            DrawMode = DrawingMode.draw;
            lb_ModeDisplay.Text = "Draw Mode";
        }

        // Returns the bounding rectangle of the given center + radius
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
            // Draw each circle in the circle list
            foreach (Circle CircleToDraw in CircleList)
            {
                Rectangle CircleBounds = ComputeCircleBounds(CircleToDraw.Center, CircleToDraw.Radius);

                Pen CirclePen = new Pen(Color.Black);
                SolidBrush CircleBrush = new SolidBrush(CircleToDraw.GetCircleColor());

                e.Graphics.FillEllipse(CircleBrush, CircleBounds);
                e.Graphics.DrawEllipse(CirclePen, CircleBounds);
            }

            // Draw the ghost cirlce if it should be drawn
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
                // If we are in drawing mode and no circle is currently drawn by the user, a new circle is created at the click position if validated by the server
                if (DrawMode == DrawingMode.draw && !bIsDrawingCircle)
                {
                    LastClickPoint = e.Location;
                    Circle NewCircle = new Circle(MinCircleSize, LastClickPoint, pn_ColorSelectPanel.BackColor);
                    NewCircle.Index = CircleList.Count;

                    // Send a request to the server, only draw the circle if the server validates the request
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
                    // Try to select a circle at the current mouse position
                    SelectedCircleIndex = SelectCircle(e.Location);

                    if (SelectedCircleIndex > -1)
                    {
                        OriginalCenter = CircleList[SelectedCircleIndex].Center;
                    }
                    LastClickPoint = e.Location;
                }
                else if (DrawMode == DrawingMode.delete)
                {
                    // Try to select a circle at the current mouse position
                    SelectedCircleIndex = SelectCircle(e.Location);

                    // If a circle is succesfully selected send a request to the server to delete it
                    if (SelectedCircleIndex > -1)
                    {
                        int CacheRadius = CircleList[SelectedCircleIndex].Radius;
                        CircleList[SelectedCircleIndex].Radius = -1;

                        // Send a request to the server, by setting the radius of the sent circle to a negative value the server understands that this is a delete request
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
            // Check if the user is currently drawing a circle 
            if (DrawMode == DrawingMode.draw && bIsDrawingCircle)
            {
                // Calculate the new radius based on the distance the mouse moved from the starting position
                int CacheRadius = CircleList[CurrentDrawCircleIndex].Radius;
                double MouseMoveDistance = 0;
                MouseMoveDistance = Distance(e.Location, LastClickPoint);

                if (MouseMoveDistance >= MinCircleSize)
                {
                    CircleList[CurrentDrawCircleIndex].Radius = (int)MouseMoveDistance;

                    // Send a request to the server if the new circle radius is valid
                    if (!SendCircleRequest(CircleList[CurrentDrawCircleIndex]))
                    {
                        // If the new circle radius is not valid draw a ghost circle to show the user the incorrect input
                        GhostCircle.Center = CircleList[CurrentDrawCircleIndex].Center;
                        GhostCircle.Radius = CircleList[CurrentDrawCircleIndex].Radius;

                        // Reset the circle radius because the request was rejected
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
            }
            else if (DrawMode == DrawingMode.move && SelectedCircleIndex > -1)
            {
                // Calculate the distance the mouse moved from the click point
                Point MouseMovePoint = new Point();
                MouseMovePoint.X = e.X - LastClickPoint.X;
                MouseMovePoint.Y = e.Y - LastClickPoint.Y;

                // Sets the new center of the circle by adding the mouse move to its original center
                Point NewCenter = new Point();
                NewCenter.X = OriginalCenter.X + MouseMovePoint.X;
                NewCenter.Y = OriginalCenter.Y + MouseMovePoint.Y;

                // Cache the center in case the request will get rejected
                Point CacheCenter = CircleList[SelectedCircleIndex].Center;
                CircleList[SelectedCircleIndex].Center = NewCenter;

                // Send a request to the server to validate the new circle position
                if (!SendCircleRequest(CircleList[SelectedCircleIndex]))
                {
                    // If the request gets rejected draw a ghost circle to show the incorrect input to the user
                    GhostCircle.Center = CircleList[SelectedCircleIndex].Center;
                    GhostCircle.Radius = CircleList[SelectedCircleIndex].Radius;

                    bDrawGhostCircle = true;

                    // Reset the position
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
            // Stop the user drawing or the user over and stop drawing a ghost circle 
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

        // Returns the distance between to given 2D points
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

            // Serialize a circle and save the size of the byte array to get the size of one circle object
            CircleObjectSize = SerializeCircle(new Circle()).Length;
        }

        // Connect to the server
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

                // Receive a boolean from the server which tells the client if he should load a configuration from the server
                byte[] bufferBytes = new byte[1];
                int bytesRec = senderSocket.Receive(bufferBytes);
                bool recBool = BitConverter.ToBoolean(bufferBytes, 0);

                // Receive the circle configuration from the server if the received boolean is true
                if (recBool)
                {
                    // Receive an integer from the server which tells the client how many circle are in the configuration to receive
                    byte[] amountBuffer = new byte[sizeof(int)];
                    int amountBytesRec = senderSocket.Receive(amountBuffer);
                    CircleList.Clear();

                    int CirclesToRec = BitConverter.ToInt32(amountBuffer, 0);
                    MessageBox.Show(CirclesToRec.ToString() + " circles loaded");

                    // Receive all circles in the configuration from the server
                    for (int i = 0; i < CirclesToRec; i++)
                    {
                        byte[] circleBuffer = new byte[CircleObjectSize];
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
                MessageBox.Show("Failed to connect to the server, please make sure the server is running");
            }

        }

        // Send a circle to the server and receive a boolean which tells if the circle is valid
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

        // Serialize a circle to send it through a socket
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

        // Disconnect from the server, the user will not be able to perform actions before he connected to the server again
        private void bt_Disconnect_Click(object sender, EventArgs e)
        {
            bIsConnected = false;
            senderSocket.Send(Encoding.ASCII.GetBytes("Shutdown"));
            senderSocket.Shutdown(SocketShutdown.Both);
            senderSocket.Close();
            bt_Connect.Enabled = true;
            bt_Disconnect.Enabled = false;
            bIsConnected = false;
            bt_Delete.Enabled = false;
            bt_Draw.Enabled = false;
            bt_Move.Enabled = false;
        }
    }
}
