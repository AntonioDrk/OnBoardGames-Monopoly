using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

public class GameManager : NetworkBehaviour
{  

    public bool targetPlayerIsMoving = false;
    public GameObject targetPlayer; 

    private Camera mainCamera;

    // THIS IS TESTING ONLY
    // This is bad v and needs to be changed ASAP, we will pass the players vector between the scripts 
    [SerializeField]
    private Vector3 cameraDistance = new Vector3(0, -3.5f, 3f);
    [SerializeField]
    // the position of the camera when the target player is not moving
    private Vector3 cameraPosition = new Vector3(-5f, 8f, -9.5f); 
    [SerializeField]
    private Text playerTurnText, connectedPlayersText;

    [SyncVar] public int playerTurn = 1; 
    [SyncVar] public int connectedPlayers = 0;

    void Start()
    { 
        
        mainCamera = Camera.main;

        connectedPlayersText = GameObject.Find("connectedPlayersText").GetComponent<Text>();
        playerTurnText = GameObject.Find("playerTurnText").GetComponent<Text>(); 
        playerTurnText.text = "Turn: Player " + playerTurn;
    }

    
    void Update()
    {
        playerTurnText.text = "Turn: Player " + playerTurn;
        connectedPlayersText.text = connectedPlayers + " players"; 

        if (targetPlayerIsMoving)
            UpdatePosCamera(targetPlayer);
        else
        {
            mainCamera.transform.position = cameraPosition;
            mainCamera.transform.eulerAngles = new Vector3(mainCamera.transform.eulerAngles.x, 0, 0);
        }
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
