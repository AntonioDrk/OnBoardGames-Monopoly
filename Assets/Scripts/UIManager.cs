using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{

    public static GameObject cardPanel, buyPropertyButton, cancelButton, payRentButton, closeButton, closeEventButton, eventPanel,
                                comunityChestLogo, chanceLogo, sellPropertyButton, sellHouseButton, buyHouseButton, railroadPanel,
                                utilityPanel, electricCompanyLogo, waterWorksLogo, buttonInfo, canvas, inJailCardPanel, playerTradePanel,
                                tradeButton, console, startGameButton, rollButton, endTurnButton, ownedPropertiesPanel;
    
    // Start is called before the first frame update
    void Start()
    {

        //In Jail Card
        inJailCardPanel = GameObject.Find("InJailCard");
        inJailCardPanel.SetActive(false);

        // Button info
        buttonInfo = GameObject.Find("ButtonInfo");
        buttonInfo.SetActive(false);

        // Utilities
        utilityPanel = GameObject.Find("UtilityCard");
        electricCompanyLogo = GameObject.Find("ElectricCompanyLogo");
        electricCompanyLogo.SetActive(false);
        waterWorksLogo = GameObject.Find("WaterWorksLogo");
        waterWorksLogo.SetActive(false);
        utilityPanel.SetActive(false);

        // Railroad Card
        railroadPanel = GameObject.Find("RailroadCard");
        railroadPanel.SetActive(false);

        // Property Card
        cardPanel = GameObject.Find("Card");
        buyPropertyButton = GameObject.Find("BuyProperty");
        buyPropertyButton.SetActive(false);
        cancelButton = GameObject.Find("Cancel");
        cancelButton.SetActive(false);
        closeButton = GameObject.Find("Close");
        closeButton.SetActive(false);
        payRentButton = GameObject.Find("PayRent");
        payRentButton.SetActive(false);

        // Owned Card
        sellPropertyButton = GameObject.Find("SellProperty");
        sellPropertyButton.SetActive(false);
        sellHouseButton = GameObject.Find("SellHouse");
        sellHouseButton.SetActive(false);
        buyHouseButton = GameObject.Find("BuyHouse");
        buyHouseButton.SetActive(false);
        cardPanel.SetActive(false);

        // Event Card
        eventPanel = GameObject.Find("EventCard");
        closeEventButton = GameObject.Find("CloseEvent");
        //closeEventButton.SetActive(false);
        comunityChestLogo = GameObject.Find("ComunityChestLogo");
        comunityChestLogo.SetActive(false);
        chanceLogo = GameObject.Find("ChanceLogo");
        chanceLogo.SetActive(false);
        eventPanel.SetActive(false);

        canvas = GameObject.Find("Canvas");

        // Trade
        tradeButton = GameObject.Find("TradeButton");
        tradeButton.GetComponent<Button>().onClick.AddListener(openPlayerTradePanel);
        tradeButton.SetActive(false);
        playerTradePanel = GameObject.Find("playerTradePanel");
        playerTradePanel.SetActive(false);

        // Console
        console = GameObject.Find("Console");
        console.SetActive(false);

        // Start Game
        startGameButton = GameObject.Find("StartGame");
        startGameButton.SetActive(false);

        // Roll Button
        rollButton = GameObject.Find("RollDice");
        rollButton.SetActive(false);

        // End Turn Button
        endTurnButton = GameObject.Find("EndTurn");
        endTurnButton.SetActive(false);

        // Properties Panel
        ownedPropertiesPanel = GameObject.Find("OwnedProprietiesPanel");

    }

    private static void openPlayerTradePanel()
    {
        if (GameObject.Find("TradePanel")) return;
        tradeButton.GetComponent<Button>().onClick.RemoveAllListeners();
        tradeButton.GetComponent<Button>().onClick.AddListener(closePlayerTradePanel);
        playerTradePanel.SetActive(true);
        SoundManager.Instance.PlaySound(SoundManager.Instance.selectProperty);
    }

    public static void closePlayerTradePanel()
    {
        tradeButton.GetComponent<Button>().onClick.RemoveAllListeners();
        tradeButton.GetComponent<Button>().onClick.AddListener(openPlayerTradePanel);
        playerTradePanel.SetActive(false);
        SoundManager.Instance.PlaySound(SoundManager.Instance.close);
    } 

    private void writeButtonInfo(string info)
    {
        buttonInfo.SetActive(true);
        buttonInfo.transform.GetChild(0).GetComponent<Text>().text = info;
    }

    public void removeButtonInfo()
    {
        buttonInfo.SetActive(false);
    }

}
