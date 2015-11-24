using UnityEngine;
using System.Collections;

public static class Util 
{

    public static void RPC2(this NetworkView networkView, string methodName, RPCMode mode, params object[] args)
    {
        Debug.Log("sending rpc to relay: " + methodName);

        object[] newArgs = new object[args.Length + 1];
        args.CopyTo(newArgs, 1);
        newArgs[0] = methodName;
        networkView.RPC("relay", RPCMode.All, newArgs);
    }
}
