using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    // Serialized just for testing purposes, remove me
    [SerializeField]
    private int indexPosition = 0; // Indicates the position on the board list (the list of cards that are on the board)

    [SerializeField]
    private float movementSteps = 0.5f;

    [SerializeField]
    private Transform[] proprietiesTransforms = new Transform[0];

    [SerializeField]
    private Animator anim;

    int numberOfMoves = 0;

    void Start()
    {
        anim = GetComponent<Animator>();
        if(anim == null)
        {
            Debug.LogError("Didn't find the animator component on the player object !");
        }
        StartCoroutine("animateMovement",15);
        //animateMovement(15);
    }

    void Update()
    { 
        if (Input.GetKeyUp(KeyCode.Space))
        {
            StartCoroutine("movePawn",11);
        }
        
    }

    IEnumerator animateMovement(int amountToMove)
    {
        for (int i = 0; i < amountToMove; i++)
        {
            anim.Play("StraightMovement", 0);
            indexPosition = (indexPosition + 1) % 40;
            if (indexPosition % 10 == 0)
            {
                anim.Play("RotationAnimation", 0);
            }
            yield return new WaitForSeconds(anim.GetCurrentAnimatorStateInfo(0).length + anim.GetCurrentAnimatorStateInfo(0).normalizedTime - .5f);   
        }
        
    }

    void callBackMovementStopped()
    {
        if (numberOfMoves > 0)
        {
            numberOfMoves--;
            animateMovement(numberOfMoves);
        }
    }

    IEnumerator movePawn(int tilesToMove)
    {   // Iterate through each tile to move ( taking it step by step )
        for(int i = 1; i <= tilesToMove; i++)
        {
            // If we're in a corner rotate ( this will be necesary when the pawns have special models
            // So the models look forward
            if (indexPosition % 10 == 0)
            {
                transform.eulerAngles += new Vector3(0, 90, 0);
            }
            // Get the distance between our current position and our next's waypoint position
            float distance = Vector3.Distance(transform.position, proprietiesTransforms[i-1].position);
            // We divide our distance in "steps", so it looks like it's animated, each step is incremented by time
            for (float step = 0; step < distance; step += movementSteps)
            {
                Debug.Log("Step is : " + step + "\nDistance is: " + distance);
                transform.position = Vector3.MoveTowards(transform.position, proprietiesTransforms[i-1].position, step);
                yield return null;
            }

            indexPosition = (indexPosition + 1) % 40;

            yield return new WaitForSeconds(0.25f);
        }
    }

    


    /*  REALLY BAD STUFF
    void movePlayer(int noTiles)
    {
        int targetIndex = (indexPosition + noTiles) % 40;
        float distance = Vector3.Distance(transform.position, proprietiesTransforms[0].position);

        for (float step = 0; step < distance; step += 0.0005f*Time.deltaTime)
        {
            Debug.Log("Step is : " + step + "\nDistance is: " + distance);
            transform.position = Vector3.MoveTowards(transform.position, proprietiesTransforms[0].position, step);
        }

        /*for(int i = 1; i <= noTiles; i++)
        {
            while(Vector3.Distance(transform.position,proprietiesTransforms[(i + indexPosition)%40].position) > 0.5)
            {
                float step = speed * Time.deltaTime;
                transform.position = Vector3.MoveTowards(transform.position, proprietiesTransforms[0].position, speed);
            }
        }
    }

    void updateMovement(int amountToMove)
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
*/
}
