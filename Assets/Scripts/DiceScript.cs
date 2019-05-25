using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class DiceScript : NetworkBehaviour
{
    public int rolledNumber = 0; 
    public bool isDouble = false;
    public bool rolled = false;

    public int diceCounter = 0;
    public GameObject dice;
    public GameManager gameManager;
     
    void Start()
    {
           diceCounter = 100;
           gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
    } 

    [Command]
    public void CmdAddDice(int rolledValue)
    {
        diceCounter++;
        rolledNumber += rolledValue;
        if (diceCounter == 2)
        {
            gameManager.players[gameManager.playerTurn].GetComponent<Player>().RpcMovePlayer(rolledNumber,rolledValue);
        }
    }

    // This is already called in a command on the server
    public void CmdRollDice()
    {
        // Make sure this is run only on the server to not fuck up something
        if (!isServer) return;

        if (transform.childCount > 0)
        {
            return;
        }

        diceCounter = 0;
        rolledNumber = 0;

        GameObject go = Instantiate(dice);
        dice.transform.GetChild(0).transform.eulerAngles = new Vector3(90 * Random.Range(0, 4), 90 * Random.Range(0, 4), 90 * Random.Range(0, 4));
        dice.transform.GetChild(1).transform.eulerAngles = new Vector3(90 * Random.Range(0, 4), 90 * Random.Range(0, 4), 90 * Random.Range(0, 4));
        NetworkServer.Spawn(go);
    }

    // This is already called in a command on the server
    public void CmdSetDiceInactive()
    {
        // Make sure this is run only on the server to not fuck up something
        if (!isServer) return;
        NetworkServer.Destroy(transform.GetChild(0).gameObject);
    }
}
