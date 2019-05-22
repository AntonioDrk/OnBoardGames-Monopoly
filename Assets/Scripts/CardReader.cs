using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;
using UnityEngine;
using UnityEngine.UI;

public class CardReader : MonoBehaviour
{
    string propertiesFile = "Data files/Properties.json";
    string railroadsFile = "Data files/Railroads.json";
    string utilitiesFile = "Data files/Utilities.json";
    string chanceFile = "Data files/Chance.json";
    string chestFile = "Data files/CommunityChest.json";

    static public PropertyCard[] propertyCards;
    static public RailroadCard[] railroadCards;
    static public UtilityCard[] utilityCards;
    static public EventCard[] chanceCards;
    static public EventCard[] chestCards;

    static public GameObject cardPanel, buyPropertyButton, cancelButton, payRentButton, closeButton, closeEventButton, eventPanel,
                                ComunityChestLogo, ChanceLogo, sellPropertyButton, sellHouseButton, buyHouseButton, railroadPanel,
                                utilityPanel, ElectricCompanyLogo, WaterWorksLogo, buttonInfo;

    // Start is called before the first frame update
    void Start()
    {
        LoadPropertyCards(propertiesFile);
        LoadRailroadCards(railroadsFile);
        LoadUtilityCards(utilitiesFile);
        LoadChanceCards(chanceFile);
        LoadChestCards(chestFile);

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
    }

    // Update is called once per frame
    void Update()
    { 

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
        string filePath = Path.Combine(Application.dataPath, fileName);

        if (File.Exists(filePath))
        {
            string propertiesJson = File.ReadAllText(filePath);
            propertyCards = JsonHelper.FromJson<PropertyCard>(propertiesJson);

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
        string filePath = Path.Combine(Application.dataPath, fileName);

        if (File.Exists(filePath))
        {
            string railwaysJson = File.ReadAllText(filePath);
            railroadCards = JsonHelper.FromJson<RailroadCard>(railwaysJson);

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
        string filePath = Path.Combine(Application.dataPath, fileName);

        if (File.Exists(filePath))
        {
            string utilitiesJson = File.ReadAllText(filePath);
            utilityCards = JsonHelper.FromJson<UtilityCard>(utilitiesJson);

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
        string filePath = Path.Combine(Application.dataPath, fileName);

        if (File.Exists(filePath))
        {
            string chanceJson = File.ReadAllText(filePath);
            chanceCards = JsonHelper.FromJson<EventCard>(chanceJson);

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
        string filePath = Path.Combine(Application.dataPath, fileName);

        if (File.Exists(filePath))
        {
            string chestJson = File.ReadAllText(filePath);
            chestCards = JsonHelper.FromJson<EventCard>(chestJson);

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
