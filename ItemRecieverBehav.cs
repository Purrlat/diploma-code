using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class ItemRecieverBehav : NetworkBehaviour
{
    public string neededItem;

    //GameObject playerToCheck;
    //Player playerComponent;

    public NetworkManagerEdited netman;

    public Sprite[] spriteArray;

    void Start()
    {
        if (neededItem == "StokerItem")
            this.gameObject.GetComponent<SpriteRenderer>().sprite = spriteArray[0];
        if (neededItem == "FireWater")
            this.gameObject.GetComponent<SpriteRenderer>().sprite = spriteArray[1];
        if (neededItem == "Instruments")
            this.gameObject.GetComponent<SpriteRenderer>().sprite = spriteArray[2];
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.tag == "Player")
        {
            Debug.Log("click!");
            Player player = other.GetComponent<Player>();
            player.ItemUsed(gameObject);
        }
    }
}
