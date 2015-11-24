using UnityEngine;
using System.Collections;
using System;

public class NetworkManager : MonoBehaviour 
{
    public static NetworkPlayer server;

    public GameObject playerPrefab;

	void Start () 
    {
        string[] args = System.Environment.GetCommandLineArgs();

        foreach (string arg in args)
        {
            if (arg == "-batchmode")
            {
                Network.InitializeServer(10, 45678, false);
                Network.InitializeServer(10, 45679, false);
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

        server = networkView.owner;

        NetworkViewID playerID = Network.AllocateViewID();
        networkView.RPC2("spawnPlayer", RPCMode.All, playerID);
    }

    void OnPlayerConnected()
    {
        Debug.Log("client connected");
    }

    void OnServerInitialized()
    {
        Debug.Log("server initialized");
    }

    [RPC]
    void spawnPlayer(NetworkViewID viewID)
    {
        Debug.Log("received test rpc: " + viewID);

        GameObject player = GameObject.Instantiate(playerPrefab) as GameObject;
        player.networkView.viewID = viewID;
    }

    [RPC]
    void relay(string methodName, NetworkViewID viewID)
    {
        Debug.Log("received rpc at relay, sending back out to clients: " + methodName);
        networkView.RPC(methodName, RPCMode.Others, viewID);
    }
}
