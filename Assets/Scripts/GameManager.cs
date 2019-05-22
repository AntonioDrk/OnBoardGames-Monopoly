using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

public class GameManager : NetworkBehaviour
{
    public int nrOfPlayers = 0;
    public bool targetPlayerIsMoving = false;
    public GameObject targetPlayer; 

    private Camera mainCamera;

    // THIS IS TESTING ONLY
    // This is bad v and needs to be changed ASAP, we will pass the players vector between the scripts 
    [SerializeField]
    private Vector3 cameraDistance = new Vector3(0, -3.5f, 3f);
    [SerializeField]
    // the position of the camera when the target player is not moving
    //private Vector3 cameraPosition = new Vector3(-5f, 8f, -9.5f); 
    private Vector3 cameraPosition;
    [SerializeField]
    private Text playerTurnText, connectedPlayersText;

    [SyncVar] public int playerTurn = 0; 
    [SyncVar] public int connectedPlayers = 0;
    public List<GameObject> players;
    public SyncListInt cardsOwner;

    [SerializeField] private List<Mesh> meshes; // Set them at startup in the editor!!
    private List<int> meshesIndexes; // Indexes of the meshes to use
    public GameObject playerInfoPrefab;
    private GameObject[] playerInfo = new GameObject[6];
    public bool gameStarted = false;
    private GameObject startGameButton;
    public int currentRolledNumber;

    private List<Color> playerColors;
    
    void Start()
    { 

        mainCamera = Camera.main;
        
        connectedPlayersText = GameObject.Find("connectedPlayersText").GetComponent<Text>();
        playerTurnText = GameObject.Find("playerTurnText").GetComponent<Text>(); 
        playerTurnText.text = "Turn: Player " + playerTurn;
        startGameButton = GameObject.Find("StartGame");
        startGameButton.GetComponent<Button>().onClick.AddListener(startGame);

        if (!isServer)
            startGameButton.SetActive(false);

        players = new List<GameObject>();

        // Make sure we have meshes to assign the player
        if(meshes.Count == 0)
        {
            Debug.LogError("No meshes for players to assign! Assign the meshes in the editor in the gamemanager object");
        }

        meshesIndexes = new List<int>();

        for (int i = 0; i < meshes.Count; i++)
        {
            meshesIndexes.Add(i);
        }

        for (int i = 0; i < 28; i++)
            cardsOwner.Add(-1);

        if(isServer)
            addPlayerColor(new List<Color> { Color.red, Color.yellow, Color.magenta, Color.green, Color.blue, Color.black });

    } 

    void Update()
    {
        playerTurnText.text = "Turn: Player " + playerTurn;
        connectedPlayersText.text = connectedPlayers + " players";
        
        if(gameStarted && isServer)
            for (int i = 0; i < connectedPlayers; i++)
                CmdChangeMoneyOnPanel(i, players[i].GetComponent<Player>().getMoney());

            if (targetPlayerIsMoving)
            UpdatePosCamera(targetPlayer);
        else
        {
            mainCamera.transform.position = cameraPosition;
            mainCamera.transform.eulerAngles = new Vector3(90, 0, 0);
        }
    }
    
    void startGame()
    {
        nrOfPlayers = connectedPlayers;
        Debug.Log("Nr of players: " + nrOfPlayers);
        gameStarted = true;
        startGameButton.GetComponent<Button>().onClick.RemoveAllListeners();
        startGameButton.SetActive(false);

        Debug.Log("GAME STARTED");
        for (int i = 0; i < nrOfPlayers; i++)
        {
            Debug.Log("Server creates info for player " + i);
            playerInfo[i] = Instantiate(playerInfoPrefab);
            NetworkServer.Spawn(playerInfo[i]);
            players[i].GetComponent<Player>().RpcCreatePlayerInfo(i, playerInfo[i]);
        }
        players[0].GetComponent<Player>().RpcChangeColorOnPanel(playerInfo[0], 183, 84, 84, 150);
    }

    private void addPlayerColor(List<Color> colors)
    {
        playerColors = new List<Color>();
        foreach (Color color in colors)
            playerColors.Add(color);
    }

    // Moves the camera to face the player
    void UpdatePosCamera(GameObject target)
    {
        // Get the distance the camera should be from the player (the distance is set in cameraDistance vector)
        // rotation * cameraDistance makes the camera be placed with respect to thge player rotation, so it always stays "BEHIND" the player
        mainCamera.transform.position = target.transform.position - target.transform.rotation * cameraDistance;

        // Take the targets rotation on the y axis and apply it to the camera, don't change the cameras x axis rotation
        mainCamera.transform.eulerAngles = new Vector3(45, target.transform.eulerAngles.y, 0);
        
    }

