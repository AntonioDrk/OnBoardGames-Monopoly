using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

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

    static public GameObject housePrefab, hotelPrefab;
     
    void Start()
    {
        LoadPropertyCards(propertiesFile);
        LoadRailroadCards(railroadsFile);
        LoadUtilityCards(utilitiesFile);
        LoadChanceCards(chanceFile);
        LoadChestCards(chestFile);
        LoadHousePrefab();
    }

    static public int getIdOnBoard(int i)
    { 
        if (i < 22) return propertyCards[i].id;
        if (i < 26) return railroadCards[i - 22].id;
        return utilityCards[i - 26].id;
    }

    static public int getPropertyCardIndex(int id)
    {
        for (int i = 0; i < propertyCards.Length; i++)
            if (propertyCards[i].Id == id)
                return i;
        return -1;
    }

    private void LoadHousePrefab()
    {
        housePrefab = Resources.Load<GameObject>("House") as GameObject;
        if (housePrefab == null)
            Debug.LogError("House/hotel object not found in the resources folder!");
    }

    private void LoadPropertyCards(string fileName)
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

    private void LoadRailroadCards(string fileName)
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

    private void LoadUtilityCards(string fileName)
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

    private void LoadChanceCards(string fileName)
    {
        string filePath = Path.Combine(folderLocation, fileName);
        TextAsset chanceJson = Resources.Load<TextAsset>(filePath);

        if (chanceJson != null)
        {
            chanceCards = JsonHelper.FromJson<EventCard>(chanceJson.ToString());

            Debug.Log(fileName + " data retrieved. " + chanceCards.Length + " cards loaded.");

            for (int i = 0; i < chanceCards.Length; i++)
                chanceCards[i].EventCardConstructor();
        }
        else
        {
            Debug.LogError("Cannot load file:" + fileName);
        }
    }

    private void LoadChestCards(string fileName)
    {
        string filePath = Path.Combine(folderLocation, fileName);
        TextAsset chestJson = Resources.Load<TextAsset>(filePath);

        if (chestJson != null)
        {
            chestCards = JsonHelper.FromJson<EventCard>(chestJson.ToString());

            Debug.Log(fileName + " data retrieved. " + chestCards.Length + " cards loaded.");

            for (int i = 0; i < chestCards.Length; i++)
                chestCards[i].EventCardConstructor();
        }
        else
        {
            Debug.LogError("Cannot load file:" + fileName);
        }
    }

}
