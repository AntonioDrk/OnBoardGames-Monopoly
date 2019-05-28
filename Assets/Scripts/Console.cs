using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class Console : MonoBehaviour
{
    private InputField input;
    private GameManager gameManager;

    // Start is called before the first frame update
    void Start()
    {
        input = GameObject.Find("ConsoleInput").GetComponent<InputField>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return))
        {
            executeCommand();
            input.text = "";
            EventSystem.current.SetSelectedGameObject(input.gameObject, null);
            input.OnPointerClick(null);
        }
    }

    public void executeCommand()
    {
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        string inputCommand = input.text;
        string[] command = inputCommand.Split(' ');

        if(command[0] == "addMoney" && command.Length == 3)
        {
            int playerId = int.Parse(command[1]);
            int amount = int.Parse(command[2]);
            if (playerId < gameManager.connectedPlayers)
                gameManager.CmdGiveMoneyToPlayer(playerId, amount);
        }
        else if(command[0] == "changeOwner" && command.Length == 3)
        {
            int ownerId = int.Parse(command[1]);
            int cardIndex = int.Parse(command[2]);
            int idOnBoard = -1;

            if (cardIndex < 28 && ownerId < gameManager.connectedPlayers)
            {
                if (cardIndex < 22)
                    idOnBoard = CardReader.propertyCards[cardIndex].id;
                else if (cardIndex < 26)
                    idOnBoard = CardReader.railroadCards[cardIndex - 22].id;
                else
                    idOnBoard = CardReader.utilityCards[cardIndex - 26].id;

                if (gameManager.cardsOwner[cardIndex] != -1)
                {
                    gameManager.players[gameManager.cardsOwner[cardIndex]].GetComponent<Player>().RpcSellProperty(cardIndex);
                }

                gameManager.players[ownerId].GetComponent<Player>().RpcBuyProperty(cardIndex);
                gameManager.CmdChangeOwner(cardIndex, ownerId, idOnBoard);
            }
        }
        else if (command[0] == "movePlayer" && command.Length == 3)
        {
            int playerId = int.Parse(command[1]);
            int amount = int.Parse(command[2]);
            if (playerId < gameManager.connectedPlayers)
                gameManager.players[playerId].GetComponent<Player>().moveSpaces(amount);
        }
    }
}
