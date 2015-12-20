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

        public ServerForm()
        {
            InitializeComponent();
        }

        private void StartServer_Click(object sender, EventArgs e)
        {
            permission = new SocketPermission(NetworkAccess.Accept, TransportType.Tcp, "", SocketPermission.AllPorts);

            sListener = null;

            CircleObject TestCircleObject = new CircleObject();
            MessageBox.Show("Circle Byte Size = " + TestCircleObject.BufferSize);

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

            // Create the state object.
            CircleObject recCircle = new CircleObject();
            recCircle.workSocket = handler;
            handler.BeginReceive(recCircle.buffer, 0, recCircle.BufferSize, 0, new AsyncCallback(ReadCallback), recCircle);
        }

        public void ReadCallback(IAsyncResult ar)
        {

            // Retrieve the state object and the handler socket
            // from the asynchronous state object.
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
                        Send(handler, true);
                    }
                    else
                    {
                        Send(handler, false);
                    }

                }
                else
                {
                    if (receivedCircle.Radius < 0)
                    {
                        CircleList.RemoveAt(receivedCircle.Index);
                        Send(handler, true);
                    }
                    else
                    {
                        if (!IsCircleColliding(receivedCircle))
                        {
                            CircleList[receivedCircle.Index] = receivedCircle;
                            Send(handler, true);
                        }
                        else
                        {
                            Send(handler, false);
                        }
                    }
                }

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

        private static void Send(Socket handler, bool data)
        {
            // Convert the string data to byte data using ASCII encoding.
            byte[] byteData = BitConverter.GetBytes(data);

            // Begin sending the data to the remote device.
            handler.Send(byteData);
        }

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
                    }
                }
            }
            return outBool;
        }

    }

    // State object for reading client data asynchronously
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
