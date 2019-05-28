﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

public class Player : NetworkBehaviour
{
    [SyncVar] public int idPlayer = 0;

    // Serialized just for testing purposes, remove me 
    [SerializeField] [SyncVar] private int indexPosition = 0; // Indicates the position on the board list (the list of cards that are on the board)
    [SerializeField] private GameObject diceManager, gameManager;

    private Animator anim;
    private DiceScript diceScript;
    private GameManager gameManagerScript;
    private Vector3 goPosition = new Vector3(2.5f, 0.125f, -6.49f);
    private Vector3 jailPosition = new Vector3(-11f, 0.125f, -6f);
    private Vector3 justVisitingPosition = new Vector3(-11.45854f, 0.125f, -6.49f);
    private GameObject rollButton, endTurnButton, ownedPropertiesPanel;
    private GameObject jail;
    public GameObject ownedPropertyPanelPrefab;// playerInfoPrefab;
    private Text playerMoneyText, idText;

    [SyncVar] private Color plyColor;

    private Renderer renderer;
    [SyncVar] private int myMeshIndex;

    [SerializeField] [SyncVar] private int money = 1500;
    private int doublesRolled = 0;
    private int roundsInJail = 0;
    private bool inJail = false;
    private bool waitingForTrade = false;
    [SerializeField] private int stage = 0; // 0 = player can roll the dice/ 1 = player rolled / 2 = the player ended his movement

    [SerializeField] private List<Card> ownedPropertyCards;
    private List<GameObject> ownedPropertyList;

    void Start()
    {
        gameManager = GameObject.Find("GameManager");
        gameManagerScript = gameManager.GetComponent<GameManager>();
        diceManager = GameObject.Find("DiceManager");
        diceScript = diceManager.GetComponent<DiceScript>();
        anim = GetComponent<Animator>();
        renderer = transform.GetChild(0).GetComponent<Renderer>();

        if (isLocalPlayer)
        {
            ownedPropertyList = new List<GameObject>();
            ownedPropertiesPanel = GameObject.Find("OwnedProprietiesPanel");
            ownedPropertyCards = new List<Card>();
            rollButton = GameObject.Find("RollDice");
            rollButton.GetComponent<Button>().onClick.AddListener(RollTheDice);
            rollButton.SetActive(false);
            endTurnButton = GameObject.Find("EndTurn");
            endTurnButton.GetComponent<Button>().onClick.AddListener(nextPlayer);
            endTurnButton.SetActive(false);
            jail = GameObject.Find("Jail");
            jail.SetActive(false);

            // the listeners for the buttons from jail panel
            CardReader.inJailCardPanel.transform.GetChild(0).GetChild(0).GetComponent<Button>().onClick.AddListener(() => useCardInJail("Chance")); // chance
            CardReader.inJailCardPanel.transform.GetChild(0).GetChild(1).GetComponent<Button>().onClick.AddListener(() => useCardInJail("Chest")); // chest
            CardReader.inJailCardPanel.transform.GetChild(0).GetChild(2).GetComponent<Button>().onClick.AddListener(payFine); // pay fine
            CardReader.inJailCardPanel.transform.GetChild(0).GetChild(3).GetComponent<Button>().onClick.AddListener(rollDoubles); // roll doubles

            CmdAddConnectedPlayer(this.gameObject);
            idText = GameObject.Find("idText").GetComponent<Text>();
            playerMoneyText = GameObject.Find("playerMoneyText").GetComponent<Text>();
            transform.position = goPosition;
        }

    }

    void FixedUpdate()
    {
        localPlayerUpdate();
    }

