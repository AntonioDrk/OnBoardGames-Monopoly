using System.Collections;
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
    private GameObject rollButton;
    private Text playerMoneyText, idText;

    [SyncVar] private Color plyColor;

    private Renderer renderer;

    [SerializeField][SyncVar]
    private int money = 1500;
    private int doublesRolled = 0;
    private int roundsInJail = 0;
    private bool inJail = false;

    void Start()
    {
        rollButton = GameObject.Find("RollDice");
        gameManager = GameObject.Find("GameManager");
        gameManagerScript = gameManager.GetComponent<GameManager>();
        diceManager = GameObject.Find("DiceManager");
        diceScript = diceManager.GetComponent<DiceScript>();
        anim = GetComponent<Animator>();
        renderer = transform.GetChild(0).GetComponent<Renderer>();

        if (isLocalPlayer)
        {
            rollButton.SetActive(false);
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
        if (gameManagerScript.playerTurn == idPlayer)
        {
            if (rollButton.activeInHierarchy == false)
            {
                rollButton.SetActive(true);
                rollButton.GetComponent<Button>().onClick.AddListener(RollTheDice);
            }
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
                    nextPlayer();
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
        else if(diceScript.isDouble)
            CmdSetDiceInactive();
        else
            nextPlayer();
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


    // This function is to make the link between the button on click event and sending a command
    void RollTheDice()
    {
        Debug.Log("You have clicked the button!");
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
        CmdSetDiceInactive();
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

    [ClientRpc]
    public void RpcUpdateColor(Color col)
    {
        renderer.material.color = col;
        plyColor = col;
    }

    [Command]
    public void CmdAddMoney(int amount)
    {
        money += amount;
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
