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
    
    static public PropertyCard[] propertyCards;

    // Start is called before the first frame update
    void Start()
    {
        LoadPropertyCards(propertiesFile);
        LoadRailwayCards(railroadsFile);
        LoadUtilityCards(utilitiesFile);
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
            RailwayCard[] loadedData = JsonHelper.FromJson<RailwayCard>(railwaysJson);

            Debug.Log(fileName + " data retrieved. " + loadedData.Length + " cards loaded.");

            /*for (int i = 0; i < loadedData.Length; i++)
                Debug.Log(loadedData[i].ToString());*/
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
            UtilityCard[] loadedData = JsonHelper.FromJson<UtilityCard>(utilitiesJson);
             
            Debug.Log(fileName + " data retrieved. " + loadedData.Length + " cards loaded.");

            /*for (int i = 0; i < loadedData.Length; i++)
                Debug.Log(loadedData[i].ToString());*/
        }
        else
        {
            Debug.LogError("Cannot load file:" + fileName);
        }
    }
}
