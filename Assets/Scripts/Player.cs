using System.Collections;
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
    public GameObject ownedPropertyPanelPrefab;// playerInfoPrefab;
    private Text playerMoneyText, idText;

    [SyncVar] private Color plyColor;
    [SyncVar] public int chestJailCardOwner;
    [SyncVar] public int chanceJailCardOwner;

    private Renderer renderer;
    [SyncVar] private int myMeshIndex;

    [SerializeField] [SyncVar] private int money = 1500;
    private int doublesRolled = 0;
    private int roundsInJail = 0;
    private bool inJail = false;
    [SerializeField] private int stage = 0;

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

            CmdAddConnectedPlayer(this.gameObject);
            idText = GameObject.Find("idText").GetComponent<Text>();
            playerMoneyText = GameObject.Find("playerMoneyText").GetComponent<Text>();
            transform.position = goPosition;
        }
        
    }

    void Update()
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
        if((isServer && gameManagerScript.gameStarted) || !isServer)
        if (gameManagerScript.playerTurn == idPlayer && stage == 0)
        {
            rollButton.SetActive(true);
        }
        else
            rollButton.SetActive(false);


        // if the diceManager rolled and it's player's turn 
        if (diceScript.rolled == true && gameManagerScript.playerTurn == idPlayer)
        {
            diceScript.rolled = false;
            Debug.Log("Player " + idPlayer + " rolled " + diceScript.rolledNumber);

            if (diceScript.isDouble)
                doublesRolled++;
            else
                doublesRolled = 0;

            if (inJail)
            {
                roundsInJail += 1;

                if (diceScript.isDouble || roundsInJail == 3)
                {
                    inJail = false;
                    if (!diceScript.isDouble)
                        CmdAddMoney(-50);
                    else
                        CmdAddMoney(-25);
                    roundsInJail = 0;
                    doublesRolled = 0;
                    diceScript.isDouble = false;
                    transform.position = justVisitingPosition; 
                }

                if (roundsInJail < 3 && inJail)
                {
                    CmdSetDiceInactive();
                    endTurn();
                }
            }

            if (doublesRolled == 3)
            {
                goToJail();
            }
            else if (!inJail)
                moveSpaces(diceScript.rolledNumber);

            diceScript.rolledNumber = 0;
        }

        playerMoneyText.text = "$" + money;

    }

    public void moveSpaces(int amount)
    {
        StartCoroutine("animateMovement", amount);
    }

    IEnumerator animateMovement(int amountToMove)
    {
        gameManagerScript.targetPlayerIsMoving = true;
        gameManagerScript.targetPlayer = this.gameObject;

        for (int i = 0; i < amountToMove; i++)
        {
            if (indexPosition % 10 == 9 || indexPosition % 10 == 0)
            {
                anim.Play("StraightMovementToCorner", 0);
                CmdMovePlayer();

                yield return new WaitForSeconds(anim.GetCurrentAnimatorStateInfo(0).length);

                if (indexPosition == 0)
                {
                    transform.position = goPosition;
                    CmdAddMoney(200);
                    Debug.Log("Money " + money);
                }

                if (indexPosition % 10 == 0)
                {
                    transform.eulerAngles += new Vector3(0, 90, 0);
                }

            }
            else
            {
                anim.Play("StraightMovement", 0);
                CmdMovePlayer();
                yield return new WaitForSeconds(anim.GetCurrentAnimatorStateInfo(0).length);

            }
        }

        gameManagerScript.targetPlayerIsMoving = false;
        if (indexPosition == 30)
        {
            goToJail();
        }
        else
        {
            stage = 2;
            if(diceManager.transform.childCount > 0)
                CmdSetDiceInactive();

            if(indexPosition == 12 || indexPosition == 28) // Utilities
            {
                int utilityIndex = (indexPosition - 12) / 16;
                //Debug.Log(CardReader.utilityCards[utilityIndex].ToString());
                CardReader.utilityCards[utilityIndex].doAction(this.gameObject);
            }
            else if(indexPosition == 5 || indexPosition == 15 || indexPosition == 25 || indexPosition == 35) // Railroads
            {
                int railroadIndex = (indexPosition - 5) / 10;
                //Debug.Log(CardReader.railroadCards[railroadIndex].ToString());
                CardReader.railroadCards[railroadIndex].doAction(this.gameObject);
            }
            else if(indexPosition == 4) // Income Tax - Pay $200
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
                if (chestJailCardOwner == -1)
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
                if (chanceJailCardOwner == -1)
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
    public int getStage() { return stage;  }
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

    public void buyProperty(Card propertyCard)
    {
        Debug.Log("Bought " + propertyCard.CardName);

        int numberOfOwnedCards = ownedPropertyCards.Count;
        //Debug.Log("Nr of owned cards: " + numberOfOwnedCards);

        GameObject ownedPropertyPanel = Instantiate(ownedPropertyPanelPrefab);

        if(propertyCard.GetType() == typeof(PropertyCard))
            ownedPropertyPanel.transform.GetComponent<Image>().color = new Color32((byte)((PropertyCard)propertyCard).cardColor[0], 
                (byte)((PropertyCard)propertyCard).cardColor[1], (byte)((PropertyCard)propertyCard).cardColor[2], 255);

        ownedPropertyPanel.transform.SetParent(ownedPropertiesPanel.transform);
        ownedPropertyPanel.transform.position = new Vector3(0, 0, 0);
        ownedPropertyPanel.GetComponent<RectTransform>().localScale = new Vector3(1,1,1);
        ownedPropertyPanel.transform.GetChild(0).GetComponent<Text>().text = propertyCard.CardName;
        ownedPropertyPanel.GetComponent<Button>().onClick.AddListener(() => propertyCard.showOwnedCard(this.gameObject));        
        //Debug.LogError("Owned Property Panel List Count: " + ownedPropertyList.Count);
        
        // find the new position in the list
        int newIndex = 0;
        if(numberOfOwnedCards > 0)
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
            for(int k=newIndex+1; k < numberOfOwnedCards+1; k++)
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
        for(int k = cardIndex + 1; k < ownedPropertyList.Count; k++)
        {
            changePositionOfPanel(ownedPropertyList[k], k - 1);
        }

        Destroy(ownedPropertyList[cardIndex]);
        ownedPropertyList.RemoveAt(cardIndex);
        
    }

    // This function is to make the link between the button on click event and sending a command
    void RollTheDice()
    {
        stage = 1;
        rollButton.SetActive(false);
        diceScript.diceCounter = 0;
        //Debug.LogError("Dice counter becomes 0.");
        diceScript.rolledNumber = 0;
        CmdRollDice();
    }

    // Tell the server to roll the dices from the Dice Manager 
    [Command]
    public void CmdRollDice()
    {
        diceScript.CmdRollDice();
    }

    public void goToJail()
    {
        indexPosition = 10;
        doublesRolled = 0;
        roundsInJail = 0;
        inJail = true;
        transform.position = jailPosition;
        transform.eulerAngles = new Vector3(0, 90, 0);

        if (diceManager.transform.childCount > 0)
            CmdSetDiceInactive();

        endTurn();
    }

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
    public void RpcChangeColorOnPanel(GameObject playerInfo,byte r,byte g,byte b,byte a)
    {
        playerInfo.transform.GetComponent<Image>().color = new Color32(r,g,b,a);
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
    public void CmdMovePlayer()
    {
        indexPosition = (indexPosition + 1) % 40;
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
