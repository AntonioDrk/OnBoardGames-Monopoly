using System;
using UnityEngine;

public class Die : MonoBehaviour {

    public int diceNumber;
    private Collider myCollider;
    private Rigidbody rb; 
    private DiceManager diceManager;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        diceManager = GameObject.Find("DiceManager").GetComponent<DiceManager>();
    }

    void OnEnable()
    {
        transform.parent.parent = GameObject.Find("DiceManager").transform;
    }

    public void OnCollisionEnter(Collision collision)
    {
        if(rb.velocity.x < 0.01 && rb.velocity.y < 0.01 && rb.velocity.z < 0.01 )
        {
            rb.velocity = Vector3.zero;

            int index = collision.contacts.Length - 1; 
            myCollider = collision.contacts[index].thisCollider;
            
            if (myCollider.CompareTag("1") || myCollider.CompareTag("2") || myCollider.CompareTag("3") || myCollider.CompareTag("4")
                || myCollider.CompareTag("5") || myCollider.CompareTag("6"))
            {
                int rolledValue = 7 - Int32.Parse(myCollider.tag);
                diceManager.CmdAddDice(rolledValue); 
            } 
                
        }

    }

}
