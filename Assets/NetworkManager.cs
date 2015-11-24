using UnityEngine;
using System.Collections;
using System;

public class NetworkManager : MonoBehaviour 
{

	void Start () 
    {
        string[] args = System.Environment.GetCommandLineArgs();

        foreach (string arg in args)
        {
            if (arg == "-batchmode")
            {
                Network.InitializeServer(10, 45678, true);
                Network.InitializeServer(10, 45679, true);
            }
        }
	}

    public void OnGUI()
    {
        if (GUI.Button(new Rect(0, 10, 150, 100), "Connect 1"))
        {
            Network.Connect("50.57.111.104", 45678);
        }
        if (GUI.Button(new Rect(0, 120, 150, 100), "Connect 2"))
        {
            Network.Connect("50.57.111.104", 45679);
        }
    }

    void OnConnectedToServer()
    {
        Debug.Log("connected");

        networkView.RPC2("testRPC", RPCMode.All, 666);
    }

    void OnPlayerConnected()
    {
        Debug.Log("player connected");
    }

    void OnServerInitialized()
    {
        Debug.Log("server initialized");
    }

    [RPC]
    void testRPC(int arg)
    {
        Debug.Log("received test rpc: " + arg);
    }

    [RPC]
    void relay(string methodName, int data)
    {
        Debug.Log("received rpc at relay, sending back out to clients: " + methodName);
        networkView.RPC(methodName, RPCMode.All, data);
    }
}
