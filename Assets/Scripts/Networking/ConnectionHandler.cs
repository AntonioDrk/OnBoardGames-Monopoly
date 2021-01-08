using UnityEngine;
using UnityEngine.Networking;

public class ConnectionHandler : NetworkManager
{

    public override void OnServerDisconnect(NetworkConnection conn)
    {
        // When a client disconnects this will be called
        Debug.LogWarning("A client disconnected");

        GameManager gameManagerScript = GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>();
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
    
}
