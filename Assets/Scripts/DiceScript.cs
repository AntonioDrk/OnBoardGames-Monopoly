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
        //Debug.Log("Dice counter: " + diceCounter);
        diceCounter++;
        rolledNumber += rolledValue;
        if (diceCounter == 2)
        {
            if (rolledValue == rolledNumber - rolledValue)
                isDouble = true;
            else
                isDouble = false;

            rolled = true;
        }
    }
    /*
    public bool areDiceActive()
    {
        if (gameObject.transform.GetChild(0).gameObject.activeInHierarchy == true)
            return true;
        return false;
    }
    */
}
