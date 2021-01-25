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
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
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
        string inputCommand = input.text;
        inputCommand = inputCommand.TrimEnd(' ').TrimStart(' ');
        string[] command = inputCommand.Split(' ');

        switch (command[0])
        {
            case "takeMoney" when command.Length == 3:
            {
                int playerId = int.Parse(command[1]);
                int amount = int.Parse(command[2]);
                if (playerId < gameManager.connectedPlayers)
                    gameManager.players[playerId].GetComponent<Player>().CmdTakeMoney(amount);
                break;   
            }
            case "addMoney" when command.Length == 3:
            {
                int playerId = int.Parse(command[1]);
                int amount = int.Parse(command[2]);
                if (playerId < gameManager.connectedPlayers)
                    gameManager.CmdGiveMoneyToPlayer(playerId, amount);
                break;
            }
            case "changeOwner" when command.Length == 3:
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

                break;
            }
            case "movePlayer" when command.Length == 3:
            {
                int playerId = int.Parse(command[1]);
                int amount = int.Parse(command[2]);
                if (playerId < gameManager.connectedPlayers && amount > 0)
                    gameManager.players[playerId].GetComponent<Player>().moveSpaces(amount);
                break;
            }
        }
    }
}
