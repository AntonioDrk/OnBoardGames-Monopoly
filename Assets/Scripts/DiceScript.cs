using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class DiceScript : MonoBehaviour
{
    public int rolledNumber = 0; 
    public bool rolled = false;

    public int diceCounter = 0;
     

    // Start is called before the first frame update
    void Start()
    { 

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void addDice(int rolledValue)
    {
        diceCounter++;
        rolledNumber += rolledValue;
        if (diceCounter == 2)
            rolled = true;
    }

    
    public void CmdRollDice()
    {
        // if it's player's turn and the dice are not rolling  
        if(!areDiceActive())
        {
            diceCounter = 0;
            gameObject.transform.GetChild(0).gameObject.SetActive(true);
            gameObject.transform.GetChild(1).gameObject.SetActive(true);
        }
    }

    public bool areDiceActive()
    {
        if (gameObject.transform.GetChild(0).gameObject.activeInHierarchy == true)
            return true;
        return false;
    }

    public void setDiceInactive()
    {
        gameObject.transform.GetChild(0).gameObject.SetActive(false);
        gameObject.transform.GetChild(1).gameObject.SetActive(false); 
    }
}