    void localPlayerUpdate()
    {
        // exit from update if this is not the local player
        if (!isLocalPlayer)
            return;

        idText.text = "id: " + idPlayer;

        // If it's my turn
        if ((isServer && gameManagerScript.gameStarted) || !isServer)
            if (gameManagerScript.playerTurn == idPlayer && stage == 0)
            {
                if (inJail)
                {
                    playerInJail();
                }
                else
                {
                    CardReader.tradeButton.SetActive(true);
                    rollButton.SetActive(true);
                }
            }
            else
            {
                rollButton.SetActive(false);
                CardReader.tradeButton.SetActive(false);
                CardReader.inJailCardPanel.SetActive(false);
            }


        // if the diceManager rolled and it's player's turn 
        if (diceScript.rolled == true && gameManagerScript.playerTurn == idPlayer)
        {
            diceScript.rolled = false;
            Debug.Log("Player " + idPlayer + " rolled " + gameManager.GetComponent<GameManager>().currentRolledNumber);

            if (inJail)
            {
                if (diceScript.isDouble)
                {
                    diceScript.isDouble = false;
                    inJail = false;
                    CmdGetOutOfJail(idPlayer);
                    roundsInJail = 0;
                    transform.position = justVisitingPosition;
                }
                else if (roundsInJail == 3)
                {
                    CmdTakeMoney(50);
                    inJail = false;
                    CmdGetOutOfJail(idPlayer);
                    roundsInJail = 0;
                    transform.position = justVisitingPosition;
                }
                else // the player is still in jail
                {
                    CmdSetDiceInactive();
                    endTurn();
                }
            }

            if (diceScript.isDouble)
                doublesRolled++;
            else
                doublesRolled = 0;

            if (doublesRolled == 3)
            {
                goToJail();
            }
            else if (!inJail)
                moveSpaces(gameManager.GetComponent<GameManager>().currentRolledNumber);

        }

        playerMoneyText.text = "$" + money;

    }

    public void moveSpaces(int amount)
    {
        StartCoroutine("animateMovement", 1);
    }

    IEnumerator animateMovement(int amountToMove)
    {
        gameManagerScript.targetPlayerIsMoving = true;
        gameManagerScript.targetPlayer = this.gameObject;
        CardReader.canvas.SetActive(false);

        for (int i = 0; i < amountToMove; i++)
        {
            if (indexPosition % 10 == 9 || indexPosition % 10 == 0)
            {
                anim.Play("StraightMovementToCorner", 0);
                CmdMovePlayer(indexPosition + 1);

                yield return new WaitForSeconds(anim.GetCurrentAnimatorStateInfo(0).length);

                if (indexPosition % 10 == 0)
                {
                    transform.eulerAngles += new Vector3(0, 90, 0);
                }

                if (indexPosition == 0)
                {
                    transform.eulerAngles = new Vector3(0, 0, 0);
                    transform.position = goPosition;
                    CmdAddMoney(200);
                }


            }
            else
            {
                anim.Play("StraightMovement", 0);
                CmdMovePlayer(indexPosition + 1);
                yield return new WaitForSeconds(anim.GetCurrentAnimatorStateInfo(0).length);

            }
        }

        CardReader.canvas.SetActive(true);
        gameManagerScript.targetPlayerIsMoving = false;

        if (indexPosition == 30)
        {
            goToJail();
        }
        else
        {
            stage = 2;
            if (diceManager.transform.childCount > 0)
                CmdSetDiceInactive();

            if (indexPosition == 12 || indexPosition == 28) // Utilities
            {
                int utilityIndex = (indexPosition - 12) / 16;
                CardReader.utilityCards[utilityIndex].doAction(this.gameObject);
            }
            else if (indexPosition == 5 || indexPosition == 15 || indexPosition == 25 || indexPosition == 35) // Railroads
            {
                int railroadIndex = (indexPosition - 5) / 10;
                CardReader.railroadCards[railroadIndex].doAction(this.gameObject);
            }
            else if (indexPosition == 4) // Income Tax - Pay $200
            {
                CmdTakeMoney(200);
                endMovement();
            }
            else if (indexPosition == 38) // Luxury Tax - Pay $100
            {
                CmdTakeMoney(100);
                endMovement();
            }
            else if (indexPosition == 2 || indexPosition == 17 || indexPosition == 33) // Comunity Chest
            {
                int eventNr = 0;
                if (gameManagerScript.chestJailCardOwner == -1)
                    eventNr = Random.Range(0, 14);
                else
                    eventNr = Random.Range(0, 13);

                Debug.Log("Event nr: " + eventNr + " triggered.");
                Debug.Log(CardReader.chestCards[eventNr].ToString());
                CardReader.chestCards[eventNr].doAction(this.gameObject);
            }
            else if (indexPosition == 7 || indexPosition == 22 || indexPosition == 36) //Chance
            {
                int eventNr = 0;
                if (gameManagerScript.chanceJailCardOwner == -1)
                    eventNr = Random.Range(0, 14);
                else
                    eventNr = Random.Range(0, 13);

                Debug.Log("Event nr: " + eventNr + " triggered.");
                Debug.Log(CardReader.chanceCards[eventNr].ToString());
                CardReader.chanceCards[eventNr].doAction(this.gameObject);
            }
            else
            {
                int cardIndex = CardReader.getPropertyCardIndex(indexPosition);
                Debug.Log("Property card index: " + cardIndex);
                if (cardIndex != -1) // if it's a property card
                {
                    CardReader.propertyCards[cardIndex].doAction(this.gameObject);
                }
                else
                    endMovement();
            }
        }

    }

