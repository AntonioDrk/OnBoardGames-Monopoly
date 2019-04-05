using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

public class Player : NetworkBehaviour
{ 
     
    public int idPlayer = 0;

    // Serialized just for testing purposes, remove me 
    [SerializeField]
    private int indexPosition; // Indicates the position on the board list (the list of cards that are on the board)
    [SerializeField]
    private GameObject dice, gameManager;

    private Animator anim; 
    private DiceScript diceScript;
    private GameManager gameManagerScript;
    private Vector3 goPosition = new Vector3(2.5f, 0.125f, -6.49f);
    private Vector3 jailPosition = new Vector3(-11f, 0.125f, -6f);
    private Vector3 justVisitingPosition = new Vector3(-11.45854f, 0.125f, -6.49f);
    private GameObject rollButton;
    private Text playerMoneyText;

    private int money = 1500;
    private int doublesRolled = 0;
    private int roundsInJail = 0;
    private bool inJail = false;


    void Start()
    {
        playerMoneyText = GameObject.Find("playerMoneyText").GetComponent<Text>();
        rollButton = GameObject.Find("RollDice");
        gameManager = GameObject.Find("GameManager");
        gameManagerScript = gameManager.GetComponent<GameManager>();
        dice = GameObject.Find("Dice");
        diceScript = dice.GetComponent<DiceScript>();
        anim = GetComponent<Animator>();

        if (isLocalPlayer)
        {
            playerMoneyText.text = "$1500";
            idPlayer = gameManagerScript.connectedPlayers + 1;
            CmdAddPlayer();
            Debug.Log("id: " + idPlayer); 
            GameObject.Find("idText").GetComponent<Text>().text = "id: " + idPlayer;
            transform.position = goPosition;
        }
           
    }
     

    void Update()
    { 
        // exit from update if this is not the local player
        if (!isLocalPlayer)
             return;

        if (gameManagerScript.playerTurn == idPlayer)
            rollButton.SetActive(true);
        else
            rollButton.SetActive(false);


        // if the dice rolled and it's player's turn
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

                if(diceScript.isDouble || roundsInJail == 3)
                {
                    inJail = false;
                    roundsInJail = 0;
                    doublesRolled = 0;
                    diceScript.isDouble = false;
                    transform.position = justVisitingPosition;
                    if (!diceScript.isDouble)
                        money -= 50; 
                }

                if (roundsInJail < 3 && inJail)
                    nextPlayer(); 
            }
             
            if(doublesRolled == 3)
            {
                goToJail();
            }
            else if(!inJail)
                StartCoroutine("animateMovement", diceScript.rolledNumber);

            diceScript.rolledNumber = 0;
        }


        playerMoneyText.text = "$" + money;
        Debug.Log("Doubles rolled: " + doublesRolled);
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
                indexPosition = (indexPosition + 1) % 40;
                yield return new WaitForSeconds(anim.GetCurrentAnimatorStateInfo(0).length);

                if (indexPosition == 0)
                {
                    transform.position = goPosition;
                    money += 200;
                    Debug.Log(money);
                }

                if (indexPosition % 10 == 0)
                {
                    transform.eulerAngles += new Vector3(0, 90, 0); 
                }

            }
            else
            {
                anim.Play("StraightMovement", 0);
                indexPosition = (indexPosition + 1) % 40;
                yield return new WaitForSeconds(anim.GetCurrentAnimatorStateInfo(0).length);

            }
        }


        gameManagerScript.targetPlayerIsMoving = false;
        if (indexPosition == 30)
        {
            goToJail();
        }
        else if(diceScript.isDouble)
            diceScript.setDiceInactive();
        else
            nextPlayer();
    }

    [Command]
    public void CmdNextPlayer()
    {
        gameManagerScript.playerTurn++;
        if (gameManagerScript.playerTurn > gameManagerScript.connectedPlayers) gameManagerScript.playerTurn = 1;
        Debug.Log("playerTurn: " + gameManagerScript.playerTurn);
        //gameManagerScript.playerTurnText.text = "Turn: Player " + gameManagerScript.playerTurn;
    }

    [Command]
    public void CmdAddPlayer()
    {
        gameManagerScript.connectedPlayers++;
        Debug.Log("connectedPlayers: " + gameManagerScript.connectedPlayers);
        //gameManagerScript.connectedPlayersText.text = gameManagerScript.connectedPlayers + " players";
    }

    void nextPlayer()
    {
        diceScript.setDiceInactive();
        CmdNextPlayer();
    }

    void goToJail()
    {
        indexPosition = 10;
        doublesRolled = 0;
        roundsInJail = 0;
        inJail = true;
        transform.position = jailPosition;
        transform.eulerAngles = new Vector3(0, 90, 0);
        nextPlayer();
    }
}
