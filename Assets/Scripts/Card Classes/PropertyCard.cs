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
    public bool hasHotel;
    public int[] rent = new int[6];
    public int[] propertiesFromSameGroup;
    public List<NetworkInstanceId> buildings = new List<NetworkInstanceId>();
    public int housesBuilt
    {
        get;
        set;
    }

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
        Player playerScript = player.GetComponent<Player>();
        if (ownerId != -1)
        {
            if (ownerId == playerScript.idPlayer)
            {
                UIManager.closeButton.SetActive(true);
                UIManager.closeButton.GetComponent<Button>().onClick.AddListener(() => closeCardSound(player));
            }
            else
            { 
                Debug.Log("Player must pay rent to player " + ownerId);
                int amountPaid = calculateRent(ownerId);
                UIManager.payRentButton.transform.GetChild(0).GetComponent<Text>().text = "Pay $" + amountPaid;
                UIManager.payRentButton.SetActive(true);
                UIManager.payRentButton.GetComponent<Button>().onClick.AddListener(() => payRent(player,playerScript, ownerId, amountPaid));
            }
        }
        else
        {
            Debug.Log("Player can buy this property. Card owner: " + ownerId);

            if (playerScript.getMoney() > Price)
            {
                UIManager.buyPropertyButton.SetActive(true);
                UIManager.buyPropertyButton.GetComponent<Button>().onClick.AddListener(() => buyCard(player));
            }
            UIManager.cancelButton.SetActive(true);
            UIManager.cancelButton.GetComponent<Button>().onClick.AddListener(() => hideCardSound(player));
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
        SoundManager.Instance.PlaySound(SoundManager.Instance.buyHouse);
        closeCard();
        if (!hasHotel && housesBuilt < 4 && propertiesAreEquallyDeveloped())
        {
            Debug.Log("Player built a house.");
            if(!useManual)
            {
                SoundManager.Instance.PlaySound(SoundManager.Instance.payMoney);
                player.GetComponent<Player>().CmdTakeMoney(pricePerHouse);
            }

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

        SoundManager.Instance.PlaySound(SoundManager.Instance.payMoney);
        player.GetComponent<Player>().CmdTakeMoney(pricePerHouse);

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
        SoundManager.Instance.PlaySound(SoundManager.Instance.getMoney);
        closeCard();
        //Debug.LogWarning("Do I have hotel? " + hasHotel);
        if (hasHotel)
        {
            //Debug.LogWarning("Entering CmdDeconstructHotel");
            player.GetComponent<Player>().CmdDeconstructHotel(cardIndex);
            //Debug.LogWarning("Exiting CmdDeconstructHotel");

            housesBuilt = 0;
            hasHotel = false;

            //Debug.LogWarning("Entering for loop");
            for (int i = 0; i < 4; i++)
            {
                //Debug.LogWarning("I am in the for loop, constructing " + i + " / 4 houses");
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
        //Debug.LogWarning("Added a house object to the list");
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
    
    void closeCardSound()
    {
        SoundManager.Instance.PlaySound(SoundManager.Instance.close);
        closeCard();
    }

    void closeCardSound(GameObject plr)
    {
        SoundManager.Instance.PlaySound(SoundManager.Instance.close);
        closeCard(plr);
    }

    void hideCardSound(GameObject plr)
    {
        SoundManager.Instance.PlaySound(SoundManager.Instance.close);
        hideCard(plr);
    }

    public override void showOwnedCard(GameObject player)
    {
        closeCard();
        if (player.GetComponent<Player>().getStage() != 0) 
            return;
        showCard();
        UIManager.closeButton.SetActive(true);
        UIManager.closeButton.GetComponent<Button>().onClick.AddListener(closeCardSound);

        // if it's your turn you can sell/buy houses and sell the property
         if (GameObject.Find("GameManager").GetComponent<GameManager>().playerTurnId == player.GetComponent<Player>().idPlayer)
        {
            // Sell property button
            UIManager.sellPropertyButton.SetActive(true);
            UIManager.sellPropertyButton.GetComponent<Button>().onClick.RemoveAllListeners();
            UIManager.sellPropertyButton.GetComponent<Button>().onClick.AddListener( () => sellCard(player) );

            if(hasAllProperties(player.GetComponent<Player>().idPlayer) && propertiesAreEquallyDeveloped() && !hasHotel)
            {
                // Buy house
                if (player.GetComponent<Player>().getMoney() > pricePerHouse)
                {                
                    UIManager.buyHouseButton.SetActive(true);
                    UIManager.buyHouseButton.GetComponent<Button>().onClick.RemoveAllListeners();
                    UIManager.buyHouseButton.GetComponent<Button>().onClick.AddListener(() => buildHouse(player));
                }
            }

            // Check to see if selling is a valid move
            if (hasAllProperties(player.GetComponent<Player>().idPlayer) && housesBuilt > 0 && propertiesAreEquallyDeveloped(true))
            {
                // Sell house
                UIManager.sellHouseButton.SetActive(true);
                UIManager.sellHouseButton.GetComponent<Button>().onClick.RemoveAllListeners();
                UIManager.sellHouseButton.GetComponent<Button>().onClick.AddListener(() => sellHouse(player));
            }
        }

    }

    void showCard()
    {
        SoundManager.Instance.PlaySound(SoundManager.Instance.openCard);
        UIManager.railroadPanel.SetActive(false);
        UIManager.utilityPanel.SetActive(false);
        closeCard();

        GameObject cardPanel = UIManager.cardPanel;
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
        UIManager.closeButton.GetComponent<Button>().onClick.RemoveAllListeners();
        UIManager.buyHouseButton.SetActive(false);
        UIManager.sellHouseButton.SetActive(false);
        UIManager.sellPropertyButton.SetActive(false);
        UIManager.closeButton.SetActive(false);
        UIManager.cardPanel.SetActive(false);
    }

    void closeCard(GameObject player)
    {
        UIManager.closeButton.GetComponent<Button>().onClick.RemoveAllListeners();
        UIManager.closeButton.SetActive(false);
        UIManager.cardPanel.SetActive(false);
        player.GetComponent<Player>().endMovement();
    }

    protected override void hideCard(GameObject player)
    {
        UIManager.buyPropertyButton.GetComponent<Button>().onClick.RemoveAllListeners();
        UIManager.cancelButton.GetComponent<Button>().onClick.RemoveAllListeners();
        UIManager.buyPropertyButton.SetActive(false);
        UIManager.cancelButton.SetActive(false);
        UIManager.cardPanel.SetActive(false);
        player.GetComponent<Player>().endMovement();
        base.hideCard(player);
    }

    protected override void buyCard(GameObject player)
    {
        Player playerScript = player.GetComponent<Player>();
        playerScript.CmdChangeOwner(playerScript.idPlayer, cardIndex); 
        base.buyCard(player);
        hideCard(player);
    }

    protected override void sellCard(GameObject player)
    {
        if(housesBuilt > 0) return;
        
        foreach (int i in propertiesFromSameGroup)
            if (CardReader.propertyCards[i].housesBuilt > 0) return;
        
        base.sellCard(player);
        
        Player playerScript = player.GetComponent<Player>();
        playerScript.CmdChangeOwner(-1, cardIndex);
        closeCard();
    }

    int calculateRent(int ownerId)
    {
        int amountPaid = rent[0];
        if (hasHotel)
        {
            // Daca are hotel costul e ultimul din vectorul rent
            amountPaid = rent[5];
        }
        else if (housesBuilt > 0)
        {
            // Daca are un nr de case atunci nr de case este indexul pentru vectorul rent
            amountPaid = rent[housesBuilt];
        }
        else if (hasAllProperties(ownerId))
        {   // Daca are toate proprietatile din color group si fara case=> rent[0]x2
            amountPaid = 2 * rent[0];
        }
        return amountPaid;
    }

    // player pays rent to player[ownerId]
    void payRent(GameObject player, Player playerScript, int ownerId, int amountPaid) 
    { 
        UIManager.payRentButton.GetComponent<Button>().onClick.RemoveAllListeners();
        UIManager.payRentButton.SetActive(false);
         
        SoundManager.Instance.PlaySound(SoundManager.Instance.payMoney);
        playerScript.CmdGiveMoneyToPlayer(ownerId, amountPaid);
        playerScript.CmdTakeMoney(amountPaid);

        UIManager.cardPanel.SetActive(false);
        if(playerScript.getMoney() > 0)
            player.GetComponent<Player>().endMovement();
    }

}