    public Renderer getRenderer() { return renderer; }
    public void setPlyColor(Color value) { plyColor = value; }
    public Color getPlyColor() { return plyColor; }
    public int getMyMeshIndex() { return myMeshIndex; }
    public void setMyMeshIndex(int value) { myMeshIndex = value; }
    public int getIndexPosition() { return indexPosition; }
    public int getStage() { return stage; }
    public int getMoney() { return money; }

    public void endMovement()
    {
        //CmdSetDiceInactive();
        if (diceScript.isDouble)
        {
            stage = 0;
        }
        else
            endTurn();
    }

    void endTurn()
    {
        endTurnButton.SetActive(true);
    }

    void nextPlayer()
    {
        endTurnButton.SetActive(false);
        stage = 0;
        CmdNextPlayer();
    }
    
    [ClientRpc]
    public void RpcBuyProperty(int id)
    {
        if (!isLocalPlayer) return;

        Card card;
        if (id < 22)
            card = CardReader.propertyCards[id];
        else if (id < 26)
            card = CardReader.railroadCards[id - 22];
        else
            card = CardReader.utilityCards[id - 26];

        buyProperty(card);
    }

    [ClientRpc]
    public void RpcSellProperty(int id)
    {
        if (!isLocalPlayer) return;

        Card card;
        if (id < 22)
            card = CardReader.propertyCards[id];
        else if (id < 26)
            card = CardReader.railroadCards[id - 22];
        else
            card = CardReader.utilityCards[id - 26];
         
        sellProperty(card);
    }

    public void buyProperty(Card propertyCard)
    {
        Debug.Log("Bought " + propertyCard.CardName);

        int numberOfOwnedCards = ownedPropertyCards.Count;
        //Debug.Log("Nr of owned cards: " + numberOfOwnedCards);

        GameObject ownedPropertyPanel = Instantiate(ownedPropertyPanelPrefab);

        if (propertyCard.GetType() == typeof(PropertyCard))
            ownedPropertyPanel.transform.GetComponent<Image>().color = new Color32((byte)((PropertyCard)propertyCard).cardColor[0],
                (byte)((PropertyCard)propertyCard).cardColor[1], (byte)((PropertyCard)propertyCard).cardColor[2], 255);

        ownedPropertyPanel.transform.SetParent(ownedPropertiesPanel.transform);
        ownedPropertyPanel.transform.position = new Vector3(0, 0, 0);
        ownedPropertyPanel.GetComponent<RectTransform>().localScale = new Vector3(1, 1, 1);
        ownedPropertyPanel.transform.GetChild(0).GetComponent<Text>().text = propertyCard.CardName;
        ownedPropertyPanel.GetComponent<Button>().onClick.AddListener(() => propertyCard.showOwnedCard(this.gameObject));
        //Debug.LogError("Owned Property Panel List Count: " + ownedPropertyList.Count);

        // find the new position in the list
        int newIndex = 0;
        if (numberOfOwnedCards > 0)
            while (newIndex < numberOfOwnedCards && ownedPropertyCards[newIndex].Id < propertyCard.Id) newIndex++;

        // last position
        if (newIndex == numberOfOwnedCards)
        {
            ownedPropertyCards.Add(propertyCard);
            ownedPropertyList.Add(ownedPropertyPanel);
            changePositionOfPanel(ownedPropertyPanel, newIndex);
        }
        else
        {
            ownedPropertyCards.Insert(newIndex, propertyCard);
            ownedPropertyList.Insert(newIndex, ownedPropertyPanel);
            changePositionOfPanel(ownedPropertyPanel, newIndex);
            for (int k = newIndex + 1; k < numberOfOwnedCards + 1; k++)
                changePositionOfPanel(ownedPropertyList[k], k);
        }

    }

