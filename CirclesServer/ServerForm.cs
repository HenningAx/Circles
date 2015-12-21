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
        // List of all circles
        List<Circle> CircleList = new List<Circle>();


        // Socket connection 

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

        // Creates the server socket and start listening for a connection
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
                sListener.BeginAccept(new AsyncCallback(AcceptCallback), sListener);

                bt_StartServer.Enabled = false;
                bt_Load.Enabled = false;
                bt_Save.Enabled = true;

                // Wait until a connection is made before continuing.
                allDone.WaitOne();


            }
            catch (Exception exc) { MessageBox.Show(exc.ToString()); }
        }

        public void AcceptCallback(IAsyncResult ar)
        {

            // Signal the main thread to continue.
            allDone.Set();

            // Get the socket that handles the client request.
            Socket listener = (Socket)ar.AsyncState;
            Socket handler = listener.EndAccept(ar);

            // Tell the client if the server has loaded a configuration
            bool hasLoaded = CircleList.Count > 0;
            Send(handler, hasLoaded);

            EnableLoadButton(false);
            EnableSaveButton(true);

            if(hasLoaded)
            {
                // If the server has loaded a configuration send the client the amount of circles he has to receive
                byte[] circleAmountData = BitConverter.GetBytes(CircleList.Count);
                handler.Send(circleAmountData);

                // Send all circles in the current configuration
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

            // If the server received a full circle
            if (bytesRead >= recCircle.BufferSize)
            {
                // Deserialize the received bytes to a circle
                Circle receivedCircle;
                BinaryFormatter formatter = new BinaryFormatter();
                using (var ms = new MemoryStream(recCircle.buffer))
                {
                    receivedCircle = (Circle)formatter.Deserialize(ms);
                }

                // If the index of the received circle if higher then amount of circles
                if (receivedCircle.Index >= CircleList.Count)
                {
                    // Check if the new circle is colliding with any other circle and if not add it to the circle list
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
                    // If the radius of the received circle is negative interpret this as a delte request 
                    if (receivedCircle.Radius < 0)
                    {
                        // Delete the received circle from the list
                        CircleList.RemoveAt(receivedCircle.Index);

                        float totalRad = 0.0f;

                        // Reset the index variable of the remaining circles and recalculate the average radius                 
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
                        // This is a move request, check if the circle is colliding with any other circle
                        if (!IsCircleColliding(receivedCircle))
                        {
                            // Override the circle in the circle list at the index of the received circle
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
            // If the server received less bytes then a circle has but at least some check if this is a shutdown request
            if(bytesRead > 0)
            {
                // Check if the received data is a shutdown request by checking if the byte array contains the string "Shutdown"
                string Command = Encoding.ASCII.GetString(recCircle.buffer);
                if(Command.Contains("Shutdown"))
                {
                    // Reset the socket to wait for a connection again

                    // Start listening for connections
                    MessageBox.Show("Server is waiting for connection");

                    // Set the event to nonsignaled state.
                    allDone.Reset();

                    // Start an asynchronous socket to listen for connections.
                    Console.WriteLine("Waiting for a connection...");
                    sListener.BeginAccept(
                        new AsyncCallback(AcceptCallback), sListener);

                    bt_StartServer.Enabled = false;
                    EnableLoadButton(true);
                    EnableSaveButton(true);
                } 
                else
                {
                    // Not all data received. Get more.
                    handler.BeginReceive(recCircle.buffer, 0, recCircle.BufferSize, 0,
                    new AsyncCallback(ReadCallback), recCircle);
                }             
            }

            // Not all data received. Get more.
            handler.BeginReceive(recCircle.buffer, 0, recCircle.BufferSize, 0,
            new AsyncCallback(ReadCallback), recCircle);

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

        delegate void EnableLoadButtonCallback(bool enabled);

        // Thread safe enable/disable the load button
        private void EnableLoadButton(bool enabled)
        {
            if(bt_Load.InvokeRequired)
            {
                EnableLoadButtonCallback d = new EnableLoadButtonCallback(EnableLoadButton);
                Invoke(d, new object[] { enabled });
            }
            else
            {
                bt_Load.Enabled = enabled;
            }
        }

        delegate void EnableSaveButtonCallback(bool enabled);

        // Thread safe enable/disable the save button
        private void EnableSaveButton(bool enabled)
        {
            if (bt_Save.InvokeRequired)
            {
                EnableSaveButtonCallback d = new EnableSaveButtonCallback(EnableSaveButton);
                Invoke(d, new object[] { enabled });
            }
            else
            {
                bt_Save.Enabled = enabled;
            }
        }

        // Serialize a circle to send it through a circle 
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

        // Send a boolean through the given socket
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

        // Tries to load the circle configuration file
        private void fD_loadConfig_FileOk(object sender, CancelEventArgs e)
        {
            using (Stream stream = fD_loadConfig.OpenFile())
            {
                var bformatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();

                // Tries to deserialize the loaded file to a List of Circles
                try
                {
                    CircleList = (List<Circle>)bformatter.Deserialize(stream);
                }
                catch (Exception exc) { MessageBox.Show("Failed to load file"); }

                stream.Close();

                // Calculate and the average radius and set the text in the text box
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

        // Save the current circle configuration to a binary file
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

        // Serialize a circle to get the size of a circle object in bytes
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
