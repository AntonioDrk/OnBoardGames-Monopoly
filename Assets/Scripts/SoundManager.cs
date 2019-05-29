using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    private static SoundManager _instance;

    public AudioClip buyHouse;
    public AudioClip close;
    public AudioClip completeTrade;
    public AudioClip endTurn;
    public AudioClip getMoney;
    public AudioClip payMoney;
    public AudioClip openCard;
    public AudioClip openEvent;
    public AudioClip goToJail;
    public AudioClip receiveTradeOffer;
    public AudioClip selectProperty;

    private GameObject manager { get { return GameObject.Find("SoundManager"); } }
    private AudioSource source { get { return manager.GetComponent<AudioSource>(); } }

    // Start is called before the first frame update
    void Start()
    {
        manager.AddComponent<AudioSource>();
        source.clip = selectProperty;
        source.playOnAwake = false;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public static SoundManager Instance { get { return _instance; } }

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            _instance = this;
        }
    }

    public void PlaySound(AudioClip givenSound)
    {
        source.PlayOneShot(givenSound);
    }
}
