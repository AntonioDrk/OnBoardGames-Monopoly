using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using UnityEngine.EventSystems;

public class GameManager : NetworkBehaviour
{

    public bool targetPlayerIsMoving = false;
    public GameObject targetPlayer;
    public int currentRolledNumber;

    private Camera mainCamera;    
    private Vector3 cameraDistance = new Vector3(0, -3.5f, 3f); // the distance of the camera from the target player when it's moving    
    private Vector3 cameraPosition = new Vector3(-5, 18, 0.5f); // the position of the camera when the target player is not moving
    
    private List<Color> playerColors; // colors of the players    
    private List<GameObject> playerInfo = new List<GameObject>(); // info panels of the players

    [SyncVar] public bool gameStarted;
    [SyncVar] public int playerTurn = 0;
    [SyncVar] public int playerTurnId = 0;    
    [SyncVar] public int connectedPlayers = 0;
    public List<GameObject> players;
    public SyncListInt cardsOwner;
    [SyncVar] public int chestJailCardOwner;
    [SyncVar] public int chanceJailCardOwner;

    // debug info
    private Text connectedPlayersText;

    void Start()
    {

        mainCamera = Camera.main;

        chestJailCardOwner = -1;
        chanceJailCardOwner = -1;

        players = new List<GameObject>();
        if (isServer)
        {
            gameStarted = false;
            UIManager.startGameButton.GetComponent<Button>().onClick.AddListener(startGame);
            UIManager.startGameButton.SetActive(true);
            addPlayerColor(new List<Color> { new Color32(0, 108, 0, 255), new Color32(200, 7, 0, 255), new Color32(0, 21, 161, 255), new Color32(160, 130, 0, 255),
                new Color32(139, 0, 162, 255), Color.black });
        }

        for (int i = 0; i < 28; i++)
            cardsOwner.Add(-1);

        // debug info
        // connectedPlayersText = GameObject.Find("connectedPlayersText").GetComponent<Text>();       
        
    }

