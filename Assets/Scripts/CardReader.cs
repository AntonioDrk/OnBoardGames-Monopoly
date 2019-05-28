using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;
using UnityEngine;
using UnityEngine.UI;

public class CardReader : MonoBehaviour
{
    string propertiesFile = "Properties";
    string railroadsFile = "Railroads";
    string utilitiesFile = "Utilities";
    string chanceFile = "Chance";
    string chestFile = "CommunityChest";
    string folderLocation = "Data files";

    static public PropertyCard[] propertyCards;
    static public RailroadCard[] railroadCards;
    static public UtilityCard[] utilityCards;
    static public EventCard[] chanceCards;
    static public EventCard[] chestCards;

    static public GameObject cardPanel, buyPropertyButton, cancelButton, payRentButton, closeButton, closeEventButton, eventPanel,
                                ComunityChestLogo, ChanceLogo, sellPropertyButton, sellHouseButton, buyHouseButton, railroadPanel,
                                utilityPanel, ElectricCompanyLogo, WaterWorksLogo, buttonInfo, canvas, inJailCardPanel, playerTradePanel,
                                tradeButton;

    static public GameObject housePrefab, hotelPrefab;

    // Start is called before the first frame update
    void Start()
    {
        LoadPropertyCards(propertiesFile);
        LoadRailroadCards(railroadsFile);
        LoadUtilityCards(utilitiesFile);
        LoadChanceCards(chanceFile);
        LoadChestCards(chestFile);
        LoadHousePrefab();

        //In Jail Card
        inJailCardPanel = GameObject.Find("InJailCard");
        inJailCardPanel.SetActive(false);

        // Button info
        buttonInfo = GameObject.Find("ButtonInfo");
        buttonInfo.SetActive(false);

        // Utilities
        utilityPanel = GameObject.Find("UtilityCard");
        ElectricCompanyLogo = GameObject.Find("ElectricCompanyLogo");
        ElectricCompanyLogo.SetActive(false);
        WaterWorksLogo = GameObject.Find("WaterWorksLogo");
        WaterWorksLogo.SetActive(false);
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
        ComunityChestLogo = GameObject.Find("ComunityChestLogo");
        ComunityChestLogo.SetActive(false);
        ChanceLogo = GameObject.Find("ChanceLogo");
        ChanceLogo.SetActive(false);
        eventPanel.SetActive(false);

        canvas = GameObject.Find("Canvas");
        tradeButton = GameObject.Find("TradeButton");
        tradeButton.GetComponent<Button>().onClick.AddListener(openPlayerTradePanel);
        tradeButton.SetActive(false);
        playerTradePanel = GameObject.Find("playerTradePanel");
        playerTradePanel.SetActive(false);

    }

    static public void openPlayerTradePanel()
    {
        if (GameObject.Find("TradePanel")) return;
        tradeButton.GetComponent<Button>().onClick.RemoveAllListeners();
        tradeButton.GetComponent<Button>().onClick.AddListener(closePlayerTradePanel);
        playerTradePanel.SetActive(true);
    }

    static public void closePlayerTradePanel()
    {
        tradeButton.GetComponent<Button>().onClick.RemoveAllListeners();
        tradeButton.GetComponent<Button>().onClick.AddListener(openPlayerTradePanel);
        playerTradePanel.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void LoadHousePrefab()
    {
        housePrefab = Resources.Load<GameObject>("House") as GameObject;
        //hotelPrefab = Resources.Load<GameObject>("Hotel.prefab");
        if (housePrefab == null ) Debug.LogError("House/hotel object not found in the resources folder!");
    }

    static public int getPropertyCardIndex(int id)
    {
        for (int i = 0; i < propertyCards.Length; i++)
            if (propertyCards[i].Id == id)
                return i;
        return -1;
    }

    public void LoadPropertyCards(string fileName)
    {
        string filePath = Path.Combine("Data files", fileName);
        TextAsset propertiesJson = Resources.Load<TextAsset>(filePath);

        if (propertiesJson != null)
        {
            propertyCards = JsonHelper.FromJson<PropertyCard>(propertiesJson.ToString());

            Debug.Log(fileName + " data retrieved. " + propertyCards.Length + " cards loaded.");

            for (int i = 0; i < propertyCards.Length; i++)
                propertyCards[i].PropertyCardConstructor();
        }
        else
        {
            Debug.LogError("Cannot load file:" + fileName);
        }

    }

    public void LoadRailroadCards(string fileName)
    {
        string filePath = Path.Combine(folderLocation, fileName);
        TextAsset railwaysJson = Resources.Load<TextAsset>(filePath);

        if (railwaysJson != null)
        {
            railroadCards = JsonHelper.FromJson<RailroadCard>(railwaysJson.ToString());

            Debug.Log(fileName + " data retrieved. " + railroadCards.Length + " cards loaded.");

            for (int i = 0; i < railroadCards.Length; i++)
                railroadCards[i].RailroadCardConstructor();
        }
        else
        {
            Debug.LogError("Cannot load file:" + fileName);
        }
    }

    public void LoadUtilityCards(string fileName)
    {
        string filePath = Path.Combine(folderLocation, fileName);
        TextAsset utilitiesJson = Resources.Load<TextAsset>(filePath);

        if (utilitiesJson != null)
        {
            utilityCards = JsonHelper.FromJson<UtilityCard>(utilitiesJson.ToString());

            Debug.Log(fileName + " data retrieved. " + utilityCards.Length + " cards loaded.");

            for (int i = 0; i < utilityCards.Length; i++)
                utilityCards[i].UtilityCardConstructor();
        }
        else
        {
            Debug.LogError("Cannot load file:" + fileName);
        }
    }

    public void LoadChanceCards(string fileName)
    {
        string filePath = Path.Combine(folderLocation, fileName);
        TextAsset chanceJson = Resources.Load<TextAsset>(filePath);

        if (chanceJson != null)
        {
            chanceCards = JsonHelper.FromJson<EventCard>(chanceJson.ToString());
            
            Debug.Log(fileName + " data retrieved. " + chanceCards.Length + " cards loaded.");

            for (int i = 0; i < chanceCards.Length; i++)
                chanceCards[i].EventCardConstructor();

            //for (int i = 0; i < chanceCards.Length; i++)
            // Debug.Log(chanceCards[i].ToString());
        }
        else
        {
            Debug.LogError("Cannot load file:" + fileName);
        }
    }

    public void LoadChestCards(string fileName)
    {
        string filePath = Path.Combine(folderLocation, fileName);
        TextAsset chestJson = Resources.Load<TextAsset>(filePath);

        if (chestJson != null)
        {
            chestCards = JsonHelper.FromJson<EventCard>(chestJson.ToString());
            
            Debug.Log(fileName + " data retrieved. " + chestCards.Length + " cards loaded.");

            for (int i = 0; i < chestCards.Length; i++)
                chestCards[i].EventCardConstructor();

            //for (int i = 0; i < chestCards.Length; i++)
            // Debug.Log(chestCards[i].ToString());
        }
        else
        {
            Debug.LogError("Cannot load file:" + fileName);
        }
    }

    public void writeButtonInfo(string info)
    {
        buttonInfo.SetActive(true);
        buttonInfo.transform.GetChild(0).GetComponent<Text>().text = info;
    }

    public void removeButtonInfo()
    {
        buttonInfo.SetActive(false);
    }
}
