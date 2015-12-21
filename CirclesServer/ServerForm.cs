using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using CirclesClient;

namespace CirclesServer
{

    public partial class ServerForm : Form
    {
        List<Circle> CircleList = new List<Circle>();

        SocketPermission permission;
        Socket sListener;
        IPEndPoint ipEndPoint;
        Socket handler;
        byte[] dataBuffer = new Byte[1024];

        public static ManualResetEvent allDone = new ManualResetEvent(false);

        float averageRad = 0;

        public ServerForm()
        {
            InitializeComponent();
        }

        private void StartServer_Click(object sender, EventArgs e)
        {
            permission = new SocketPermission(NetworkAccess.Accept, TransportType.Tcp, "", SocketPermission.AllPorts);

            sListener = null;

            // Ensures the code to have permission to access a Socket 
            permission.Demand();

            // Resolves a host name to an IPHostEntry instance 
            IPHostEntry ipHost = Dns.GetHostEntry("");

            // Gets first IP address associated with a localhost 
            IPAddress ipAddr = ipHost.AddressList[0];

            // Creates a network endpoint 
            ipEndPoint = new IPEndPoint(ipAddr, 11000);

            sListener = new Socket(ipAddr.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            try
            {
                sListener.Bind(ipEndPoint);
                sListener.Listen(100);

                // Start listening for connections
                MessageBox.Show("Server is waiting for connection");

                // Set the event to nonsignaled state.
                allDone.Reset();

                // Start an asynchronous socket to listen for connections.
                Console.WriteLine("Waiting for a connection...");
                sListener.BeginAccept(
                    new AsyncCallback(AcceptCallback), sListener);

                bt_StartServer.Enabled = false;
                bt_Load.Enabled = false;
                bt_Save.Enabled = true;

                // Wait until a connection is made before continuing.
                allDone.WaitOne();


            }
            catch (Exception exc) { MessageBox.Show(exc.ToString()); }


            //StartServer.Enabled = false;
        }

        public void AcceptCallback(IAsyncResult ar)
        {

            // Signal the main thread to continue.
            allDone.Set();

            // Get the socket that handles the client request.
            Socket listener = (Socket)ar.AsyncState;
            Socket handler = listener.EndAccept(ar);

            bool hasLoaded = CircleList.Count > 0;

            Send(handler, hasLoaded);

            if(hasLoaded)
            {
                byte[] circleAmountData = BitConverter.GetBytes(CircleList.Count);
                handler.Send(circleAmountData);

                foreach(Circle circleToSend in CircleList)
                {
                    handler.Send(SerializeCircle(circleToSend));                   
                }
            }

            // Create the circle object.
            CircleObject recCircle = new CircleObject();
            recCircle.workSocket = handler;
            handler.BeginReceive(recCircle.buffer, 0, recCircle.BufferSize, 0, new AsyncCallback(ReadCallback), recCircle);
        }

        public void ReadCallback(IAsyncResult ar)
        {

            // Retrieve the circle object and the handler socket
            // from the asynchronous circle object.
            CircleObject recCircle = (CircleObject)ar.AsyncState;
            Socket handler = recCircle.workSocket;

            // Read data from the client socket. 
            int bytesRead = handler.EndReceive(ar);

            if (bytesRead >= recCircle.BufferSize)
            {

                Circle receivedCircle;
                BinaryFormatter formatter = new BinaryFormatter();
                using (var ms = new MemoryStream(recCircle.buffer))
                {
                    receivedCircle = (Circle)formatter.Deserialize(ms);
                }

                if (receivedCircle.Index >= CircleList.Count)
                {
                    if (!IsCircleColliding(receivedCircle))
                    {
                        CircleList.Add(receivedCircle);

                        SetAmountText(CircleList.Count.ToString());

                        // Validate the client action
                        Send(handler, true);
                    }
                    else
                    {
                        // Block the client action
                        Send(handler, false);
                    }

                }
                else
                {
                    if (receivedCircle.Radius < 0)
                    {
                        CircleList.RemoveAt(receivedCircle.Index);

                        float totalRad = 0.0f;

                        // Reset the index variable of the remaining circles                   
                        for (int i = 0; i < CircleList.Count; i++)
                        {
                            CircleList[i].Index = i;
                            totalRad += CircleList[i].Radius;
                        }

                        averageRad = totalRad / CircleList.Count;

                        SetAverageRadText(averageRad.ToString());

                        SetAmountText(CircleList.Count.ToString());

                        // Validate the client action
                        Send(handler, true);
                    }
                    else
                    {
                        if (!IsCircleColliding(receivedCircle))
                        {
                            CircleList[receivedCircle.Index] = receivedCircle;
                            float totalRad = 0.0f;
                            foreach (Circle c in CircleList)
                            {
                                totalRad += c.Radius;
                            }

                            averageRad = totalRad / CircleList.Count;

                            SetAverageRadText(averageRad.ToString());

                            // Validate the client action
                            Send(handler, true);
                        }
                        else
                        {
                            // Block the client action
                            Send(handler, false);
                        }
                    }
                }

                // Continue to receiving to catch the next sending cirlce
                handler.BeginReceive(recCircle.buffer, 0, recCircle.BufferSize, 0,
                new AsyncCallback(ReadCallback), recCircle);

            }
            else
            {
                // Not all data received. Get more.
                handler.BeginReceive(recCircle.buffer, 0, recCircle.BufferSize, 0,
                new AsyncCallback(ReadCallback), recCircle);
            }

        }

        delegate void SetAmountTextCallback(string text);

        // Thread safe set the text of the amount of circles
        private void SetAmountText(string text)
        {
            if(this.tB_CircleAmount.InvokeRequired)
            {
                SetAmountTextCallback d = new SetAmountTextCallback(SetAmountText);
                this.Invoke(d, new object[] { text });

            }
            else
            {
                this.tB_CircleAmount.Text = text;
            }
        }

        delegate void SetAverageRadTextCallback(string text);

        // Thread safe set the text of the average radius
        private void SetAverageRadText(string text)
        {
            if (tB_averageRad.InvokeRequired)
            {
                SetAverageRadTextCallback d = new SetAverageRadTextCallback(SetAverageRadText);
                Invoke(d, new object[] { text });
            }
            else
            {
                tB_averageRad.Text = text;
            }
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

        private static void Send(Socket handler, bool data)
        {
            // Convert the bool to a byte array.
            byte[] byteData = BitConverter.GetBytes(data);

            // Begin sending the data to the remote device.
            handler.Send(byteData);
        }

        // Checks if the given circle is colliding with any other circle, returns true if there is a collision
        private bool IsCircleColliding(Circle circleToCheck)
        {
            bool outBool = false;

            foreach (Circle testCircle in CircleList)
            {
                if (testCircle.Index != circleToCheck.Index)
                {
                    double Distance = testCircle.DistanceTo(circleToCheck);
                    if (Distance < testCircle.Radius + circleToCheck.Radius)
                    {
                        outBool = true;
                        return outBool;
                    }
                }
            }
            return outBool;
        }

        private void fD_loadConfig_FileOk(object sender, CancelEventArgs e)
        {
            using (Stream stream = fD_loadConfig.OpenFile())
            {
                var bformatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();

                CircleList = (List<Circle>)bformatter.Deserialize(stream);

                stream.Close();

                float totalRad = 0.0f;
                foreach (Circle c in CircleList)
                {
                    totalRad += c.Radius;
                }

                averageRad = totalRad / CircleList.Count;

                SetAverageRadText(averageRad.ToString());
                SetAmountText(CircleList.Count.ToString());
            }
        }

        private void bt_Save_Click(object sender, EventArgs e)
        {
            fD_saveConfig.ShowDialog();
        }

        private void bt_Load_Click(object sender, EventArgs e)
        {
            fD_loadConfig.ShowDialog();
        }

        private void fD_saveConfig_FileOk(object sender, CancelEventArgs e)
        {
            using (Stream stream = fD_saveConfig.OpenFile())
            {
                var bformatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();

                bformatter.Serialize(stream, CircleList);

                stream.Close();
            }
        }
    }

    // Circle object for reading client data asynchronously
    public class CircleObject
    {
        // Client  socket.
        public Socket workSocket = null;
        // Size of receive buffer.
        public int BufferSize = 1024;
        // Receive buffer.
        public byte[] buffer = new byte[1024];

        public CircleObject()
        {
            BinaryFormatter testformatter = new BinaryFormatter();
            MemoryStream HoldStream = new MemoryStream();

            testformatter.Serialize(HoldStream, new Circle());

            BufferSize = HoldStream.ToArray().Length;
            buffer = new byte[BufferSize];
        }
    }
}
