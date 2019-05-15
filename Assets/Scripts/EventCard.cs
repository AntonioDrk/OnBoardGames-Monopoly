using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

[Serializable]
public class EventCard : Card
{
    public int id;
    public int[] cardColor = new int[3];
    public string cardName;
    public string description;

    public void EventCardConstructor()
    {
        Id = id;
        CardName = cardName;
        Price = 0;
        Mortgage = 0;
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
        return Id.ToString() + ": " + cardName.ToString() + " " + description.ToString();
    }

    public override void doAction(int playerId)
    {

    }

    public void takeMoneyFromPlayer(int playerId, int value)
    {

    }

    public void giveMoneyToPlayer(int playerId, int value)
    {

    }

    public void sendPlayerToJail(int playerId)
    {

    }

    public void sendPlayerToGo(int playerId)
    {

    }

}
