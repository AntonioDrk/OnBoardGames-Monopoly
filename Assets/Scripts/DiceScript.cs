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

    public void addDice(int rolledValue)
    {
        diceCounter++;
        rolledNumber += rolledValue;
        //Debug.LogError("Dice counter: " + diceCounter);// + ". Dice value: " + rolledValue);
        if (diceCounter == 2)
        {
            //Debug.LogError("Dice counter: " + diceCounter);
            if (rolledValue == rolledNumber - rolledValue)
                isDouble = true;
            else
                isDouble = false;

            //rolledNumber = 2;
            gameManager.GetComponent<GameManager>().currentRolledNumber = rolledNumber;
            rolled = true;
        }
    }

    // This is already called in a command on the server
    public void CmdRollDice()
    {
        // Make sure this is run only on the server to not fuck up something
        if (!isServer) return;

        Debug.Log("(ServerSide) Entered CmdRollDice");

        if (transform.childCount > 0)
        {
            return;
        }

        GameObject go = Instantiate(dice);
        dice.transform.GetChild(0).transform.eulerAngles = new Vector3(90 * Random.Range(0, 4), 90 * Random.Range(0, 4), 90 * Random.Range(0, 4));
        dice.transform.GetChild(1).transform.eulerAngles = new Vector3(90 * Random.Range(0, 4), 90 * Random.Range(0, 4), 90 * Random.Range(0, 4));
        //go.transform.parent = diceScript.transform;
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
