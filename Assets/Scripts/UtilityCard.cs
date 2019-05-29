using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class UtilityCard : Card
{
    public int id;
    public string cardName;
    public int[] rentMultiplier = { 4, 10 };

    public void UtilityCardConstructor()
    {
        Id = id;
        CardName = cardName;
        Price = 150;
        Mortgage = 75;
    }

    public override string ToString()
    {
        return "Id: " + Id.ToString() +
               "\nName: " + cardName.ToString() +
               "\nPrice: " + Price.ToString() +
               "\nMortgage: " + Mortgage.ToString();
    }
    
    void buyUtility(GameObject player)
    {
        int cardIndex = 26 + (id - 12) / 16;
        Player playerScript = player.GetComponent<Player>();
        playerScript.CmdChangeOwner(playerScript.idPlayer, cardIndex,id);
        SoundManager.Instance.PlaySound(SoundManager.Instance.payMoney);
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
        CardReader.utilityPanel.SetActive(false);
        player.GetComponent<Player>().endMovement();
    }

    void closeCard(GameObject player)
    {
        CardReader.closeButton.GetComponent<Button>().onClick.RemoveAllListeners();
        CardReader.closeButton.SetActive(false);
        CardReader.utilityPanel.SetActive(false);
        player.GetComponent<Player>().endMovement();
    }
    
    public override void doAction(GameObject player)
    {
        int cardIndex = (id - 12) / 16;
        int ownerId = GameObject.Find("GameManager").GetComponent<GameManager>().cardsOwner[26 + cardIndex]; // the utilities are 26, 27
        showCard();
        Debug.Log("Owner's id for " + cardName + " : " + ownerId);
        if (ownerId != -1)
        {
            Player playerScript = player.GetComponent<Player>();
            if (ownerId == playerScript.idPlayer)
            {
                CardReader.closeButton.SetActive(true);
                CardReader.closeButton.GetComponent<Button>().onClick.AddListener(() => closeCardSound(player));
            }
            else
            {
                Debug.Log("Player must pay rent to player " + ownerId);
                int rolledNumber = GameObject.Find("GameManager").GetComponent<GameManager>().currentRolledNumber;
                int amountPaid = rentMultiplier[numberOfUtilities(ownerId) - 1] * rolledNumber;
                CardReader.payRentButton.transform.GetChild(0).GetComponent<Text>().text = "Pay $" + amountPaid;
                CardReader.payRentButton.SetActive(true);
                CardReader.payRentButton.GetComponent<Button>().onClick.AddListener(() => payRent(player, playerScript, ownerId, amountPaid));
            }
        }
        else
        {
            Debug.Log("Player can buy this utility. Card owner: " + ownerId);
            CardReader.buyPropertyButton.SetActive(true);
            CardReader.cancelButton.SetActive(true);
            CardReader.buyPropertyButton.GetComponent<Button>().onClick.AddListener(() => buyUtility(player));
            CardReader.cancelButton.GetComponent<Button>().onClick.AddListener(() => hideCardSound(player));
        }
        
    }

    // player pays rent to player[ownerId]
    void payRent(GameObject player, Player playerScript, int ownerId, int amountPaid)
    {
        CardReader.payRentButton.GetComponent<Button>().onClick.RemoveAllListeners();
        CardReader.payRentButton.SetActive(false);       

        SoundManager.Instance.PlaySound(SoundManager.Instance.payMoney);
        playerScript.CmdTakeMoney(amountPaid);
        playerScript.CmdGiveMoneyToPlayer(ownerId, amountPaid);

        CardReader.utilityPanel.SetActive(false);
        player.GetComponent<Player>().endMovement();
    }

    int numberOfUtilities(int ownerId)
    {
        int count = 0;
        for (int k = 26; k < 28; k++)
            if (GameObject.Find("GameManager").GetComponent<GameManager>().cardsOwner[k] == ownerId)
                count++;
        return count;
    }
    
    void showCard()
    {
        CardReader.cardPanel.SetActive(false);
        CardReader.railroadPanel.SetActive(false);
        closeCard();

        CardReader.utilityPanel.SetActive(true);
        CardReader.utilityPanel.transform.GetChild(0).GetComponent<Text>().text = cardName;
        if (id == 12)
            CardReader.ElectricCompanyLogo.SetActive(true);
        else
            CardReader.WaterWorksLogo.SetActive(true);
    }

    void closeCard()
    {
        CardReader.closeButton.GetComponent<Button>().onClick.RemoveAllListeners();
        CardReader.sellPropertyButton.SetActive(false);
        CardReader.closeButton.SetActive(false);
        CardReader.ElectricCompanyLogo.SetActive(false);
        CardReader.WaterWorksLogo.SetActive(false);
        CardReader.utilityPanel.SetActive(false);
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
        CardReader.closeButton.SetActive(true);
        CardReader.closeButton.GetComponent<Button>().onClick.AddListener(closeCardSound);

        // if it's your turn you can sell the property
        if (GameObject.Find("GameManager").GetComponent<GameManager>().playerTurn == player.GetComponent<Player>().idPlayer)
        {
            CardReader.sellPropertyButton.SetActive(true);
            CardReader.sellPropertyButton.GetComponent<Button>().onClick.RemoveAllListeners();
            CardReader.sellPropertyButton.GetComponent<Button>().onClick.AddListener(() => sellUtility(player));
        }
    }

    void sellUtility(GameObject player)
    {
        int cardIndex = 26 + (id - 12) / 16;
        CardReader.sellPropertyButton.GetComponent<Button>().onClick.RemoveAllListeners();
        CardReader.sellPropertyButton.SetActive(false);
        Player playerScript = player.GetComponent<Player>();
        playerScript.CmdChangeOwner(-1, cardIndex,id);
        playerScript.CmdAddMoney(Mortgage); // players get in return the card's mortgage value
        playerScript.sellProperty(this);
        closeCard();
    }
}
