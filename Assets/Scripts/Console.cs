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
            input.OnPointerClick(new PointerEventData(EventSystem.current));
        }
    }

    public void executeCommand()
    {
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        string inputCommand = input.text;
        inputCommand = inputCommand.TrimEnd(' ').TrimStart(' ');
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

            if (cardIndex < 28 && ownerId < gameManager.connectedPlayers)
            {
                if (gameManager.cardsOwner[cardIndex] != -1)
                {
                    gameManager.players[gameManager.cardsOwner[cardIndex]].GetComponent<Player>().RpcSellProperty(cardIndex);
                }

                gameManager.players[ownerId].GetComponent<Player>().RpcBuyProperty(cardIndex);
                gameManager.CmdChangeOwner(cardIndex, ownerId);
            }
        }
        else if (command[0] == "movePlayer" && command.Length == 3)
        {
            int playerId = int.Parse(command[1]);
            int amount = int.Parse(command[2]);
            if (playerId < gameManager.connectedPlayers && amount > 0)
                gameManager.players[playerId].GetComponent<Player>().moveSpaces(amount);
        }
    }
}
