using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

[Serializable]
public class PropertyCard : Card
{
    public int id;                          // Table id
    public int cardIndex;                   // Index in properties vector
    public int[] cardColor = new int[3];
    public string cardName;
    public int priceValue;
    public int mortgageValue;
    public int pricePerHouse;
    public float[] houseCoordinates = new float[3];
    private float houseOffset = 0.3f;
    public int cardsInGroup = 2;
    public bool hasHotel = false;
    public int[] rent = new int[6];
    public int[] propertiesFromSameGroup;
    public List<NetworkInstanceId> buildings = new List<NetworkInstanceId>();

    public int housesBuilt { get; set; } = 0;

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
        int ownerId = GameObject.Find("GameManager").GetComponent<GameManager>().cardsOwner[cardIndex];
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
                int amountPaid = calculateRent(ownerId);
                CardReader.payRentButton.transform.GetChild(0).GetComponent<Text>().text = "Pay $" + amountPaid;
                CardReader.payRentButton.SetActive(true);
                CardReader.payRentButton.GetComponent<Button>().onClick.AddListener(() => payRent(player,playerScript, ownerId, amountPaid));
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

    //  verifies if the player has all properties in the same colour group
    bool hasAllProperties(int ownerId)
    {
        for(int index=0; index < propertiesFromSameGroup.Length; index++)
        {
            if (GameObject.Find("GameManager").GetComponent<GameManager>().cardsOwner[propertiesFromSameGroup[index]] != ownerId)
                return false;
        }
        Debug.Log("Player " + ownerId + " has all properies.");
        return true;
    }

    //  verifies if the all properties in the same colour group are equally developed
    public bool propertiesAreEquallyDeveloped(bool forSelling = false)
    {
        if(cardsInGroup == 2)
        {
            int brotherCardId = propertiesFromSameGroup[0];
            int minMaxValue;
            minMaxValue = forSelling ? Math.Max(CardReader.propertyCards[brotherCardId].housesBuilt, housesBuilt) : Math.Min(CardReader.propertyCards[brotherCardId].housesBuilt, housesBuilt);
            if (minMaxValue != housesBuilt)
                return false;
        }
        else if(cardsInGroup == 3)
        {
            int brotherCardId = propertiesFromSameGroup[0];
            int sisterCardId = propertiesFromSameGroup[1];
            int brotherHouses = CardReader.propertyCards[brotherCardId].housesBuilt;
            int sisterHouses = CardReader.propertyCards[sisterCardId].housesBuilt;
            int minMaxValue = forSelling ? Math.Max(Math.Max(brotherHouses, sisterHouses), housesBuilt) : Math.Min(Math.Min(brotherHouses, sisterHouses), housesBuilt);
            if (minMaxValue != housesBuilt)
                return false;
        }


        return true;
    }

    public void buildHouse(GameObject player, bool useManual = false, int manualIndexOffset = 1)
    {
        closeCard();
        if (!hasHotel && housesBuilt < 4 && propertiesAreEquallyDeveloped())
        {
            Debug.Log("Player built a house.");
            if(!useManual)
                player.GetComponent<Player>().CmdAddMoney(-pricePerHouse);

            int leftRight, downUp;
            leftRight = downUp = 0;
            Vector3 houseStartPosition = new Vector3(houseCoordinates[0], houseCoordinates[1], houseCoordinates[2]);

            if (0 < id && id < 10) leftRight = -1;
            if (10 < id && id < 20) downUp = 1;
            if (20 < id && id < 30) leftRight = 1;
            if (30 < id && id <= 39) downUp = -1;

            Vector3 offsetVector = new Vector3(houseOffset * leftRight, 0, houseOffset * downUp);

            // Would've been prettier with multiple if statements
            Vector3 rotation = new Vector3(0, 180 * (Mathf.Abs((1 - leftRight) / 2)) + (-downUp * 90), 0);

            if (useManual)
                player.GetComponent<Player>().CmdConstructHouse("House", houseStartPosition + (offsetVector * manualIndexOffset), rotation,cardIndex);
            else
                player.GetComponent<Player>().CmdConstructHouse("House", houseStartPosition + (offsetVector * housesBuilt), rotation, cardIndex);
        }
        else if (housesBuilt == 4)
            buildHotel(player);
    }

    public void buildHotel(GameObject player)
    {
        Debug.Log("Player built a hotel.");

        // First delete all the houses on the property

        player.GetComponent<Player>().CmdAddMoney(-pricePerHouse);

        int leftRight, downUp;
        leftRight = downUp = 0;
        Vector3 houseStartPosition = new Vector3(houseCoordinates[0], houseCoordinates[1], houseCoordinates[2]);

        if (0 < id && id < 10) leftRight = -1;
        if (10 < id && id < 20) downUp = 1;
        if (20 < id && id < 30) leftRight = 1;
        if (30 < id && id <= 39) downUp = -1;

        Vector3 offsetVector = new Vector3(houseOffset * leftRight, 0, houseOffset * downUp);

        // Would've been prettier with multiple if statements
        Vector3 rotation = new Vector3(0, 180 * (Mathf.Abs((1 - leftRight) / 2)) + (-downUp * 90), 0);

        player.GetComponent<Player>().CmdConstructHotel("Hotel", houseStartPosition + (3 * offsetVector / 2), rotation, cardIndex);

    }

