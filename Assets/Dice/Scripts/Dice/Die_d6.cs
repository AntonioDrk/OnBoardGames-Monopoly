using System;
using UnityEngine;
using System.Collections.Generic;

public class Die_d6 : Die {

    public int diceNumber;
    private Collider myCollider;
    private Rigidbody rb;
    private DiceScript diceScript;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        diceScript = transform.parent.gameObject.GetComponent<DiceScript>();
    }

    void OnEnable()
    {
        transform.position = new Vector3(transform.position.x, 5.5f, transform.position.z);
        transform.eulerAngles = new Vector3(90 * UnityEngine.Random.Range(0, 4), 90 * UnityEngine.Random.Range(0, 4), 90 * UnityEngine.Random.Range(0, 4));
    }

    public void OnCollisionEnter(Collision collision)
    {
        if(rb.velocity.x < 0.01 && rb.velocity.y < 0.01 && rb.velocity.z < 0.01 )
        {
            rb.velocity = Vector3.zero;

            int index = collision.contacts.Length - 1; 
            myCollider = collision.contacts[index].thisCollider; 

            if(myCollider.tag == "1" || myCollider.tag == "2" || myCollider.tag == "3" || myCollider.tag == "4"
                || myCollider.tag == "5" || myCollider.tag == "6")
            {
                int rolledValue = 7 - Int32.Parse(myCollider.tag);
                Debug.Log("Dice " + diceNumber + " rolled " + rolledValue);
                diceScript.addDice(rolledValue); 
            } 
                
        }

    }

}
