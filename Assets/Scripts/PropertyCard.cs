using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

[Serializable]
public class PropertyCard : Card
{
    public int id;
    public int cardIndex;
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
        int ownerId = GameObject.Find("GameManager").GetComponent<GameManager>().propertyCardsOwners[cardIndex];
        showCard();
        Debug.Log("Owner's id for " + cardName + " : " + ownerId);
        if (ownerId != -1)
        {
            Player playerScript = player.GetComponent<Player>();
            if (ownerId == playerScript.idPlayer)
            {
                CardReader.closeButton.SetActive(true);
                CardReader.closeButton.GetComponent<Button>().onClick.AddListener(() => closeCard(player));
            }
            else
            { 
                Debug.Log("Player must pay rent to player " + ownerId);
                CardReader.payRentButton.SetActive(true);
                CardReader.payRentButton.GetComponent<Button>().onClick.AddListener(() => payRent(player,playerScript, ownerId));
            }
        }
        else
        {
            Debug.Log("Player can buy this property. Card owner: " + ownerId);
            CardReader.buyPropertyButton.SetActive(true);
            CardReader.cancelButton.SetActive(true);
            CardReader.buyPropertyButton.GetComponent<Button>().onClick.AddListener(() => buyProperty(player));
            CardReader.cancelButton.GetComponent<Button>().onClick.AddListener(() => hideCard(player));
        }

    }

    //  verifies if the player has all properties in the same colour group but no houses on the property he landed on
    bool hasAllProperties(int ownerId)
    {
        if (housesBuilt != 0) return false;
        for(int index=0; index < propertiesFromSameGroup.Length; index++)
        {
            if (GameObject.Find("GameManager").GetComponent<GameManager>().propertyCardsOwners[propertiesFromSameGroup[index]] != ownerId)
                return false;
        }
        Debug.Log("Player " + ownerId + " has all properies.");
        return true;
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

        // if it's your turn you can sell/buy houses and sell the property
        if (GameObject.Find("GameManager").GetComponent<GameManager>().playerTurn == player.GetComponent<Player>().idPlayer)
        {
            CardReader.sellPropertyButton.SetActive(true);
            CardReader.sellPropertyButton.GetComponent<Button>().onClick.RemoveAllListeners();
            CardReader.sellPropertyButton.GetComponent<Button>().onClick.AddListener(() => sellProperty(player));
        }

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
        CardReader.sellPropertyButton.SetActive(false);
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
        playerScript.CmdChangeOwner(playerScript.idPlayer, cardIndex); 
        playerScript.CmdTakeMoney(priceValue);
        playerScript.buyProperty(this);
        hideCard(player);
    }

    void sellProperty(GameObject player)
    {
        CardReader.sellPropertyButton.GetComponent<Button>().onClick.RemoveAllListeners();
        CardReader.sellPropertyButton.SetActive(false);
        Player playerScript = player.GetComponent<Player>();
        playerScript.CmdChangeOwner(-1, cardIndex);
        playerScript.CmdAddMoney(mortgageValue); // players get in return the card's mortgage value
        playerScript.sellProperty(this);
        closeCard();
    }

    // player pays rent to player[ownerId]
    void payRent(GameObject player, Player playerScript, int ownerId) 
    { 
        CardReader.payRentButton.GetComponent<Button>().onClick.RemoveAllListeners();
        CardReader.payRentButton.SetActive(false);

        int amountPaid = 0;
        if (hasHotel)
            amountPaid = rent[5];
        else if (hasAllProperties(ownerId)) //daca are toate din color group si fara case=> rent[0]x2
            amountPaid = 2 * rent[0];
        else
            amountPaid = rent[housesBuilt];
        
        playerScript.CmdTakeMoney(amountPaid);
        playerScript.CmdGiveMoneyToPlayer(ownerId, amountPaid);

        CardReader.cardPanel.SetActive(false);
        player.GetComponent<Player>().endMovement();
    }

}
