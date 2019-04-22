using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class ConnectionHandler : NetworkManager
{
    GameManager gameManagerScript;

    private void Update()
    {
        if(gameManagerScript == null && GameObject.FindGameObjectWithTag("GameManager") != null)
            gameManagerScript = GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>();
    }

    public override void OnServerDisconnect(NetworkConnection conn)
    {
        // When a client disconnects this will be called
        Debug.LogWarning("A client disconnected");

        if (gameManagerScript != null)
        {
            foreach (NetworkInstanceId id in conn.clientOwnedObjects)
            {
                gameManagerScript.CmdOnPlayerDisconnect(id);
                break;
            }
        }
        else
        {
            Debug.LogError("I didn't find the game manager script");
        }

        base.OnServerDisconnect(conn);
    }

    public override void OnStartServer()
    {
        // This is called when the server starts
        base.OnStartServer();
    }
}