    void changePositionOfPanel(GameObject ownedPropertyPanel, int position)
    {
        ownedPropertyPanel.GetComponent<RectTransform>().offsetMax = new Vector2(0, -29.7f * position);
        ownedPropertyPanel.GetComponent<RectTransform>().offsetMin = new Vector2(0, 363 - 29.7f * position);
    }

    public void sellProperty(Card propertyCard)
    {

        Debug.Log("Sold " + propertyCard.CardName);
        int cardIndex = ownedPropertyCards.IndexOf(propertyCard);
        if (cardIndex == -1)
        {
            Debug.LogError("Tried to delete " + propertyCard.CardName);
            return;
        }

        ownedPropertyCards.Remove(propertyCard);

        // move all the cards under the removed one
        for (int k = cardIndex + 1; k < ownedPropertyList.Count; k++)
        {
            changePositionOfPanel(ownedPropertyList[k], k - 1);
        }

        Destroy(ownedPropertyList[cardIndex]);
        ownedPropertyList.RemoveAt(cardIndex);

    }

    // This function is to make the link between the button on click event and sending a command
    void RollTheDice()
    {
        if (GameObject.Find("TradePanel") || waitingForTrade) return;
        stage = 1;
        rollButton.SetActive(false);
        CmdRollDice();
    }

    // Tell the server to roll the dices from the Dice Manager 
    [Command]
    public void CmdRollDice()
    {
        diceScript.CmdRollDice();
    }

    [ClientRpc]
    public void RpcMovePlayer(int rolledNumber, int rolledValue)
    {
        if (!isLocalPlayer) return;
        Debug.Log("Move player: " + idPlayer);

        if (rolledValue == rolledNumber - rolledValue)
            diceScript.isDouble = true;
        else
            diceScript.isDouble = false;

        gameManager.GetComponent<GameManager>().currentRolledNumber = rolledNumber;
        diceScript.rolled = true;
    }

    // ----------- jail related functions

    void useCardInJail(string type)
    {
        CmdChangeCardJailOwner(-1, type);
        CardReader.inJailCardPanel.SetActive(false);
        inJail = false;
        CmdGetOutOfJail(idPlayer);
        roundsInJail = 0;
        transform.position = justVisitingPosition;
        RollTheDice();

    }

    void payFine()
    {
        CmdTakeMoney(50);
        CardReader.inJailCardPanel.SetActive(false);
        inJail = false;
        CmdGetOutOfJail(idPlayer);
        roundsInJail = 0;
        transform.position = justVisitingPosition;
        RollTheDice();
    }

    void rollDoubles()
    {
        CardReader.inJailCardPanel.SetActive(false);
        roundsInJail += 1;
        RollTheDice();
    }

    void playerInJail()
    {
        CardReader.inJailCardPanel.SetActive(true);
        if (gameManagerScript.chanceJailCardOwner != idPlayer)
            CardReader.inJailCardPanel.transform.GetChild(0).GetChild(0).gameObject.SetActive(false);
        if (gameManagerScript.chestJailCardOwner != idPlayer)
            CardReader.inJailCardPanel.transform.GetChild(0).GetChild(1).gameObject.SetActive(false);
    }

