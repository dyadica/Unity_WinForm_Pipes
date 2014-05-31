// <copyright file="UnityPipeIn.cs" company="dyadica.co.uk">
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

using UnityEngine;
using System.Collections;

public class UnityPipeIn : MonoBehaviour
{
    #region Properties

    public ServerPipe ServerPipe;

    public GUIText RawDataGUI;

    public static string InData = string.Empty;

    public bool AutoStartServer = false;

    public Rect ControlBoxIn = 
        new Rect(10, 10, 200, 100);

    string status;

    #endregion Properties

    #region Unity Events

    /// <summary>
    /// Generic unity start event.
    /// </summary>
    void Start () 
    {
        if (ServerPipe == null)
        {
            print("Creating new Server Pipe");
            ServerPipe = new ServerPipe();
        }

        ServerPipe.ParseServerPipeDataEvent +=
            ServerPipe_ParseServerPipeDataEvent;

        ServerPipe.ClientClosedEvent +=
            ServerPipe_ClientClosedEvent;

        if (AutoStartServer)
            ServerPipe.StartServer();
	}
	
    /// <summary>
    /// Generic unity update loop.
    /// </summary>
	void Update () 
    {
        if (ServerPipe.IsRunning == false)
        { status = "Closed"; }
        else if (InData == string.Empty)
        { status = "Ready"; }
        else if (InData != string.Empty)
        { status = "Active"; }
    }

    /// <summary>
    /// Render the pipes control box 
    /// to the screen.
    /// </summary>
    void OnGUI()
    {
        #region Pipe Controls

        GUILayout.BeginArea(ControlBoxIn, GUI.skin.box);

        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        GUILayout.BeginVertical(GUILayout.Width(150));

        GUILayout.Label("In Pipe");

        string buttonText = string.Empty;

        if(ServerPipe != null && ServerPipe.IsRunning)
        { buttonText = "Close"; } 
        else 
        { buttonText = "Open"; }

        if (GUILayout.Button(buttonText))
        {
            switch (buttonText)
            {
                case "Open": ServerPipe.StartServer(); 
                    break;
                case "Close": ServerPipe.StopServer(); 
                    break;
            }            
        }

        if (InData != null)
            GUILayout.TextField(InData);

        GUILayout.Label("Status: " + status);

        GUILayout.EndVertical();
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();

        GUILayout.EndArea();

        #endregion Pipe Controls

        if (RawDataGUI != null)
            RawDataGUI.text = InData;
    }

    /// <summary>
    /// A little additional extra 
    /// clean up just in case its 
    /// needed.
    /// </summary>
    void OnDestroy()
    {
        try
        {
            ServerPipe.StopServer();
            ServerPipe = null;
        }
        catch
        {
            print("Failed to stop server on destroy!");
        }
    }

    #endregion Unity Events

    #region Custom Events

    /// <summary>
    /// Event called when the server disconnects whilst
    /// the client is still running. In this example if
    /// the winform close button is called or the form
    /// is shut down.
    /// </summary>
    void ServerPipe_ClientClosedEvent()
    {
        print("Killing server on client drop!");

        if (ServerPipe != null)
        {
            ServerPipe.IsRunning = false;
            ServerPipe.StopServer();           
        }

        ServerPipe = null; 
    }

    void ServerPipe_ParseServerPipeDataEvent(string data)
    {
        InData = data;
    }

    #endregion Custom Events
}
