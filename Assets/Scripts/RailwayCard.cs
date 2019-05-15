using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

[Serializable]
public class RailwayCard  : Card
{
    public int id;
    public string cardName;
    public int[] rent = { 25, 50, 100, 200 };

    public void RailwayCardConstructor()
    {
        Id = id;
        CardName = cardName;
        Price = 200;
        Mortgage = 100;
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
        return "Id: " + Id.ToString() +
               "\nName: " + cardName.ToString() +
               "\nPrice: " + Price.ToString() +
               "\nMortgage: " + Mortgage.ToString();
    }

    public override void doAction(GameObject player)
    {
        if(OwnerId > 0)
        {
            /*if(OwnerId != playerId)
            {
                Debug.Log("Player must pay rent.");
                //Player.money -= rent[Owner.railwaysOwned];
                //Debug.Log("Player + " Player.idPlayer + " paid $" + rent[Owner.railwaysOwned] + " to player " + OwnerId);
            }*/
        }
        else
        {
            Debug.Log("Player must buy this property.");
            // TO-DO: print card on the screen and buttons to buy the property or auction it
        }
    }
}
