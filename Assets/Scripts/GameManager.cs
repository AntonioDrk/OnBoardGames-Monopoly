using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using UnityEngine.EventSystems;

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
    public GameObject playerInfoPrefab, tradePanelPrefab, propertyTradePanelPrefab;
    private GameObject[] playerInfo = new GameObject[6];
    public bool gameStarted = false;
    private GameObject startGameButton;
    public int currentRolledNumber;

    private List<Color> playerColors;

    [SyncVar] public int chestJailCardOwner;
    [SyncVar] public int chanceJailCardOwner;

    void Start()
    {
        chestJailCardOwner = -1;
        chanceJailCardOwner = -1;

        mainCamera = Camera.main;

        connectedPlayersText = GameObject.Find("connectedPlayersText").GetComponent<Text>();
        playerTurnText = GameObject.Find("playerTurnText").GetComponent<Text>();
        playerTurnText.text = "Turn: Player " + playerTurn;
        startGameButton = GameObject.Find("StartGame");
        startGameButton.GetComponent<Button>().onClick.AddListener(startGame);

        if (!isServer)
        {
            startGameButton.SetActive(false);
        }
            
        players = new List<GameObject>();

        // Make sure we have meshes to assign the player
        if (meshes.Count == 0)
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

        if (isServer)
            addPlayerColor(new List<Color> { new Color32(0, 108, 0, 255), new Color32(200, 7, 0, 255), new Color32(0, 21, 161, 255), new Color32(160, 130, 0, 255), new Color32(139, 0, 162, 255), Color.black });

    }

    void Update()
    {
        playerTurnText.text = "Turn: Player " + playerTurn;
        connectedPlayersText.text = connectedPlayers + " players";

        if (gameStarted && isServer)
        {
            for (int i = 0; i < connectedPlayers; i++)
                CmdChangeMoneyOnPanel(i, players[i].GetComponent<Player>().getMoney());
        
            if (Input.GetKeyDown(KeyCode.BackQuote))
                {
                    bool consoleState = CardReader.console.activeInHierarchy;

                    if (!consoleState)
                    {
                        CardReader.console.SetActive(true);
                        InputField input = GameObject.Find("ConsoleInput").GetComponent<InputField>();
                        input.Select();
                        input.ActivateInputField();
                        input.text = "";
                        EventSystem.current.SetSelectedGameObject(input.gameObject, null);
                        input.OnPointerClick(null);   
                    }
                    else
                        CardReader.console.SetActive(false);
                }
        
        }

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

            GameObject playerTradePanel = Instantiate(playerInfoPrefab);
            NetworkServer.Spawn(playerTradePanel);
            players[i].GetComponent<Player>().RpcCreatePlayerTradeInfo(i, playerTradePanel);

        }
        players[0].GetComponent<Player>().RpcChangeColorOnPanel(playerInfo[0], 255, 190, 190, 190);

        for (int i = 0; i < nrOfPlayers; i++)
            players[i].GetComponent<Player>().RpcAddButtonToPlayerTradeInfo();

    }

    public void CmdSendTrade(int sourceId, int destinationId, int[] sourceProperties, int sourcePropertiesLength, int[] destinationProperties, int destinationPropertiesLength)
    {
            players[destinationId].GetComponent<Player>().RpcReceiveTrade(sourceId, destinationId, sourceProperties, sourcePropertiesLength, destinationProperties, destinationPropertiesLength);
    }

    public void CmdRefusedTrade(int sourceId)
    {
        players[sourceId].GetComponent<Player>().RpcRefusedTrade();
    }

    public void CmdExecuteTrade(int sourceId, int destinationId, int[] sourceProperties, int sourcePropertiesLength, int[] destinationProperties, int destinationPropertiesLength)
    {

        //Debug.Log("Execute trade from " + sourceId + " to " + destinationId);        
        for(int k=0; k< sourcePropertiesLength; k++)
        {
            int i = sourceProperties[k];
            int idOnBoard = -1; 
            if (i < 22)
                idOnBoard = CardReader.propertyCards[i].id;
            else if (i < 26)
                idOnBoard = CardReader.railroadCards[i - 22].id;
            else
                idOnBoard = CardReader.utilityCards[i - 26].id;
            
            CmdChangeOwner(i, destinationId, idOnBoard);
            // the source sells it and the destination buys it
            players[destinationId].GetComponent<Player>().RpcBuyProperty(i);
            players[sourceId].GetComponent<Player>().RpcSellProperty(i);

        }

        for (int k = 0; k < destinationPropertiesLength; k++)
        {
            int i = destinationProperties[k];
            int idOnBoard = -1;
            if (i < 22)
                idOnBoard = CardReader.propertyCards[i].id;
            else if (i < 26)
                idOnBoard = CardReader.railroadCards[i - 22].id;
            else
                idOnBoard = CardReader.utilityCards[i - 26].id;
            
            CmdChangeOwner(i, sourceId, idOnBoard);
            // the destination sells it and the source buys it
            players[sourceId].GetComponent<Player>().RpcBuyProperty(i);
            players[destinationId].GetComponent<Player>().RpcSellProperty(i);
        }

        players[sourceId].GetComponent<Player>().RpcAcceptedTrade();

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

    public void CmdChangeCardJailOwner(int ownerId, string type)
    {
        if (type == "Chest")
            chestJailCardOwner = ownerId;
        else
            chanceJailCardOwner = ownerId;
    }

    [Command]
    public void CmdChangeMoneyOnPanel(int i, int money)
    {
        players[i].GetComponent<Player>().RpcChangeMoneyOnPanel(playerInfo[i]);
    }
    
    public void CmdJailAnimation()
    {
        for (int i = 0; i < connectedPlayers; i++)
            players[i].GetComponent<Player>().RpcJailAnimation();
    }


    public void CmdGetOutOfJail(int id)
    {
        int ok = 0;
        for (int i = 0; i < connectedPlayers; i++)
            if (players[i].transform.position == new Vector3(-11f, 0.125f, -6f) && i != id)
                ok = 1;

        if (ok == 0)
            for (int i = 0; i < connectedPlayers; i++)
                players[i].GetComponent<Player>().RpcGetOutOfJail();
    }

    public void CmdNextPlayer()
    {
        // Make sure this is run only on the server to not fuck up something
        if (!isServer)
            return;

        players[playerTurn].GetComponent<Player>().RpcChangeColorOnPanel(playerInfo[playerTurn], 255, 255, 255, 190);
        playerTurn = (playerTurn + 1) % connectedPlayers;
        Debug.Log("playerTurn: " + playerTurn);
        players[playerTurn].GetComponent<Player>().RpcChangeColorOnPanel(playerInfo[playerTurn], 255, 190, 190, 190);
    }

    public void CmdChangeOwner(int cardIndex, int newOwnerId, int id)
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

    public void CmdPayEachPlayer(int id, int amount)
    {
        for (int i = 0; i < connectedPlayers; i++)
            if(i != id)
            {
                CmdGiveMoneyToPlayer(i, amount);
            }
    }

    public void CmdGiveMoneyToPlayer(int playerId, int amount)
    {
        if (!isServer)
        {
            Debug.LogError("Clientul a incercat sa intre in cmd");
            return;
        }
        players[playerId].GetComponent<Player>().CmdAddMoney(amount);
        //Debug.LogError("Added " + amount + " to " + playerId);
    }

    public void CmdConstructHouse(string housePrefabPath, Vector3 position, Vector3 rotation, int cardIndex)
    {
        // Instantiate in world
        GameObject go = Instantiate(Resources.Load<GameObject>(housePrefabPath), position, Quaternion.Euler(rotation));

        NetworkServer.Spawn(go);

        // Update the number of houses on the property for all the clients
        RpcUpdateHouses(cardIndex, go.GetComponent<NetworkIdentity>().netId);
    }


    public void CmdConstructHotel(string hotelPrefabPath, Vector3 position, Vector3 rotation, int cardIndex)
    {
        if (!isServer)
        {
            Debug.LogError("Client tried to run CmdConstructHotel");
            return;
        }

        foreach (NetworkInstanceId objectNetId in CardReader.propertyCards[cardIndex].buildings)
        {
            NetworkServer.Destroy(NetworkServer.FindLocalObject(objectNetId));

        }

        Debug.Log("In cmd construct hotel");
        RpcRemoveHouses(cardIndex);

        // Instantiate in world
        GameObject go = Instantiate(Resources.Load<GameObject>(hotelPrefabPath), position, Quaternion.Euler(rotation));

        NetworkServer.Spawn(go);

        // Update the number of houses on the property for all the clients
        RpcUpdateHouses(cardIndex, go.GetComponent<NetworkIdentity>().netId);

    }

    public void CmdDeconstructHotel(int cardIndex)
    {
        NetworkInstanceId lastBuilding = CardReader.propertyCards[cardIndex].buildings[CardReader.propertyCards[cardIndex].buildings.Count - 1];
        NetworkServer.Destroy(NetworkServer.FindLocalObject(lastBuilding));
        RpcRemoveHotel(cardIndex);
    }

    public void CmdDeconstructHouse(int cardIndex)
    {
        NetworkInstanceId lastBuilding = CardReader.propertyCards[cardIndex].buildings[CardReader.propertyCards[cardIndex].buildings.Count - 1];
        NetworkServer.Destroy(NetworkServer.FindLocalObject(lastBuilding));
        RpcRemoveHouse(cardIndex);
    }

    [ClientRpc]
    public void RpcUpdateHouses(int cardIndex, NetworkInstanceId netId)
    {
        CardReader.propertyCards[cardIndex].housesBuilt += 1;
        if (CardReader.propertyCards[cardIndex].housesBuilt == 5)
            CardReader.propertyCards[cardIndex].hasHotel = true;
        CardReader.propertyCards[cardIndex].addHouseObject(netId);
    }

    [ClientRpc]
    public void RpcRemoveHotel(int cardIndex)
    {
        CardReader.propertyCards[cardIndex].housesBuilt = 0;
        CardReader.propertyCards[cardIndex].hasHotel = false;
        CardReader.propertyCards[cardIndex].removeLastHouse();
    }

    [ClientRpc]
    public void RpcRemoveHouse(int cardIndex)
    {
        CardReader.propertyCards[cardIndex].housesBuilt -= 1;
        CardReader.propertyCards[cardIndex].hasHotel = false;
        CardReader.propertyCards[cardIndex].removeLastHouse();
    }

    [ClientRpc]
    public void RpcRemoveLastPropertyHouse(int cardIndex, GameObject player)
    {
        CardReader.propertyCards[cardIndex].removeLastHouse();
        if (CardReader.propertyCards[cardIndex].housesBuilt == 5)
        {
            Debug.LogWarning("Has hotel then I remove 5 houses from housesBuilt");
            CardReader.propertyCards[cardIndex].housesBuilt = 0;
        }
        else
        {
            Debug.LogWarning("Doesn't have hotel then I remove only one house from housesBuilt");
            CardReader.propertyCards[cardIndex].housesBuilt -= 1;
        }
        CardReader.propertyCards[cardIndex].hasHotel = false;
        
    }

    [ClientRpc]
    public void RpcRemoveHouses(int cardIndex)
    {
        CardReader.propertyCards[cardIndex].removeHouses();
    }

    [Command]
    public void CmdOnPlayerDisconnect(NetworkInstanceId id)
    {
        Debug.Log("REMOVING DCED PLAYER");
        GameObject playerDisconnected = NetworkServer.FindLocalObject(id);
        if (playerDisconnected == null)
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
