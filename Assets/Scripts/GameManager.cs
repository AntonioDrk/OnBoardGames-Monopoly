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
        // Get the distance the camera should be from the player (the distance is set in cameraDistance vector)
        // rotation * cameraDistance makes the camera be placed with respect to thge player rotation, so it always stays "BEHIND" the player
        mainCamera.transform.position = target.transform.position - target.transform.rotation * cameraDistance;

        // Take the targets rotation on the y axis and apply it to the camera, don't change the cameras x axis rotation
        mainCamera.transform.eulerAngles = new Vector3(mainCamera.transform.eulerAngles.x, target.transform.eulerAngles.y, 0);
        
    }
}
