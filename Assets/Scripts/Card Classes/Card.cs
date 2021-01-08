using UnityEngine;
using UnityEngine.UI;

//[Serializable]
public abstract class Card
{
    private int id;
    private string cardName;
    private int price;
    private int mortgage; 

    public int Id { get => id; set => id = value; }
    public string CardName { get => cardName; set => cardName = value; }
    public int Price { get => price; set => price = value; }
    public int Mortgage { get => mortgage; set => mortgage = value; }

    public abstract void doAction(GameObject player);
    public abstract void showOwnedCard(GameObject player);

    protected virtual void hideCard(GameObject player)
    {
        // Stuff to be called at the end
        player.GetComponent<Player>().ViewingCard = false;
    }
    
    protected virtual void buyCard(GameObject player)
    {
        Player playerScript = player.GetComponent<Player>();
        SoundManager.Instance.PlaySound(SoundManager.Instance.payMoney);
        playerScript.CmdTakeMoney(Price);
        playerScript.buyProperty(this);
    }
    protected virtual void sellCard(GameObject player)
    {
        UIManager.sellPropertyButton.GetComponent<Button>().onClick.RemoveAllListeners();
        UIManager.sellPropertyButton.SetActive(false);
        Player playerScript = player.GetComponent<Player>();
        playerScript.CmdAddMoney(Mortgage); // players get in return the card's mortgage value
        playerScript.sellProperty(this);
    }
}
