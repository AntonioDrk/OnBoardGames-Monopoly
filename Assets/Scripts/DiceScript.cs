using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class DiceScript : NetworkBehaviour
{
    public int rolledNumber = 0; 
    public bool rolled = false;
    public bool isDouble = false;

    public int diceCounter = 0;
    public GameObject dice;
     

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
        {
            if (rolledValue == rolledNumber - rolledValue)
                isDouble = true;
            else
                isDouble = false;
            rolled = true;
        }
    }

    
    public void CmdRollDice()
    {  
        if(transform.childCount > 0)
        {
            Destroy(transform.GetChild(0));
        }

        diceCounter = 0;
        GameObject go = Instantiate(dice, dice.transform.position, Random.rotation);
        go.transform.parent = transform;
        NetworkServer.Spawn(go);
        //dice.transform.GetChild(0).gameObject.SetActive(true);
        //dice.transform.GetChild(1).gameObject.SetActive(true);

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
