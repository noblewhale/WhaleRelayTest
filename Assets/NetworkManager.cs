using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

public class NetworkManager : MonoBehaviour 
{
    public static NetworkPlayer server;

    public GameObject playerPrefab;

    // Only used on clients
    [NonSerialized]
    public int clientGroup = -1;

    // Only used on server. TODO: Better
    public int nextClientGroup = 1;

    Dictionary<NetworkPlayer, int> clientGroups = new Dictionary<NetworkPlayer,int>();

	void Start () 
    {
        string[] args = System.Environment.GetCommandLineArgs();

        foreach (string arg in args)
        {
            if (arg == "-batchmode")
            {
                Network.InitializeServer(10, 45685, true);
            }
        }
	}

    public void OnGUI()
    {
        if (GUI.Button(new Rect(0, 10, 150, 100), "Connect 1"))
        {
            Network.Connect("50.57.111.104", 45685);
        }
        if (GUI.Button(new Rect(0, 120, 150, 100), "Connect 2"))
        {
            clientGroup = 1;
            Network.Connect("50.57.111.104", 45685);
        }
    }

    void OnConnectedToServer()
    {
        Debug.Log("connected");

        server = networkView.owner;

        if (clientGroup == -1)
        {
            networkView.RPC("getClientGroupFromRelay", server);
        }
        else
        {
            networkView.RPC("setClientGroupOnRelay", server, clientGroup);
        }
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

    // Called by Owner, received by Server
    [RPC]
    void spawnObject(NetworkViewID viewID, NetworkMessageInfo info)
    {
        GameObject dummy = new GameObject();
        dummy.AddComponent<NetworkView>();
        dummy.networkView.viewID = viewID;

        // Disable network view for every player that is not in the sender's group
        int group = clientGroups[info.sender];
        foreach (KeyValuePair<NetworkPlayer, int> playerAndGroup in clientGroups)
        {
            if (playerAndGroup.Value != group)
            {
                dummy.networkView.SetScope(playerAndGroup.Key, false);
            }
        }
    }

    [RPC]
    void setClientGroupOnClient(int group)
    {
        Debug.Log("client received group: " + group);
        clientGroup = group;

        NetworkViewID playerID = Network.AllocateViewID();
        networkView.RPC("spawnObject", server, playerID);
        networkView.RPC2("spawnPlayer", RPCMode.All, playerID);
    }

    [RPC]
    void setClientGroupOnRelay(int group, NetworkMessageInfo info)
    {
        clientGroups[info.sender] = group;

        setClientGroupAndScope(info.sender, group);
    }

    [RPC]
    void getClientGroupFromRelay(NetworkMessageInfo info)
    {
        clientGroups[info.sender] = nextClientGroup;

        setClientGroupAndScope(info.sender, nextClientGroup);

        nextClientGroup++;
    }

    void setClientGroupAndScope(NetworkPlayer client, int group)
    {
        // Don't send updates from existing network views for the new player
        NetworkView[] everyNetworkView = GameObject.FindObjectsOfType<NetworkView>();
        foreach (NetworkView view in everyNetworkView)
        {
            if (view == networkView) continue; // Never disable the NetworkManager's view for any clients

            view.SetScope(client, false);
        }

        networkView.RPC("setClientGroupOnClient", client,  group);
    }

    void OnApplicationQuit()
    {
        Network.Disconnect();
    }
}
