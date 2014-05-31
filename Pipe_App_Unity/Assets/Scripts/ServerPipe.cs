// <copyright file="ServerPipe.cs" company="dyadica.co.uk">
// Copyright (c) 2010, 2014 All Right Reserved, http://www.dyadica.co.uk

// This source is subject to the dyadica.co.uk Permissive License.
// Please see the http://www.dyadica.co.uk/permissive-license file 
// for more information. All other rights reserved.

// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY
// KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
// PARTICULAR PURPOSE.
// </copyright>

// <author>SJB</author>
// <email>SJB@dyadica.co.uk</email>
// <date>26.05.2014</date>
// <summary>A simple class containing code that can be used to
// communicate with the Unity (input) via pipes.</summary>

using System.IO;
using System.IO.Pipes;

using System.Threading;

[System.Serializable]
public class ServerPipe
{
    #region Properties

    // A static reference to this script just in case.

    public static ServerPipe Instance;

    //
    //

    public delegate void ParseServerPipeDataHandler(string data);
    public static event ParseServerPipeDataHandler ParseServerPipeDataEvent;

    // An Event called when the server disconnects whilst
    // the client is still running.

    public delegate void ClientClosedEventHandler();
    public static event ClientClosedEventHandler ClientClosedEvent;

    // Flag used to determine if the pipe is running.

    public bool IsRunning = false;

    // Static property used to store any incoming data. 
    // As it is static it can be accessed accross thread.

    public static string RawData;

    // Property detailing the name of the pipe.

    public string ServerName = "UnityOutPipe";

    // Thread used to listen for any incoming messages.

    private Thread ServerThread;

    #endregion Properties

    /// <summary>
    /// Initialise the Server class
    /// </summary>
    public ServerPipe()
    {
        // Create a static reference just in case.
        // Not used yet!

        Instance = this;

        // Make a call to stop the server just in
        // case an instance is already running.
        // This also acts as a generic clean up.

        StopServer();

        // Register the events. First up is the
        // event that is used to update the data
        // upon input.

        ParseServerPipeDataEvent +=
            ServerPipe_ParseServerPipeDataEvent;

        // The next event is used to inform of a
        // closure of the attached client whilst
        // the server is still running.

        ClientClosedEvent +=
            ServerPipe_ClientClosedEvent;
    }

    /// <summary>
    /// Initialise and start the server.
    /// </summary>
    public void StartServer()
    {
        // Flag that the server is now running.

        IsRunning = true;

        // Initialise the thread loop used to
        // listen for incoming data.

        ServerThread = new Thread(ServerThreadLoop);
        ServerThread.Start();
    }

    /// <summary>
    /// Shut down and perform cleanup of 
    /// an existing server instance.
    /// </summary>
    public void StopServer()
    {
        // Flag that the server is no longer
        // running and if it is then let this
        // be the last loop.

        if (IsRunning != false)
            IsRunning = false;

        // Alittle delay to let things catch up.

        Thread.Sleep(100);

        // If the server thread is still alive
        // then hard kill it!

        if (ServerThread != null)
            ServerThread.Abort();
    }

    /// <summary>
    /// Thread used to listen for incoming 
    /// data.
    /// </summary>
    void ServerThreadLoop()
    {
        try
        {
            using (NamedPipeServerStream pipeStream = new NamedPipeServerStream(ServerName))
            {
                pipeStream.WaitForConnection();

                using (StreamReader sr = new StreamReader(pipeStream))
                {
                    while ((RawData = sr.ReadLine()) != null)
                    {
                        if (IsRunning == false)
                        {
                            pipeStream.Disconnect();
                            pipeStream.Dispose();
                            break;
                        }

                        if (RawData != string.Empty && ParseServerPipeDataEvent != null)
                            ParseServerPipeDataEvent(RawData);
                    }
                }
            }
        }
        catch (IOException)
        {
            // Error with the server loop IO
        }
        catch (System.ObjectDisposedException)
        {
            // Disposed object exception
        }

        // Remove reference to the server

        ServerThread = null;
    }

    #region Custom Events

    /// <summary>
    /// Event called when the server disconnects whilst
    /// the client is still running.
    /// </summary>
    void ServerPipe_ClientClosedEvent()
    {
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="data"></param>
    void ServerPipe_ParseServerPipeDataEvent(string data)
    {
        // Check if the client is closed and if
        // it is then throw a client closed event.

        if (data == "Closing Client")
        {
            if (ClientClosedEvent != null)
                ClientClosedEvent();
        }
    }

    #endregion Custom Events
}
