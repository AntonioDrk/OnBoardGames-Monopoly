using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{

    
    private Camera mainCamera;
    // THIS IS TESTING ONLY
    // This is bad v and needs to be changed ASAP, we will pass the players vector between the scripts
    [SerializeField]
    private GameObject player;
    [SerializeField]
    private Vector3 cameraDistance = new Vector3(0, -2.8f, 2.9f);
    

    void Start()
    {
        mainCamera = Camera.main;

        if(mainCamera == null)
        {
            Debug.LogError("Main Camera not found! Make sure it's tagged as main camera");
        }

        // GET RID OF THIS CHECK AFTER TESTING
        if(player == null)
        {
            Debug.LogError("Target player is not set !!!");
        }
    }

    
    void Update()
    {
        UpdatePosCamera(player);
    }

    // Moves the camera to face the player
    void UpdatePosCamera(GameObject target)
    {
        mainCamera.transform.position = target.transform.position - cameraDistance;
    }
}