    void Update()
    {

        // connectedPlayersText.text = connectedPlayers + " players";

        if (gameStarted && isServer)
        {
            for (int i = 0; i < connectedPlayers; i++)
                CmdChangeMoneyOnPanel(i, players[i].GetComponent<Player>().getMoney());
        
            if (Input.GetKeyDown(KeyCode.BackQuote))
                {
                    if (!UIManager.console.activeInHierarchy)
                    {
                        UIManager.console.SetActive(true);
                        InputField input = GameObject.Find("ConsoleInput").GetComponent<InputField>();
                        input.Select();
                        input.ActivateInputField();
                        input.text = "";
                        EventSystem.current.SetSelectedGameObject(input.gameObject, null);
                        input.OnPointerClick(null);   
                    }
                    else
                        UIManager.console.SetActive(false);
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

        Debug.Log("Nr of players: " + connectedPlayers);
        gameStarted = true;
        UIManager.startGameButton.GetComponent<Button>().onClick.RemoveAllListeners();
        UIManager.startGameButton.SetActive(false);
        Debug.Log("GAME STARTED");

        for (int i = 0; i < connectedPlayers; i++)
        {
            Debug.Log("Server creates info for player " + players[i].GetComponent<Player>().idPlayer);
            playerInfo.Add(Instantiate(Resources.Load<GameObject>("PlayerInfo")));
            NetworkServer.Spawn(playerInfo[i]);
            players[i].GetComponent<Player>().RpcCreatePlayerInfo(i, playerInfo[i]);

            GameObject playerTradePanel = Instantiate(Resources.Load<GameObject>("PlayerInfo"));
            NetworkServer.Spawn(playerTradePanel);
            players[i].GetComponent<Player>().RpcCreatePlayerTradeInfo(i, playerTradePanel);

        }
        players[0].GetComponent<Player>().RpcChangeColorOnPanel(playerInfo[0], 255, 190, 190, 190);

        for (int i = 0; i < connectedPlayers; i++)
            players[i].GetComponent<Player>().RpcAddButtonToPlayerTradeInfo();

    }

    // this function is only for the server
    private int getPlayerIndex(int idPlayer)
    {
        for (int i = 0; i < connectedPlayers; i++)
            if (players[i].GetComponent<Player>().idPlayer == idPlayer)
                return i;
        Debug.LogError("Player " + idPlayer + " is out.");
        return -1;
    }

    public void CmdSendTrade(int sourceId, int destinationId, int[] sourceProperties, int sourcePropertiesLength, int[] destinationProperties, int destinationPropertiesLength)
    {
        if (getPlayerIndex(destinationId) == -1)
            players[getPlayerIndex(sourceId)].GetComponent<Player>().RpcRefusedTrade();
        else
            players[getPlayerIndex(destinationId)].GetComponent<Player>().RpcReceiveTrade(sourceId, destinationId, sourceProperties, sourcePropertiesLength, destinationProperties, destinationPropertiesLength);
    }

    public void CmdRefusedTrade(int sourceId)
    {
        players[getPlayerIndex(sourceId)].GetComponent<Player>().RpcRefusedTrade();
    }

    public void CmdExecuteTrade(int sourceId, int destinationId, int[] sourceProperties, int sourcePropertiesLength, int[] destinationProperties, int destinationPropertiesLength)
    {

        //Debug.Log("Execute trade from " + sourceId + " to " + destinationId);        
        for(int k=0; k< sourcePropertiesLength; k++)
        {
            int i = sourceProperties[k]; 
            CmdChangeOwner(i, destinationId);

            // the source sells it and the destination buys it
            players[getPlayerIndex(destinationId)].GetComponent<Player>().RpcBuyProperty(i);
            players[getPlayerIndex(sourceId)].GetComponent<Player>().RpcSellProperty(i);
        }

        for (int k = 0; k < destinationPropertiesLength; k++)
        {
            int i = destinationProperties[k];
            CmdChangeOwner(i, sourceId);

            // the destination sells it and the source buys it
            players[getPlayerIndex(sourceId)].GetComponent<Player>().RpcBuyProperty(i);
            players[getPlayerIndex(destinationId)].GetComponent<Player>().RpcSellProperty(i);
        }

        players[getPlayerIndex(sourceId)].GetComponent<Player>().RpcAcceptedTrade();

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

        if (!isServer) return;
        if (gameStarted) return;

        Player playerScript = newPlayer.GetComponent<Player>();        

        players.Add(newPlayer);
        playerScript.idPlayer = connectedPlayers;
        connectedPlayers++;

        // Asign the Color for the player
        Color randomColor = playerColors[Random.Range(0, playerColors.Count - 1)];
        playerScript.setPlyColor(randomColor);
        playerColors.Remove(randomColor);
        playerScript.getRenderer().material.color = playerScript.getPlyColor();

        // Update each player for the new color change
        for (int i = 0; i < connectedPlayers; i++)
        {
            players[i].GetComponent<Player>().RpcUpdateColor(players[i].GetComponent<Player>().getPlyColor());
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

    private bool checkIfPlayerInJail(int id)
    {
        for (int i = 0; i < connectedPlayers; i++)
            if (players[i].transform.position == new Vector3(-11f, 0.125f, -6f) && players[i].GetComponent<Player>().idPlayer != id)
                return true;
        return false;
    }
    
    public void CmdGetOutOfJail(int id)
    {
        if (!checkIfPlayerInJail(id))
            for (int i = 0; i < connectedPlayers; i++)
                players[i].GetComponent<Player>().RpcGetOutOfJail();
    }

    public void CmdNextPlayer()
    {
        if (!isServer) return;

        players[playerTurn].GetComponent<Player>().RpcChangeColorOnPanel(playerInfo[playerTurn], 255, 255, 255, 190);
        playerTurn = (playerTurn + 1) % connectedPlayers;
        Debug.Log("playerTurn: " + playerTurn);
        players[playerTurn].GetComponent<Player>().RpcChangeColorOnPanel(playerInfo[playerTurn], 255, 190, 190, 190);
        playerTurnId = players[playerTurn].GetComponent<Player>().idPlayer;
    }

    public void CmdChangeOwner(int cardIndex, int newOwnerId)
    {
        if (!isServer) return;
        cardsOwner[cardIndex] = newOwnerId;
        
        for (int i = 0; i < connectedPlayers; i++)
        {
            Color ownerColor = Color.clear;
            if (newOwnerId != -1)
                ownerColor = players[getPlayerIndex(newOwnerId)].GetComponent<Player>().getPlyColor();
            players[i].GetComponent<Player>().RpcChangeOwnerPanelColor(newOwnerId, CardReader.getIdOnBoard(cardIndex), ownerColor);
        }

        Debug.Log("Owner changed for " + cardIndex + " : " + newOwnerId);
    }

    public void CmdPayEachPlayer(int id, int amount)
    {
        for (int i = 0; i < connectedPlayers; i++)
            if(players[i].GetComponent<Player>().idPlayer != id)
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
        players[getPlayerIndex(playerId)].GetComponent<Player>().CmdAddMoney(amount);
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
        
        int idPlayer = playerDisconnected.GetComponent<Player>().idPlayer;
        int indexPlayer = getPlayerIndex(idPlayer);

        if (playerTurnId == idPlayer)
            CmdNextPlayer();
        else if (playerTurnId > idPlayer)
            playerTurn--;

        // SELL HIS PROPERTIES TO THE BANK
        for (int i = 0; i < 28; i++)
            if (cardsOwner[i] == idPlayer)
                CmdChangeOwner(i, -1);

        // DESTROY PANEL OF DISCONNECTED PLAYER
        Destroy(playerInfo[indexPlayer]);
        playerInfo.Remove(playerInfo[indexPlayer]);
      
        players.Remove(playerDisconnected);
        NetworkServer.Destroy(playerDisconnected);

        connectedPlayers--;

        // Change the postions of the rest panels
        for (int i = 0; i < connectedPlayers; i++)
        {
            players[i].GetComponent<Player>().RpcChangePositionOfPlayerPanel(i, playerInfo[i]);
        }
    }
    
}
