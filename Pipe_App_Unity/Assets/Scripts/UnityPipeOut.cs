// <copyright file="UnityPipeOut.cs" company="dyadica.co.uk">
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

using UnityEngine;
using System.Collections;

public class UnityPipeOut : MonoBehaviour
{
    #region Properties

    public ClientPipe ClientPipe;

    public bool AutoStartClient = false;

    public static string OutData;

    public Rect ControlBoxOut =
        new Rect(220, 10, 200, 100);

    string status;

    #endregion Properties

    #region Unity Events

    /// <summary>
    /// Generic unity start event
    /// </summary>
    void Start () 
    {
        if (ClientPipe == null)
        {
            print("Creating new Client Pipe");
            ClientPipe = new ClientPipe(); 
        }

        // Register the required events

        ClientPipe.PipeBrokenEvent +=
            ClientPipe_PipeBrokenEvent;

        if (AutoStartClient)
            ClientPipe.StartClient();
	}
	
    /// <summary>
    /// Generic unity update loop
    /// </summary>
	void Update () 
    {
        if (ClientPipe.IsRunning == false)
        { status = "Closed"; }
        else if (OutData == string.Empty)
        { status = "Ready"; }
        else if (OutData != string.Empty)
        { status = "Active"; }
	}

    /// <summary>
    /// Render the pipes control box 
    /// to the screen.
    /// </summary>
    void OnGUI()
    {
        #region Pipe Controls

        GUILayout.BeginArea(ControlBoxOut, GUI.skin.box);

        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        GUILayout.BeginVertical(GUILayout.Width(150));

        GUILayout.Label("Out Pipe");

        string buttonText = string.Empty;

        if (ClientPipe != null && ClientPipe.IsRunning)
        { buttonText = "Close"; }
        else
        { buttonText = "Open"; }

        if (GUILayout.Button(buttonText))
        {
            switch (buttonText)
            {
                case "Open": ClientPipe.StartClient();
                    break;
                case "Close": ClientPipe.StopClient();
                    break;
            }
        }

        ClientPipe.SendData = 
            GUILayout.TextField(ClientPipe.SendData);

        GUILayout.Label("Status: " + status);

        GUILayout.EndVertical();
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();

        GUILayout.EndArea();

        #endregion Pipe Controls
    }

    /// <summary>
    /// A little additional extra clean up just in
    /// case its needed.
    /// </summary>
    void OnDestroy()
    {
        try
        {
            ClientPipe.StopClient();
            ClientPipe = null;
        }
        catch
        {
            print("Failed to stop client on destroy!");
        }
    }

    #endregion Unity Events

    #region Custom Events

    /// <summary>
    /// Event called when the server is shut down and
    /// the client is still running. In this instance
    /// this will happen if the winform app is either
    /// shutdown or the close server button is pressed.
    /// </summary>
    void ClientPipe_PipeBrokenEvent()
    {
        OutData = "Disconnected!";

        ClientPipe.StopClient();
    }

    #endregion Custom Events
}