    [ClientRpc]
    public void RpcJailAnimation()
    {
        if (!isLocalPlayer)
            return;
        jail.SetActive(true);
        jail.GetComponent<Animator>().Play("JailAnimation", 0);
    }

    [Command]
    void CmdJailAnimation()
    {
        gameManagerScript.CmdJailAnimation();
    }

    [ClientRpc]
    public void RpcGetOutOfJail()
    {
        if (!isLocalPlayer)
            return;
        jail.SetActive(false);
    }

    [Command]
    void CmdGetOutOfJail(int id)
    {
        gameManagerScript.CmdGetOutOfJail(id);
    }

    public void goToJail()
    {
        CmdJailAnimation();
        CmdMovePlayer(10);
        doublesRolled = 0;
        roundsInJail = 0;
        inJail = true;
        if (diceManager.transform.childCount > 0)
            CmdSetDiceInactive();
        transform.position = jailPosition;
        transform.eulerAngles = new Vector3(0, 90, 0);
        endTurn();
    }

    // ----------------------- 

    [ClientRpc]
    public void RpcUpdateColor(Color col)
    {
        renderer.material.color = col;
        plyColor = col;
    }

    [ClientRpc]
    public void RpcUpdateMesh(int newMeshIndex)
    {
        transform.GetChild(0).GetComponent<MeshFilter>().mesh = gameManagerScript.getMeshes()[newMeshIndex];
        myMeshIndex = newMeshIndex;
    }

    [ClientRpc]
    public void RpcCreatePlayerInfo(int id, GameObject playerInfo)
    {
        Debug.Log("Player " + id + " added info");
        playerInfo.transform.GetChild(0).GetComponent<Text>().color = plyColor;
        playerInfo.transform.GetChild(0).GetComponent<Text>().text = "Player " + id + "\n$" + money;
        playerInfo.transform.SetParent(GameObject.Find("PlayersPanel").transform);
        playerInfo.GetComponent<RectTransform>().offsetMax = new Vector2(-17, -(12 + 58 * id));
        playerInfo.GetComponent<RectTransform>().offsetMin = new Vector2(21, 260 - 60 * id);
        playerInfo.GetComponent<RectTransform>().localScale = new Vector3(1, 1, 1);
    }

    [ClientRpc]
    public void RpcCreatePlayerTradeInfo(int id, GameObject playerInfo)
    {
        playerInfo.transform.GetChild(0).GetComponent<Text>().color = plyColor;
        playerInfo.transform.GetChild(0).GetComponent<Text>().text = "Player " + id;
        playerInfo.transform.SetParent(CardReader.playerTradePanel.transform);
        playerInfo.GetComponent<RectTransform>().offsetMax = new Vector2(-17, -(12 + 58 * id));
        playerInfo.GetComponent<RectTransform>().offsetMin = new Vector2(21, 260 - 60 * id);
        playerInfo.GetComponent<RectTransform>().localScale = new Vector3(1, 1, 1);
    }
    
    // ------------------ TRADE -------------------

    [ClientRpc]
    public void RpcAddButtonToPlayerTradeInfo()
    {
        if (!isLocalPlayer) return;
        //Debug.Log("panels: " + CardReader.playerTradePanel.transform.childCount);
        for (int k = 0; k < CardReader.playerTradePanel.transform.childCount; k++)
            if(k != idPlayer)
        {
            Transform panel = CardReader.playerTradePanel.transform.GetChild(k);
            int destinationId = k;
            panel.gameObject.AddComponent<Button>().onClick.AddListener(() => playerWantsTrade(destinationId));
        }
    }
    
