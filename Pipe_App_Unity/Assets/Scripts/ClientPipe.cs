// <copyright file="ClientPipe.cs" company="dyadica.co.uk">
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
// communicate with the Unity (output) via pipes.</summary>

using System.IO;
using System.IO.Pipes;

using System.Threading;

[System.Serializable]
public class ClientPipe
{
    #region Properties

    // A static reference to this script just in case.

    public static ClientPipe Instance;

    //

    public delegate void PipeBrokenEventHandler();
    public static event PipeBrokenEventHandler PipeBrokenEvent;

    // Flag used to determine if this end of the pipe
    // is running.

    public bool IsRunning = false;

    // Static property used to store any outgoing data. 
    // As it is static it can be accessed accross thread.

    public static string SendData = string.Empty;

    // Property detailing the name of the pipe.

    private Thread ClientThread;

    // Thread used to send any outgoing messages.

    public string ClientPipeName = "UnityInPipe";

    // Flag used to determine if we show a debug
    // count in addition to the string data.

    public bool DebugCount = false;

    #endregion Properties

    /// <summary>
    /// Initalise the Client class
    /// </summary>
    public ClientPipe()
    {
        // Create a static reference just in case.
        // Not used yet!

        Instance = this;

        // Make a call to stop the client just in
        // case an instance is already running.
        // This also acts as a generic clean up.

        StopClient();

        // Register the events.

        PipeBrokenEvent +=
            ClientPipe_PipeBrokenEvent;
    }

    /// <summary>
    /// Initialise and Start the Client
    /// </summary>
    public void StartClient()
    {
        IsRunning = true;

        Thread.Sleep(100);

        ClientThread = new Thread(ClientThreadLoop);
        ClientThread.Start();

    }

    /// <summary>
    /// Shut down and perform cleanup of 
    /// an existing client instance.
    /// </summary>
    public void StopClient()
    {
        if (IsRunning != false)
            IsRunning = false;

        Thread.Sleep(100);

        if (ClientThread != null)
            ClientThread.Abort();
    }

    /// <summary>
    /// Thread used to send for outgoing 
    /// data. 
    /// </summary>
    private void ClientThreadLoop()
    {
        using (NamedPipeClientStream pipeStream = new NamedPipeClientStream(ClientPipeName))
        {
            // The connect function will indefinately wait for the pipe to become available
            // If that is not acceptable specify a maximum waiting time (in ms)

            pipeStream.Connect();

            // Reset the count to zero.

            int i = 0;

            try
            {
                using (StreamWriter sw = new StreamWriter(pipeStream))
                {
                    sw.AutoFlush = true;

                    while (IsRunning)
                    {
                        // If the DebugCount flag is set to true then
                        // show the count. Otherwise just show the data.

                        if (DebugCount)
                        { sw.WriteLine(SendData + " : " + i.ToString()); }
                        else
                        { sw.WriteLine(SendData); }

                        // inc the count

                        i++;
                    }

                    // Send a closing message to the server.

                    sw.WriteLine("Closing Client");

                    // Attempt a little clean up.

                    try
                    {
                        pipeStream.Close();
                        pipeStream.Dispose();
                    }
                    catch
                    {
                        // Failed to perform clean up
                    }
                }
            }
            catch (IOException)
            {
                // Pipe is broken

                if (PipeBrokenEvent != null)
                    PipeBrokenEvent();
            }
            catch (System.ObjectDisposedException)
            {
                // We have killed the pipe!
            }
        }
    }

    /// <summary>
    /// Event called when the server disconnects whilst
    /// the client is still running.
    /// </summary>
    void ClientPipe_PipeBrokenEvent()
    {
    }
}