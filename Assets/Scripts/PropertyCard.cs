using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class PropertyCard : Card
{
    public int id;
    public int[] cardColor = new int[3];
    public string cardName;
    public int priceValue;
    public int mortgageValue;
    public int pricePerHouse;
    private int housesBuilt = 0;
    public int cardsInGroup = 2;
    private bool hasHotel = false;
    public int[] rent = new int[6];
    public int[] propertiesFromSameGroup;

    public void PropertyCardConstructor()
    {
        Id = id;
        CardName = cardName;
        Price = priceValue;
        Mortgage = mortgageValue;
        OwnerId = -1;
    }

    public override string ToString()
    {
        return "Id: " + Id.ToString() +
               "\nName: " + cardName.ToString() +
               "\nPrice: " + priceValue.ToString() +
               "\nMortgage: " + mortgageValue.ToString() +
               "\nHotel: " + hasHotel.ToString() +
               "\nHouses: " + housesBuilt.ToString();
    }

    public override void doAction(GameObject player)
    {
        showCard();
        if (OwnerId > 0)
        {
            /*if (OwnerId != playerId)
            {
                Debug.Log("Player must pay rent.");
                if (hasHotel)
                    Player.money -= rent[6];
                else
                    Player.money -= rent[housesBuilt];
                //Debug.Log("Player + " Player.idPlayer + " paid $" + rent + " to player " + OwnerId);
            }*/
        }
        else
        {
            Debug.Log("Player can buy this property.");
            CardReader.buyPropertyButton.SetActive(true);
            CardReader.buyPropertyButton.GetComponent<Button>().onClick.AddListener(() => buyProperty(player));
            CardReader.cancelButton.GetComponent<Button>().onClick.AddListener(() => hideCard(player));
            CardReader.cancelButton.SetActive(true);
        }

    }

    //  verifies if the all properties in the same colour group are equally developed
    public bool propertiesAreEquallyDeveloped()
    {
        // TO-DO: create array of properties to be able to check other properties
        /*if(cardsInGroup == 2)
        {
            int brotherCardId = propertiesFromSameGroup[0];
            if (Math.Abs(GameManager.properties[brotherCardId].housesBuilt - housesBuilt) > 1)
                return false;
        }
        else if(cardsInGroup == 3)
        {
            int brotherCardId = propertiesFromSameGroup[0];
            int sisterCardId = propertiesFromSameGroup[1];
            int brotherHouses = GameManager.properties[brotherCardId].housesBuilt;
            int sisterHouses = GameManager.properties[sisterCardId].housesBuilt;
            int min = Math.Min(Math.Min(brotherHouses, sisterHouses), housesBuilt);
            if (min != housesBuilt)
                return false;
        }*/

        return true;
    }

    public void buildHouse()
    {
        if (!hasHotel && housesBuilt <= 3 && propertiesAreEquallyDeveloped())
        {
            Debug.Log("Player built a house.");
            //Player.money -= pricePerHouse;
            housesBuilt += 1;
            // TO-DO: animation for house being built
        }
        else if (housesBuilt == 4)
            buildHotel();
    }

    public void buildHotel()
    {
        Debug.Log("Player built a hotel.");
        //Player.money -= pricePerHouse;
        hasHotel = true;
        // TO-DO: animation for hotel being built
    }
    
    void showCard()
    {
        GameObject cardPanel = CardReader.cardPanel;
        cardPanel.SetActive(true);

        cardPanel.transform.GetChild(0).transform.GetComponent<Image>().color
            = new Color32((byte)cardColor[0], (byte)cardColor[1], (byte)cardColor[2], 255);
        cardPanel.transform.GetChild(0).transform.GetChild(0).GetComponent<Text>().text = cardName;
        cardPanel.transform.GetChild(1).GetComponent<Text>().text = "RENT $" + rent[0].ToString();

        cardPanel.transform.GetChild(3).GetComponent<Text>().text =
            "$" + rent[1].ToString() + '\n' +
            "$" + rent[2].ToString() + '\n' +
            "$" + rent[3].ToString() + '\n' +
            "$" + rent[4].ToString() + '\n';

        cardPanel.transform.GetChild(4).GetComponent<Text>().text =
            "With HOTEL $" + rent[5].ToString() + '\n' +
            "Mortgage Value $" + mortgageValue.ToString() + '\n' +
            "Houses cost $" + pricePerHouse.ToString() + " each\n" +
            "Hotels, $" + pricePerHouse.ToString() + " plus 4 houses";
    }

    void hideCard(GameObject player)
    {
        CardReader.buyPropertyButton.SetActive(false);
        CardReader.cancelButton.SetActive(false);
        CardReader.cardPanel.SetActive(false);
        player.GetComponent<Player>().endMovement();
    }

    void buyProperty(GameObject player)
    {
        player.GetComponent<Player>().buyProperty(this);
        hideCard(player);
    }
    
}