    // create trade panel
    void playerWantsTrade(int destinationId)
    {

        if (waitingForTrade) return;

        CardReader.closePlayerTradePanel();
        GameObject tradePanel = Instantiate(gameManagerScript.tradePanelPrefab);
        tradePanel.transform.SetParent(CardReader.canvas.transform);
        tradePanel.transform.localPosition = new Vector3(0, 0, 0);
        tradePanel.name = "TradePanel";

        List<int> sourceProperties = new List<int>();
        List<int> destinationProperties = new List<int>();

        // create panels for your properties
        createPanelsForPlayerProperties(tradePanel, 0, idPlayer, sourceProperties);

        // create panels for the other player properties
        createPanelsForPlayerProperties(tradePanel, 1, destinationId, destinationProperties);

        // the button
        tradePanel.transform.GetChild(2).transform.GetChild(0).GetComponent<Text>().text = "Trade";
        tradePanel.transform.GetChild(2).GetComponent<Button>().onClick.AddListener(() => 
                    sendTrade(tradePanel, idPlayer, destinationId, sourceProperties, destinationProperties));
        tradePanel.transform.GetChild(3).GetComponent<Button>().onClick.AddListener(() =>
                    destroyGameObject(tradePanel)); // cancel button

    }
    
    [ClientRpc]
    public void RpcReceiveTrade(int sourceId, int destinationId, int[] sourceProperties, int sourcePropertiesLength, int[] destinationProperties, int destinationPropertiesLength)
    {

        // you are the destination now and the other player is the source
        if (!isLocalPlayer) return;

        GameObject tradePanel = Instantiate(gameManagerScript.tradePanelPrefab);
        tradePanel.transform.SetParent(CardReader.canvas.transform);
        tradePanel.transform.localPosition = new Vector3(0, 0, 0);
        
        // create panels for your properties (destinationId)
        int idOfOwnedProp = -1;
        for (int i = 0; i < destinationPropertiesLength; i++)
        {
            idOfOwnedProp++;
            GameObject panel = createIndividualPanel(tradePanel, 0, idOfOwnedProp, destinationProperties[i]);
        }

        // create panels for the other player properties (sourceId)
        idOfOwnedProp = -1;
        for (int i=0; i< sourcePropertiesLength; i++)
        {
            idOfOwnedProp++;
            GameObject panel = createIndividualPanel(tradePanel, 1, idOfOwnedProp, sourceProperties[i]);
        }
        
        // the button
        tradePanel.transform.GetChild(2).transform.GetChild(0).GetComponent<Text>().text = "Accept";
        tradePanel.transform.GetChild(2).GetComponent<Button>().onClick.AddListener(() => 
                    executeTrade(tradePanel, sourceId, destinationId, sourceProperties, sourcePropertiesLength, destinationProperties, destinationPropertiesLength));
        tradePanel.transform.GetChild(3).GetComponent<Button>().onClick.AddListener(() =>
                    refuseTrade(tradePanel,sourceId)); 
    }

    void refuseTrade(GameObject tradePanel, int sourceId)
    {
        CmdRefusedTrade(sourceId);
        destroyGameObject(tradePanel);
    }

    [Command]
    void CmdRefusedTrade(int sourceId)
    {
        gameManagerScript.CmdRefusedTrade(sourceId);
    }

    void destroyGameObject(GameObject go)
    {
        Destroy(go);
    }

    GameObject createIndividualPanel(GameObject tradePanel, int childId, int idOfOwnedProp, int i)
    {
        GameObject panel = Instantiate(gameManagerScript.propertyTradePanelPrefab);
        panel.transform.SetParent(tradePanel.transform.GetChild(childId));
        panel.transform.localPosition = new Vector3(-73 + (50 * (int)(idOfOwnedProp % 4)), 135 - (50 * (int)(idOfOwnedProp / 4)), 0);
        if (i < 22) // the card is a property
        {
            panel.transform.GetChild(0).GetComponent<Text>().text = CardReader.propertyCards[i].cardName[0].ToString();
            panel.transform.GetComponent<Image>().color = new Color32((byte)CardReader.propertyCards[i].cardColor[0],
                (byte)CardReader.propertyCards[i].cardColor[1], (byte)CardReader.propertyCards[i].cardColor[2], 255);
        }
        else if (i < 26) // it's a railroad
        {
            panel.transform.GetChild(0).GetComponent<Text>().text = CardReader.railroadCards[i - 22].cardName[0].ToString();
        }
        else // it's a utility
        {
            panel.transform.GetChild(0).GetComponent<Text>().text = CardReader.utilityCards[i - 26].cardName[0].ToString();
        }
        return panel;
    }

