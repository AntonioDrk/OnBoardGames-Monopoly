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
    private GameObject rollButton;

    void Start()
    { 

        rollButton = GameObject.Find("RollDice");
        gameManager = GameObject.Find("GameManager");
        gameManagerScript = gameManager.GetComponent<GameManager>();
        dice = GameObject.Find("Dice");
        diceScript = dice.GetComponent<DiceScript>();
        anim = GetComponent<Animator>();

        if (isLocalPlayer)
        {
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
            StartCoroutine("animateMovement", diceScript.rolledNumber);
            diceScript.rolledNumber = 0;
        }
        
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
        CmdNextPlayer();
        diceScript.setDiceInactive();

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
}
