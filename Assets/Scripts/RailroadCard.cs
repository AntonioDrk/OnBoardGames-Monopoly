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
        SoundManager.Instance.PlaySound(SoundManager.Instance.payMoney);
        playerScript.CmdTakeMoney(Price);
        playerScript.buyProperty(this);
        hideCard(player);
    }

    void hideCard(GameObject player)
    {
        UIManager.buyPropertyButton.GetComponent<Button>().onClick.RemoveAllListeners();
        UIManager.cancelButton.GetComponent<Button>().onClick.RemoveAllListeners();
        UIManager.buyPropertyButton.SetActive(false);
        UIManager.cancelButton.SetActive(false);
        UIManager.railroadPanel.SetActive(false);
        player.GetComponent<Player>().endMovement();
    }

    void closeCard(GameObject player)
    {
        UIManager.closeButton.GetComponent<Button>().onClick.RemoveAllListeners();
        UIManager.closeButton.SetActive(false);
        UIManager.railroadPanel.SetActive(false);
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
                UIManager.closeButton.SetActive(true);
                UIManager.closeButton.GetComponent<Button>().onClick.AddListener(() => closeCardSound(player));
            }
            else
            {
                Debug.Log("Player must pay rent to player " + ownerId);
                int amountPaid = rent[numberOfRailroads(ownerId) - 1];
                UIManager.payRentButton.transform.GetChild(0).GetComponent<Text>().text = "Pay $" + amountPaid;
                UIManager.payRentButton.SetActive(true);
                UIManager.payRentButton.GetComponent<Button>().onClick.AddListener(() => payRent(player, playerScript, ownerId, amountPaid));
            }
        }
        else
        {
            Debug.Log("Player can buy this railroad. Card owner: " + ownerId);
            UIManager.buyPropertyButton.SetActive(true);
            UIManager.cancelButton.SetActive(true);
            UIManager.buyPropertyButton.GetComponent<Button>().onClick.AddListener(() => buyRailroad(player,22 + cardIndex));
            UIManager.cancelButton.GetComponent<Button>().onClick.AddListener(() => hideCardSound(player));
        }
        
    }

    // player pays rent to player[ownerId]
    void payRent(GameObject player, Player playerScript, int ownerId, int amountPaid)
    {
        UIManager.payRentButton.GetComponent<Button>().onClick.RemoveAllListeners();
        UIManager.payRentButton.SetActive(false);
        
        SoundManager.Instance.PlaySound(SoundManager.Instance.payMoney);
        playerScript.CmdTakeMoney(amountPaid);
        playerScript.CmdGiveMoneyToPlayer(ownerId, amountPaid);

        UIManager.railroadPanel.SetActive(false);
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
        UIManager.cardPanel.SetActive(false);
        UIManager.utilityPanel.SetActive(false);
        closeCard();

        UIManager.railroadPanel.SetActive(true);
        UIManager.railroadPanel.transform.GetChild(0).GetComponent<Text>().text = cardName;
    }

    void closeCard()
    {
        UIManager.closeButton.GetComponent<Button>().onClick.RemoveAllListeners();
        UIManager.sellPropertyButton.SetActive(false);
        UIManager.closeButton.SetActive(false);
        UIManager.railroadPanel.SetActive(false);
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
        if (player.GetComponent<Player>().getStage() != 0)
            return;
        showCard();
        UIManager.closeButton.SetActive(true);
        UIManager.closeButton.GetComponent<Button>().onClick.AddListener(closeCardSound);

        // if it's your turn you can sell the property
        if (GameObject.Find("GameManager").GetComponent<GameManager>().playerTurnId == player.GetComponent<Player>().idPlayer)
        {
            UIManager.sellPropertyButton.SetActive(true);
            UIManager.sellPropertyButton.GetComponent<Button>().onClick.RemoveAllListeners();
            UIManager.sellPropertyButton.GetComponent<Button>().onClick.AddListener(() => sellRailroad(player));
        }
    }

    void sellRailroad(GameObject player)
    {
        int cardIndex = 22 + (id - 5) / 10;
        UIManager.sellPropertyButton.GetComponent<Button>().onClick.RemoveAllListeners();
        UIManager.sellPropertyButton.SetActive(false);
        Player playerScript = player.GetComponent<Player>();
        playerScript.CmdChangeOwner(-1, cardIndex);
        playerScript.CmdAddMoney(Mortgage); // players get in return the card's mortgage value
        playerScript.sellProperty(this);
        closeCard();
    }

}
