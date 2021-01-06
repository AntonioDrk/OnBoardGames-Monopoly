using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.Networking;

//[Serializable]
public abstract class Card
{
    private int id;
    private string cardName;
    private int price;
    private int mortgage; 

    public int Id { get => id; set => id = value; }
    public string CardName { get => cardName; set => cardName = value; }
    public int Price { get => price; set => price = value; }
    public int Mortgage { get => mortgage; set => mortgage = value; }

    public abstract void doAction(GameObject player);
    public abstract void showOwnedCard(GameObject player);

    protected virtual void hideCard(GameObject player)
    {
        // Stuff to be called at the end
        player.GetComponent<Player>().ViewingCard = false;
    }
}
