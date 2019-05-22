using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class RailroadCard  : Card
{
    public int id;
    public string cardName;
    public int[] rent = { 25, 50, 100, 200 };

    public void RailroadCardConstructor()
    {
        Id = id;
        CardName = cardName;
        Price = 200;
        Mortgage = 100;
    }

    public override string ToString()
    {
        return "Id: " + Id.ToString() +
               "\nName: " + cardName.ToString() +
               "\nPrice: " + Price.ToString() +
               "\nMortgage: " + Mortgage.ToString();
    }

    void buyRailroad(GameObject player, int cardIndex)
    {
        Player playerScript = player.GetComponent<Player>();
        playerScript.CmdChangeOwner(playerScript.idPlayer, cardIndex);
        playerScript.CmdTakeMoney(Price);
        playerScript.buyProperty(this);
        hideCard(player);
    }

    void hideCard(GameObject player)
    {
        CardReader.buyPropertyButton.GetComponent<Button>().onClick.RemoveAllListeners();
        CardReader.cancelButton.GetComponent<Button>().onClick.RemoveAllListeners();
        CardReader.buyPropertyButton.SetActive(false);
        CardReader.cancelButton.SetActive(false);
        CardReader.railroadPanel.SetActive(false);
        player.GetComponent<Player>().endMovement();
    }

    void closeCard(GameObject player)
    {
        CardReader.closeButton.GetComponent<Button>().onClick.RemoveAllListeners();
        CardReader.closeButton.SetActive(false);
        CardReader.railroadPanel.SetActive(false);
        player.GetComponent<Player>().endMovement();
    }

    public override void doAction(GameObject player)
    {
        int cardIndex = (id - 5) / 10;
        int ownerId = GameObject.Find("GameManager").GetComponent<GameManager>().cardsOwner[22 + cardIndex]; // the railroads are 22,23,24,25
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
                int amountPaid = rent[numberOfRailroads(ownerId) - 1];
                CardReader.payRentButton.transform.GetChild(0).GetComponent<Text>().text = "Pay $" + amountPaid;
                CardReader.payRentButton.SetActive(true);
                CardReader.payRentButton.GetComponent<Button>().onClick.AddListener(() => payRent(player, playerScript, ownerId, amountPaid));
            }
        }
        else
        {
            Debug.Log("Player can buy this railroad. Card owner: " + ownerId);
            CardReader.buyPropertyButton.SetActive(true);
            CardReader.cancelButton.SetActive(true);
            CardReader.buyPropertyButton.GetComponent<Button>().onClick.AddListener(() => buyRailroad(player,22 + cardIndex));
            CardReader.cancelButton.GetComponent<Button>().onClick.AddListener(() => hideCard(player));
        }
        
    }

    // player pays rent to player[ownerId]
    void payRent(GameObject player, Player playerScript, int ownerId, int amountPaid)
    {
        CardReader.payRentButton.GetComponent<Button>().onClick.RemoveAllListeners();
        CardReader.payRentButton.SetActive(false);
        
        playerScript.CmdTakeMoney(amountPaid);
        playerScript.CmdGiveMoneyToPlayer(ownerId, amountPaid);

        CardReader.railroadPanel.SetActive(false);
        player.GetComponent<Player>().endMovement();
    }

    int numberOfRailroads(int ownerId)
    {
        int count = 0;
        for (int k = 22; k < 26; k++)
            if (GameObject.Find("GameManager").GetComponent<GameManager>().cardsOwner[k] == ownerId)
                count++;
        Debug.Log("Player " + ownerId + " has " + count + " railroads.");
        return count;
    }

    void showCard()
    {
        CardReader.cardPanel.SetActive(false);
        CardReader.utilityPanel.SetActive(false);
        closeCard();

        CardReader.railroadPanel.SetActive(true);
        CardReader.railroadPanel.transform.GetChild(0).GetComponent<Text>().text = cardName;
    }

    void closeCard()
    {
        CardReader.closeButton.GetComponent<Button>().onClick.RemoveAllListeners();
        CardReader.sellPropertyButton.SetActive(false);
        CardReader.closeButton.SetActive(false);
        CardReader.railroadPanel.SetActive(false);
    }

    public override void showOwnedCard(GameObject player)
    {
        if (player.GetComponent<Player>().getStage() != 0)
            return;
        showCard();
        CardReader.closeButton.SetActive(true);
        CardReader.closeButton.GetComponent<Button>().onClick.AddListener(closeCard);

        // if it's your turn you can sell the property
        if (GameObject.Find("GameManager").GetComponent<GameManager>().playerTurn == player.GetComponent<Player>().idPlayer)
        {
            CardReader.sellPropertyButton.SetActive(true);
            CardReader.sellPropertyButton.GetComponent<Button>().onClick.RemoveAllListeners();
            CardReader.sellPropertyButton.GetComponent<Button>().onClick.AddListener(() => sellRailroad(player));
        }
    }

    void sellRailroad(GameObject player)
    {
        int cardIndex = 22 + (id - 5) / 10;
        CardReader.sellPropertyButton.GetComponent<Button>().onClick.RemoveAllListeners();
        CardReader.sellPropertyButton.SetActive(false);
        Player playerScript = player.GetComponent<Player>();
        playerScript.CmdChangeOwner(-1, cardIndex);
        playerScript.CmdAddMoney(Mortgage); // players get in return the card's mortgage value
        playerScript.sellProperty(this);
        closeCard();
    }

}
