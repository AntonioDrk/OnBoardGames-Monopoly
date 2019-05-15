using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

[Serializable]
public class UtilityCard : Card
{
    public int id;
    public string cardName;
    public int[] rentMultiplier = { 4, 10 };

    public void UtilityCardConstructor()
    {
        Id = id;
        CardName = cardName;
        Price = 150;
        Mortgage = 75;
        OwnerId = -1;
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public override string ToString()
    {
        return Id.ToString() + ": " + cardName.ToString() + " " +
            Price.ToString() + " " + Mortgage.ToString();
    }

    public override void doAction(int playerId)
    {
        if (OwnerId > 0)
        {
            if (OwnerId != playerId)
            {
                Debug.Log("Player must pay rent.");
                //Player.money -= DiceScript.rolledNumber * rentMultiplier[Owner.utilitiesOwned];
                //Debug.Log("Player + " Player.idPlayer + " paid $" + rent + " to player " + OwnerId);
            }
        }
        else
        {
            Debug.Log("Player must buy this property.");
            // TO-DO: print card on the screen and buttons to buy the property or auction it
        }
    }
}
