using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Card : MonoBehaviour
{
    private string cardName;
    private int price;
    private int mortgage;
    private int ownerId;

    public string CardName { get => cardName; set => cardName = value; }
    public int Price { get => price; set => price = value; }
    public int Mortgage { get => mortgage; set => mortgage = value; }
    public int OwnerId { get => ownerId; set => ownerId = value; }

    public abstract void doAction();
}