    void createPanelsForPlayerProperties(GameObject tradePanel, int childId, int id, List <int> list)
    {
        int idOfOwnedProp = -1;
        for (int i = 0; i < gameManagerScript.cardsOwner.Count; i++)
            if (gameManagerScript.cardsOwner[i] == id)
            {
                idOfOwnedProp++;
                GameObject panel = createIndividualPanel(tradePanel, childId, idOfOwnedProp, i);
                int index = i;
                panel.GetComponent<Button>().onClick.AddListener(() => addToList(panel, index, list));
            }
    }

    void addToList(GameObject panel, int index, List<int> list)
    {
        list.Add(index);
        panel.transform.GetChild(0).GetComponent<Text>().color = Color.green;
        panel.GetComponent<Button>().onClick.RemoveAllListeners();
        panel.GetComponent<Button>().onClick.AddListener(() => removeFromList(panel, index, list));
    }
    
    void removeFromList(GameObject panel, int index, List<int> list)
    {
        list.Remove(index);
        panel.transform.GetChild(0).GetComponent<Text>().color = Color.white;
        panel.GetComponent<Button>().onClick.RemoveAllListeners();
        panel.GetComponent<Button>().onClick.AddListener(() => addToList(panel, index, list));
    }

    void sendTrade(GameObject tradePanel, int sourceId, int destinationId, List<int> sourcePropertiesList, List<int> destinationPropertiesList)
    {

        if (sourcePropertiesList.Count == 0 && destinationPropertiesList.Count == 0) return;

        Destroy(tradePanel);

        int[] sourceProperties = sourcePropertiesList.ToArray();
        int[] destinationProperties = destinationPropertiesList.ToArray();
        int sourcePropertiesLength = sourcePropertiesList.Count, destinationPropertiesLength = destinationPropertiesList.Count;
               
        Debug.Log("Send: ");
        for (int k = 0; k < sourcePropertiesLength; k++)
        {
            int id = sourceProperties[k];
            Debug.Log(id + " ");
        }

        Debug.Log("Receive: ");
        for (int k = 0; k < destinationPropertiesLength; k++)
        {
            int id = destinationProperties[k];
            Debug.Log(id + " ");
        }

        waitingForTrade = true;
        CmdSendTrade(idPlayer, destinationId, sourceProperties, sourcePropertiesLength, destinationProperties, destinationPropertiesLength);
    }

    [Command]
    void CmdSendTrade(int sourceId, int destinationId, int[] sourceProperties, int sourcePropertiesLength, int[] destinationProperties, int destinationPropertiesLength)
    {
        gameManagerScript.CmdSendTrade(idPlayer, destinationId, sourceProperties, sourcePropertiesLength, destinationProperties, destinationPropertiesLength);
    }
    
    void executeTrade(GameObject tradePanel, int sourceId, int destinationId, int[] sourceProperties, int sourcePropertiesLength, int[] destinationProperties, int destinationPropertiesLength)
    {
        Destroy(tradePanel);

        Debug.Log("Trade accepted.");
        Debug.Log("Send: ");
        foreach (int id in sourceProperties)
            Debug.Log(id + " ");
        Debug.Log("Receive: ");
        foreach (int id in destinationProperties)
            Debug.Log(id + " ");

        CmdExecuteTrade(sourceId, destinationId, sourceProperties, sourcePropertiesLength, destinationProperties, destinationPropertiesLength);
    }
    
    [Command]
    void CmdExecuteTrade(int sourceId, int destinationId, int[] sourceProperties, int sourcePropertiesLength, int[] destinationProperties, int destinationPropertiesLength)
    {
        gameManagerScript.CmdExecuteTrade(sourceId, destinationId, sourceProperties, sourcePropertiesLength, destinationProperties, destinationPropertiesLength);
    }