    public void sellHouse(GameObject player)
    {
        closeCard();
        Debug.LogWarning("Do I have hotel? " + hasHotel);
        if (hasHotel)
        {
            Debug.LogWarning("Entering CmdDeconstructHotel");
            player.GetComponent<Player>().CmdDeconstructHotel(cardIndex);
            Debug.LogWarning("Exiting CmdDeconstructHotel");

            housesBuilt = 0;
            hasHotel = false;

            Debug.LogWarning("Entering for loop");
            for (int i = 0; i < 4; i++)
            {
                Debug.LogWarning("I am in the for loop, constructing " + i + " / 4 houses");
                buildHouse(player, true, i);
            }
        }
        else
        {
            player.GetComponent<Player>().CmdDeconstructHouse(cardIndex);
        }

        player.GetComponent<Player>().CmdAddMoney(pricePerHouse/2);
    }

    public void addHouseObject(NetworkInstanceId houseId)
    {
        buildings.Add(houseId);
        Debug.LogWarning("Added a house object to the list");
    }
    
    public void removeHouses()
    {
        buildings.RemoveRange(0, buildings.Count);
    }

    public void removeLastHouse()
    {
        if (buildings.Count > 0)
            buildings.RemoveAt(buildings.Count - 1);
        else
            Debug.LogError("Tried to delete a house that wasn't in the buildings list");
    }
    
    public override void showOwnedCard(GameObject player)
    {
        closeCard();
        if (player.GetComponent<Player>().getStage() != 0) 
            return;
        showCard();
        CardReader.closeButton.SetActive(true);
        CardReader.closeButton.GetComponent<Button>().onClick.AddListener(closeCard);

        // if it's your turn you can sell/buy houses and sell the property
         if (GameObject.Find("GameManager").GetComponent<GameManager>().playerTurn == player.GetComponent<Player>().idPlayer)
        {
            // Sell property button
            CardReader.sellPropertyButton.SetActive(true);
            CardReader.sellPropertyButton.GetComponent<Button>().onClick.RemoveAllListeners();
            CardReader.sellPropertyButton.GetComponent<Button>().onClick.AddListener( () => sellProperty(player) );

            if(hasAllProperties(player.GetComponent<Player>().idPlayer) && propertiesAreEquallyDeveloped() && !hasHotel)
            {
                // Buy house
                CardReader.buyHouseButton.SetActive(true);
                CardReader.buyHouseButton.GetComponent<Button>().onClick.RemoveAllListeners();
                CardReader.buyHouseButton.GetComponent<Button>().onClick.AddListener(() => buildHouse(player));
            }

            // Check to see if selling is a valid move
            if (hasAllProperties(player.GetComponent<Player>().idPlayer) && housesBuilt > 0 && propertiesAreEquallyDeveloped(true))
            {
                // Sell house
                CardReader.sellHouseButton.SetActive(true);
                CardReader.sellHouseButton.GetComponent<Button>().onClick.RemoveAllListeners();
                CardReader.sellHouseButton.GetComponent<Button>().onClick.AddListener(() => sellHouse(player));
            }
        }

    }

    void showCard()
    {
        CardReader.railroadPanel.SetActive(false);
        CardReader.utilityPanel.SetActive(false);
        closeCard();

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
        CardReader.buyHouseButton.SetActive(false);
        CardReader.sellHouseButton.SetActive(false);
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
        playerScript.CmdChangeOwner(playerScript.idPlayer, cardIndex,id); 
        playerScript.CmdTakeMoney(priceValue);
        playerScript.buyProperty(this);
        hideCard(player);
    }

    void sellProperty(GameObject player)
    {
        // TODO: SELL ALL THE HOUSES OR THE HOTEL AS WELL 
        if(housesBuilt > 0) return;
        CardReader.sellPropertyButton.GetComponent<Button>().onClick.RemoveAllListeners();
        CardReader.sellPropertyButton.SetActive(false);
        Player playerScript = player.GetComponent<Player>();
        playerScript.CmdChangeOwner(-1, cardIndex,id);
        playerScript.CmdAddMoney(mortgageValue); // players get in return the card's mortgage value
        playerScript.sellProperty(this);
        closeCard();
    }

    int calculateRent(int ownerId)
    {
        int amountPaid = rent[0];
        if (hasHotel == true)
        {
            // Daca are hotel costul e ultimul din vectorul rent
            amountPaid = rent[5];
        }
        else if (housesBuilt > 0)
        {
            // Daca are un nr de case atunci nr de case este indexul pentru vectorul rent
            amountPaid = rent[housesBuilt];
        }
        else if (hasAllProperties(ownerId) == true)
        {   // Daca are toate proprietatile din color group si fara case=> rent[0]x2
            amountPaid = 2 * rent[0];
        }
        return amountPaid;
    }

    // player pays rent to player[ownerId]
    void payRent(GameObject player, Player playerScript, int ownerId, int amountPaid) 
    { 
        CardReader.payRentButton.GetComponent<Button>().onClick.RemoveAllListeners();
        CardReader.payRentButton.SetActive(false);
         
        playerScript.CmdTakeMoney(amountPaid);
        playerScript.CmdGiveMoneyToPlayer(ownerId, amountPaid);

        CardReader.cardPanel.SetActive(false);
        player.GetComponent<Player>().endMovement();
    }

}
