using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

[Serializable]
public class PropertyCard : Card
{
    [SyncVar] public int id;
    [SyncVar] public int[] cardColor = new int[3];
    [SyncVar] public string cardName;
    [SyncVar] public int priceValue;
    [SyncVar] public int mortgageValue;
    [SyncVar] public int pricePerHouse;
    [SyncVar] private int housesBuilt = 0;
    [SyncVar] public int cardsInGroup = 2;
    [SyncVar] private bool hasHotel = false;
    [SyncVar] public int[] rent = new int[6];
    [SyncVar] public int[] propertiesFromSameGroup;

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
        Debug.Log("Owner's id for " + cardName + " : " + OwnerId);
        if (OwnerId != -1)
        {
            Player playerScript = player.GetComponent<Player>();
            if (OwnerId == playerScript.idPlayer)
            {
                CardReader.closeButton.SetActive(true);
                CardReader.closeButton.GetComponent<Button>().onClick.AddListener(() => closeCard(player));
            }
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
            CardReader.cancelButton.SetActive(true);
            CardReader.buyPropertyButton.GetComponent<Button>().onClick.AddListener(() => buyProperty(player));
            CardReader.cancelButton.GetComponent<Button>().onClick.AddListener(() => hideCard(player));
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
    
    public void showOwnedCard(GameObject player)
    {
        if (player.GetComponent<Player>().getStage() != 0)
            return;
        showCard();
        CardReader.closeButton.SetActive(true);
        CardReader.closeButton.GetComponent<Button>().onClick.AddListener(closeCard);
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

    void closeCard()
    {
        CardReader.closeButton.GetComponent<Button>().onClick.RemoveAllListeners();
        CardReader.closeButton.SetActive(false);
        CardReader.cardPanel.SetActive(false);
    }

    void closeCard(GameObject player)
    {
        CardReader.closeButton.GetComponent<Button>().onClick.RemoveAllListeners();
        CardReader.closeButton.SetActive(false);
        CardReader.cardPanel.SetActive(false);
        player.GetComponent<Player>().endMovement();
    }

    void hideCard(GameObject player)
    {
        CardReader.buyPropertyButton.GetComponent<Button>().onClick.RemoveAllListeners();
        CardReader.cancelButton.GetComponent<Button>().onClick.RemoveAllListeners();
        CardReader.buyPropertyButton.SetActive(false);
        CardReader.cancelButton.SetActive(false);
        CardReader.cardPanel.SetActive(false);
        player.GetComponent<Player>().endMovement();
    }

    void buyProperty(GameObject player)
    {
        Player playerScript = player.GetComponent<Player>();
        OwnerId = playerScript.idPlayer;
        //Debug.Log("Owner's id for " + cardName + " : " + OwnerId);
        playerScript.CmdTakeMoney(priceValue);
        playerScript.buyProperty(this);
        hideCard(player);
    }
    
}