    [ClientRpc]
    public void RpcAcceptedTrade()
    {
        if (!isLocalPlayer) return;
        waitingForTrade = false;
    }
    
    [ClientRpc]
    public void RpcRefusedTrade()
    {
        if (!isLocalPlayer) return;
        waitingForTrade = false;
    }

    // ----------------- BUILDINGS ----------------

     [ClientRpc]
    public void RpcConstructHouse(GameObject housePrefab, Vector3 position)
    {
        Instantiate(housePrefab, position, Quaternion.identity);
    }

    [Command]
    public void CmdConstructHouse(string housePrefabPath, Vector3 vector3, int cardIndex)
    {
        gameManagerScript.CmdConstructHouse(housePrefabPath, vector3, cardIndex);
    }

    [Command]
    public void CmdConstructHotel(string housePrefabPath, Vector3 vector3, int cardIndex)
    {
        gameManagerScript.CmdConstructHotel(housePrefabPath, vector3, cardIndex);
    }

    [Command]
    public void CmdDeconstructHouse(int cardIndex)
    {
        gameManagerScript.CmdDeconstructHouse(cardIndex);
    }

    [Command]
    public void CmdDeconstructHotel(int cardIndex)
    {
        gameManagerScript.CmdDeconstructHotel(cardIndex);
    }

    // -----------------------

    [ClientRpc]
    public void RpcChangeColorOnPanel(GameObject playerInfo, byte r, byte g, byte b, byte a)
    {
        playerInfo.transform.GetComponent<Image>().color = new Color32(r, g, b, a);
    }

    [ClientRpc]
    public void RpcChangeOwnerPanelColor(int newOwnerId, int id, Color ownerColor)
    {
        GameObject panel = GameObject.FindGameObjectWithTag("id" + id);

        if (newOwnerId != -1)
        {
            panel.GetComponent<Renderer>().enabled = true;
            panel.GetComponent<Renderer>().material.color = ownerColor;
        }
        else
            panel.GetComponent<Renderer>().enabled = false;
    }

    [Command]
    public void CmdChangeOwner(int newOwnerId, int cardIndex, int id)
    {
        gameManagerScript.CmdChangeOwner(cardIndex, newOwnerId, id);
    }

    [Command]
    public void CmdGiveMoneyToPlayer(int ownerId, int amountPaid)
    {
        gameManagerScript.CmdGiveMoneyToPlayer(ownerId, amountPaid);
    }

    [ClientRpc]
    public void RpcChangeMoneyOnPanel(GameObject playerInfo)
    {
        playerInfo.transform.GetChild(0).GetComponent<Text>().text = "Player " + idPlayer + "\n$" + money;
    }

    [Command]
    public void CmdChangeCardJailOwner(int ownerId, string type)
    {
        if (ownerId == 0)
            gameManagerScript.CmdChangeCardJailOwner(idPlayer, type);
        else
            gameManagerScript.CmdChangeCardJailOwner(-1, type);
    }

    [Command]
    public void CmdAddMoney(int amount)
    {
        money += amount;
        //gameManagerScript.CmdChangeMoneyOnPanel(idPlayer, money);
    }

    [Command]
    public void CmdTakeMoney(int amount)
    {
        Debug.Log("Took $" + amount);
        money -= amount;
        //gameManagerScript.CmdChangeMoneyOnPanel(idPlayer, money);
    }

    [Command]
    public void CmdMovePlayer(int newIndex)
    {
        indexPosition = (newIndex) % 40;
    }

    [Command]
    public void CmdSetDiceInactive()
    {
        diceScript.CmdSetDiceInactive();
    }

    [Command]
    public void CmdNextPlayer()
    {
        gameManagerScript.CmdNextPlayer();
    }

    [Command]
    public void CmdAddConnectedPlayer(GameObject newPlayer)
    {
        // To be able to call the function on something without authority, I will run the call of the function on the server
        gameManagerScript.CmdAddConnectedPlayer(this.gameObject);
    }
}
