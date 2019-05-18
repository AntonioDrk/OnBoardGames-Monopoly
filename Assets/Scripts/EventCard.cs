using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class EventCard : Card
{
    public int id;
    public int[] cardColor = new int[3];
    public string cardName;
    public string description;

    public void EventCardConstructor()
    {
        Id = id;
        CardName = cardName;
        Price = 0;
        Mortgage = 0;
    }

    public override string ToString()
    {
        return "Id: " + id.ToString() + 
               "Type: " + cardName.ToString() + 
               "Description: " + description.ToString();
    }

    void hideCard()
    {
        CardReader.closeEventButton.GetComponent<Button>().onClick.RemoveAllListeners();
        CardReader.ChanceLogo.SetActive(false);
        CardReader.ComunityChestLogo.SetActive(false);
        CardReader.eventPanel.SetActive(false);
    }

    public override void doAction(GameObject player)
    {
        
        CardReader.eventPanel.SetActive(true);
        CardReader.eventPanel.transform.GetChild(0).GetComponent<Text>().text = cardName;
        CardReader.eventPanel.transform.GetChild(1).GetComponent<Text>().text = description;
        if (cardName == "Chance")
            CardReader.ChanceLogo.SetActive(true);
        else
            CardReader.ComunityChestLogo.SetActive(true);
        CardReader.closeEventButton.GetComponent<Button>().onClick.AddListener(() => activateEventCard(player));
                
    }

    void activateEventCard(GameObject player)
    {
        hideCard();
        bool isMoving = false;
        if (cardName.Equals("Chance"))
        {
            switch (id)
            {
                case 0:
                    Debug.Log(description); // move back 3 spaces -> isMoving = true;
                    break;
                case 1:
                    takeMoneyFromPlayer(player, 15);
                    break;
                case 2:
                    movePlayerToIndex(player, 0);
                    isMoving = true;
                    break;
                case 3:
                    Debug.Log(description);// case si hoteluri
                    break;
                case 4:
                    movePlayerToIndex(player, 24);
                    isMoving = true;
                    break;
                case 5:
                    Debug.Log(description); // pay each player
                    break;
                case 6:
                    movePlayerToRailroad(player);
                    isMoving = true;
                    break;
                case 7:
                    movePlayerToIndex(player, 5);
                    isMoving = true;
                    break;
                case 8:
                    giveMoneyToPlayer(player, 150);
                    break;
                case 9:
                    giveMoneyToPlayer(player, 50);
                    break;
                case 10:
                    movePlayerToIndex(player, 11);
                    isMoving = true;
                    break;
                case 11:
                    movePlayerToRailroad(player);
                    isMoving = true;
                    break;
                case 12:
                    sendPlayerToJail(player);
                    isMoving = true;
                    break;
                case 13:
                    movePlayerToIndex(player, 39);
                    isMoving = true;
                    break;
                case 14:
                    Debug.Log(description); //get out of jail
                    break;
            }
        }
        else if (cardName.Equals("Community Chest"))
        {
            switch (id)
            {
                case 0:
                    giveMoneyToPlayer(player, 20);
                    break;
                case 1:
                    Debug.Log(description); //collect from every player
                    break;
                case 2:
                    takeMoneyFromPlayer(player, 100);
                    break;
                case 3:
                    giveMoneyToPlayer(player, 45);
                    break;
                case 4:
                    giveMoneyToPlayer(player, 100);
                    break;
                case 5:
                    giveMoneyToPlayer(player, 25);
                    break;
                case 6:
                    Debug.Log(description); //case si hoteluri
                    break;
                case 7:
                    takeMoneyFromPlayer(player, 50);
                    break;
                case 8:
                    giveMoneyToPlayer(player, 100);
                    break;
                case 9:
                    giveMoneyToPlayer(player, 10);
                    break;
                case 10:
                    giveMoneyToPlayer(player, 200);
                    break;
                case 11:
                    movePlayerToIndex(player, 0);
                    isMoving = true;
                    break;
                case 12:
                    sendPlayerToJail(player);
                    isMoving = true;
                    break;
                case 13:
                    giveMoneyToPlayer(player, 100);
                    break;
                case 14:
                    Debug.Log(description); //get out of jail
                    break;
            }
        }

        if (!isMoving)
            player.GetComponent<Player>().endMovement();
    }

    public void takeMoneyFromPlayer(GameObject player, int value)
    {
        player.GetComponent<Player>().CmdTakeMoney(value);
    }

    public void giveMoneyToPlayer(GameObject player, int value)
    {
        player.GetComponent<Player>().CmdAddMoney(value);
    }

    public void sendPlayerToJail(GameObject player)
    {
        player.GetComponent<Player>().goToJail();
    }

    public void movePlayerToIndex(GameObject player, int index)
    {
        int playerIndex = player.GetComponent<Player>().getIndexPosition();
        int spacesToMove;
        if (playerIndex < index)
            spacesToMove = index - playerIndex;
        else
            spacesToMove = 40 - playerIndex + index;

        Debug.Log("Player index: " + playerIndex);
        Debug.Log("Spaces to move to reach field with index " + index + ": " + spacesToMove);
        player.GetComponent<Player>().moveSpaces(spacesToMove);
    }

    public void movePlayerToRailroad(GameObject player)
    {
        int playerIndex = player.GetComponent<Player>().getIndexPosition();
        int index = 5;
        if (playerIndex < 5)
            index = 5;
        else if (playerIndex > 5 && playerIndex < 15)
            index = 15;
        else if (playerIndex > 15 && playerIndex < 25)
            index = 25;
        else if (playerIndex > 25 && playerIndex < 35)
            index = 35;
        else if (playerIndex > 35)
            index = 5;
        Debug.Log("Closest Railroad: " + index);
        movePlayerToIndex(player, index);
    }

    public void getPlayerOutOfJail(GameObject player)
    {

    }
}
