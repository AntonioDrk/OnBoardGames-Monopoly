using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    [SerializeField]
    private int indexPosition = 0; // Indicates the position on the board list (the list of cards that are on the board)
    private float moveAmount = 1.25f;
    private Vector3 targetPos;
    private int amountToMove = 1;
    private float speedMovement = 20f;


    void Start()
    {
        targetPos = transform.position;
    }

    void Update()
    {
        
    }

    void FixedUpdate()
    {
        if (amountToMove < 0)
        {
            return;
        }

        for (int i = 1; i <= amountToMove; i++)
        {
            Vector3 localForward = transform.worldToLocalMatrix.MultiplyVector(transform.forward);
            transform.Translate(localForward * i * moveAmount * Time.fixedDeltaTime);
            //transform.position = Vector3.Lerp(transform.position, localForward * i * moveAmount, Time.fixedDeltaTime);
            //Vector3.MoveTowards(transform.position, localForward * i * moveAmount);
            if ((i + indexPosition) % 10 == 0)
            {
                //Debug.Log("Sunt in if for some reason");
                transform.eulerAngles += new Vector3(0, 90, 0);
            }
        }
        indexPosition = (indexPosition + amountToMove) % 40;
    }

    void updateMovement(int amountToMove)
    {
     
        
    }
}