    // Add players to the list of players
    public void CmdAddConnectedPlayer(GameObject newPlayer)
    {
        // Make sure this is run only on the server to not fuck up something
        if (!isServer) return;

        Player playerScript = newPlayer.GetComponent<Player>();
        MeshFilter meshFilter = newPlayer.transform.GetChild(0).GetComponent<MeshFilter>();

        // We will extract a random mesh from our pool of meshes, this will be changed as players can 
        // select what model they want to play as with
        int selectedMeshIndex = meshesIndexes[Random.Range(0, meshesIndexes.Count - 1)];
        meshesIndexes.Remove(selectedMeshIndex);

        // Update shit server-side
        meshFilter.mesh = meshes[selectedMeshIndex];
        playerScript.setMyMeshIndex(selectedMeshIndex);

        players.Add(newPlayer);
        playerScript.idPlayer = connectedPlayers;
        connectedPlayers++;

        // Asign the Color for the player
        Color randomColor = playerColors[Random.Range(0, playerColors.Count - 1)];
        playerScript.setPlyColor(randomColor);
        playerColors.Remove(randomColor);
        playerScript.getRenderer().material.color = playerScript.getPlyColor();

        // Update each player for the new color change and the new mesh change
        for (int i = 0; i < connectedPlayers; i++)
        {
            players[i].GetComponent<Player>().RpcUpdateColor(players[i].GetComponent<Player>().getPlyColor());
            players[i].GetComponent<Player>().RpcUpdateMesh(players[i].GetComponent<Player>().getMyMeshIndex());
        }
        
    }

    [Command]
    public void CmdChangeMoneyOnPanel(int i, int money)
    {
        players[i].GetComponent<Player>().RpcChangeMoneyOnPanel(playerInfo[i]);
    }

    public void CmdNextPlayer()
    {
        // Make sure this is run only on the server to not fuck up something
        if (!isServer)
            return;

        players[playerTurn].GetComponent<Player>().RpcChangeColorOnPanel(playerInfo[playerTurn],255,255,255,150);
        playerTurn = (playerTurn + 1) % connectedPlayers;
        Debug.Log("playerTurn: " + playerTurn);
        players[playerTurn].GetComponent<Player>().RpcChangeColorOnPanel(playerInfo[playerTurn],183, 84, 84, 150);
    }
     
    public void CmdChangeOwner(int cardIndex, int newOwnerId,int id)
    { 
        if (!isServer) return;
        cardsOwner[cardIndex] = newOwnerId;

        for (int i = 0; i < nrOfPlayers; i++)
        {
            Color ownerColor = Color.clear;
            if (newOwnerId != -1)
                ownerColor = players[newOwnerId].GetComponent<Player>().getPlyColor();
            players[i].GetComponent<Player>().RpcChangeOwnerPanelColor(newOwnerId, id, ownerColor);
        }

        Debug.Log("Owner changed for " + cardIndex + " : " + newOwnerId);
    }
    
    public void CmdGiveMoneyToPlayer(int playerId, int amount)
    {
        if (!isServer)
        {
            Debug.LogError("Clientul a incercat sa intre in cmd");
            return; // ?
        }
        players[playerId].GetComponent<Player>().CmdAddMoney(amount);
        //Debug.LogError("Added " + amount + " to " + playerId);
    }

    [Command]
    public void CmdOnPlayerDisconnect(NetworkInstanceId id)
    {
        Debug.Log("REMOVING DCED PLAYER");
        GameObject playerDisconnected = NetworkServer.FindLocalObject(id);
        if(playerDisconnected == null)
        {
            Debug.LogError("The player disconnected object is null for some reason!");
            return;
        }

        meshesIndexes.Add(playerDisconnected.GetComponent<Player>().getMyMeshIndex());

        players.Remove(playerDisconnected);
        NetworkServer.Destroy(playerDisconnected);
        connectedPlayers--;
        // DESTROY PANEL OF DISCONNECTED PLAYER
        // SELL HIS PROPERTIES TO THE BANK
    }

//   ----------------  Getters and Setters   ----------------  
    public List<Mesh> getMeshes() { return meshes; }

//   ----------------                        ----------------  
}
