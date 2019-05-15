﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

public class Player : NetworkBehaviour
{
    [SyncVar] public int idPlayer = 0;

    // Serialized just for testing purposes, remove me 
    [SerializeField][SyncVar]
    private int indexPosition = 0; // Indicates the position on the board list (the list of cards that are on the board)
    [SerializeField]
    private GameObject diceManager, gameManager;

    private Animator anim;
    private DiceScript diceScript;
    private GameManager gameManagerScript;
    private Vector3 goPosition = new Vector3(2.5f, 0.125f, -6.49f);
    private Vector3 jailPosition = new Vector3(-11f, 0.125f, -6f);
    private Vector3 justVisitingPosition = new Vector3(-11.45854f, 0.125f, -6.49f);
    private GameObject rollButton, endTurnButton, cardPanel, buyPropertyButton;
    private Text playerMoneyText, idText;

    [SyncVar] private Color plyColor;

    private Renderer renderer;
    [SyncVar]
    private int myMeshIndex;

    [SerializeField][SyncVar] private int money = 1500;
    private int doublesRolled = 0;
    private int roundsInJail = 0;
    private bool inJail = false;
    [SerializeField] private int stage = -1; // -1 = it's your turn, 0 = you rolled the dice

    [SerializeField] private List<PropertyCard> ownedPropertyCards;

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
            ownedPropertyCards = new List<PropertyCard>();
            rollButton = GameObject.Find("RollDice");
            rollButton.GetComponent<Button>().onClick.AddListener(RollTheDice);
            endTurnButton = GameObject.Find("EndTurn");
            endTurnButton.GetComponent<Button>().onClick.AddListener(nextPlayer);
            rollButton.SetActive(false);
            endTurnButton.SetActive(false);
            cardPanel = GameObject.Find("Card");
            cardPanel.SetActive(false);

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
        if (gameManagerScript.playerTurn == idPlayer && stage == -1)
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
                    roundsInJail = 0;
                    doublesRolled = 0;
                    diceScript.isDouble = false;
                    transform.position = justVisitingPosition;
                    if (!diceScript.isDouble)
                        CmdAddMoney(-50);
                }

                if (roundsInJail < 3 && inJail)
                    endTurn();
            }

            if (doublesRolled == 3)
            {
                goToJail();
            }
            else if (!inJail)
                StartCoroutine("animateMovement", diceScript.rolledNumber);

            diceScript.rolledNumber = 0;
        }


        playerMoneyText.text = "$" + money;
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
        else if (diceScript.isDouble)
        {
            CmdSetDiceInactive();
            stage = -1;
            int cardIndex = CardReader.getPropertyCardIndex(indexPosition);
            Debug.Log("Card index: " + cardIndex);
            if (cardIndex != -1) // if it's a property card
            {
                showCard(cardIndex);
            }
        }
        else
            endTurn();
    }

    public Renderer getRenderer()
    {
        return renderer;
    }

    public void setPlyColor(Color value)
    {
        plyColor = value;
    }

    public Color getPlyColor()
    {
        return plyColor;
    }

    public int getMyMeshIndex(){ return myMeshIndex; }
    public void setMyMeshIndex(int value) { myMeshIndex = value; }
    public int getIndexPosition() { return indexPosition; }

    void endTurn()
    {
        int cardIndex = CardReader.getPropertyCardIndex(indexPosition);
        Debug.Log("Card index: " + cardIndex);
        if (cardIndex != -1) // if it's a property card
        {
            showCard(cardIndex);
        }
        Debug.Log("Button active");
        endTurnButton.SetActive(true);
    }

    void buyProperty(int cardIndex)
    {
        ownedPropertyCards.Add(CardReader.propertyCards[cardIndex]);
    }

    void showCard(int cardIndex)
    {
        cardPanel.SetActive(true);
        Debug.Log(CardReader.propertyCards[cardIndex].ToString());

        buyPropertyButton = GameObject.Find("BuyProperty");
        buyPropertyButton.GetComponent<Button>().onClick.AddListener(() => buyProperty(cardIndex));

        cardPanel.transform.GetChild(0).transform.GetComponent<Image>().color
            = new Color32((byte)CardReader.propertyCards[cardIndex].cardColor[0], (byte)CardReader.propertyCards[cardIndex].cardColor[1], (byte)CardReader.propertyCards[cardIndex].cardColor[2], 255);
        cardPanel.transform.GetChild(0).transform.GetChild(0).GetComponent<Text>().text = CardReader.propertyCards[cardIndex].CardName;
        cardPanel.transform.GetChild(1).GetComponent<Text>().text = "RENT $" + CardReader.propertyCards[cardIndex].rent[0].ToString();

        cardPanel.transform.GetChild(3).GetComponent<Text>().text =
            "$" + CardReader.propertyCards[cardIndex].rent[1].ToString() + '\n' +
            "$" + CardReader.propertyCards[cardIndex].rent[2].ToString() + '\n' +
            "$" + CardReader.propertyCards[cardIndex].rent[3].ToString() + '\n' +
            "$" + CardReader.propertyCards[cardIndex].rent[4].ToString() + '\n';

        cardPanel.transform.GetChild(4).GetComponent<Text>().text =
            "With HOTEL $" + CardReader.propertyCards[cardIndex].rent[5].ToString() + '\n' +
            "Mortgage Value $" + CardReader.propertyCards[cardIndex].mortgageValue.ToString() + '\n' +
            "Houses cost $" + CardReader.propertyCards[cardIndex].pricePerHouse.ToString() + " each\n" +
            "Hotels, $" + CardReader.propertyCards[cardIndex].pricePerHouse.ToString() + " plus 4 houses";
    }

    // This function is to make the link between the button on click event and sending a command
    void RollTheDice()
    {
        //Debug.Log("You have clicked the button!");
        stage = 0;
        cardPanel.SetActive(false);
        rollButton.SetActive(false);
        diceScript.diceCounter = 0;
        diceScript.rolledNumber = 0;
        CmdRollDice();
    }

    // Tell the server to roll the dices from the Dice Manager 
    [Command]
    public void CmdRollDice()
    {
        diceScript.CmdRollDice();
    }

    void nextPlayer()
    {
        cardPanel.SetActive(false);
        endTurnButton.SetActive(false);
        stage = -1;
        CmdSetDiceInactive();
        CmdNextPlayer();
    }

    public void goToJail()
    {
        indexPosition = 10;
        doublesRolled = 0;
        roundsInJail = 0;
        inJail = true;
        transform.position = jailPosition;
        transform.eulerAngles = new Vector3(0, 90, 0);
        endTurn();
    }

    public void moveSpaces(int amount)
    {
        StartCoroutine("animateMovement", amount);
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

    [Command]
    public void CmdAddMoney(int amount)
    {
        money += amount;
    }

    [Command]
    public void CmdTakeMoney(int amount)
    {
        money -= amount;
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
