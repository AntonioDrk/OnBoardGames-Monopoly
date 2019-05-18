using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;
using UnityEngine;

public class CardReader : MonoBehaviour
{
    string propertiesFile = "Data files/Properties.json";
    string railroadsFile = "Data files/Railroads.json";
    string utilitiesFile = "Data files/Utilities.json";
    string chanceFile = "Data files/Chance.json";
    string chestFile = "Data files/CommunityChest.json";

    static public PropertyCard[] propertyCards;
    static public RailwayCard[] railwayCards;
    static public UtilityCard[] utilityCards;
    static public EventCard[] chanceCards;
    static public EventCard[] chestCards;

    static public GameObject cardPanel, buyPropertyButton, cancelButton, payRentButton, closeButton, closeEventButton, eventPanel,
                                ComunityChestLogo, ChanceLogo, sellPropertyButton, sellHouseButton, buyHouseButton;

    // Start is called before the first frame update
    void Start()
    {
        LoadPropertyCards(propertiesFile);
        LoadRailwayCards(railroadsFile);
        LoadUtilityCards(utilitiesFile);
        LoadChanceCards(chanceFile);
        LoadChestCards(chestFile);

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

        // Owned Property Card
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

            /*for (int i = 0; i < propertyCards.Length; i++)
                Debug.Log(propertyCards[i].ToString());*/
        }
        else
        {
            Debug.LogError("Cannot load file:" + fileName);
        }

    }
    
    public void LoadRailwayCards(string fileName)
    {
        string filePath = Path.Combine(Application.dataPath, fileName);

        if (File.Exists(filePath))
        {
            string railwaysJson = File.ReadAllText(filePath);
            railwayCards = JsonHelper.FromJson<RailwayCard>(railwaysJson);

            Debug.Log(fileName + " data retrieved. " + railwayCards.Length + " cards loaded.");

            for (int i = 0; i < railwayCards.Length; i++)
                railwayCards[i].RailwayCardConstructor();

            //for (int i = 0; i < railwayCards.Length; i++)
                //Debug.Log(railwayCards[i].ToString());
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

            //for (int i = 0; i < utilityCards.Length; i++)
                //Debug.Log(utilityCards[i].ToString());
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
}
