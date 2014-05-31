using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace Pipe_App_WF
{
    public partial class Form1 : Form
    {
        public ClientPipe ClientPipe;
        public ServerPipe ServerPipe;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            // Define and set the client (out) pipe.

            ClientPipe = new ClientPipe();
            ClientPipe.ClientPipeName = "UnityInPipe";

            // Define and set the server (in) pipe.

            ServerPipe = new ServerPipe();
            ServerPipe.ServerName = "UnityOutPipe";

            // Register the required events for the client
            // based script calls.

            ClientPipe.PipeBrokenEvent +=
                ClientPipe_PipeBrokenEvent;

            // Register the required events for the server
            // based script calls.

            ServerPipe.ClientClosedEvent +=
                ServerPipe_ClientClosedEvent;

            ServerPipe.ParseServerPipeDataEvent +=
                ServerPipe_ParseServerPipeDataEvent;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            switch (button1.Text)
            {
                case "Open":
                    ClientPipe.StartClient();
                    button1.Text = "Close";
                    break;
                case "Close":
                    ClientPipe.StopClient();
                    button1.Text = "Open";
                    break;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            switch (button2.Text)
            {
                case "Open":
                    button2.Text = "Close";
                    ServerPipe.StartServer();
                    break;
                case "Close":
                    button2.Text = "Open";
                    ServerPipe.StopServer();
                    break;
            }
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            try
            { ClientPipe.StopClient(); }
            catch
            {
                // Failed to Close out pipe
            }

            try
            { ServerPipe.StopServer(); }
            catch
            {
                // Failed to Close in pipe
            }
        }

        /// <summary>
        /// Event called when the server disconnects whilst
        /// the client is still running. In this example if
        /// the unity server close button is called or the 
        /// app is shut down.
        /// </summary>
        void ClientPipe_PipeBrokenEvent()
        {
            Console.WriteLine("Pipe Broken!");

            button1.Invoke(new Action(() => button1.Text = "Open"));

            textBox2.Invoke(new Action(() => textBox2.Text = "Disconnected!"));

            ClientPipe.StopClient();
        }

        /// <summary>
        /// Event called when the server recieves data from 
        /// the client and the parse function is called.
        /// </summary>
        /// <param name="data"></param>
        void ServerPipe_ParseServerPipeDataEvent(string data)
        {
            textBox2.Invoke(new Action(() => textBox2.Text = data));
        }

        /// <summary>
        /// Event called when the server recieves a notification
        /// that the client has either been shutdown or Closeped.
        /// In this example that is if the Unity app close out
        /// pipe button has been pressed or the app has been shut.
        /// </summary>
        void ServerPipe_ClientClosedEvent()
        {
            Console.WriteLine("Killing server on client drop!");

            textBox2.Invoke(new Action(() => textBox2.Text = "Closing server due to client drop"));
            button2.Invoke(new Action(() => button2.Text = "Open"));

            if (ServerPipe != null)
            {
                ServerPipe.IsRunning = false;
                ServerPipe.StopServer();
            }

            ServerPipe = null;
            
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            if (ClientPipe != null && ClientPipe.IsRunning)
            {
                ClientPipe.SendData = textBox1.Text;
            }
        } 
    }
}
