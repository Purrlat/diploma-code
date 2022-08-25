using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;

public class ItemBehav : NetworkBehaviour
{
    [SerializeField]
    public float moveSpeed = 0.01f;

    [SerializeField]
    float frequency = 2f;

    [SerializeField]
    float magnitude = 0.02f;

    [SerializeField]
    public string itemName;

    public bool wasClicked = false;

    //GameObject currentOwner;
    public GameObject behindPlayer;
    public Player player;
    public int index;
    public Sprite[] spriteArray;

    Vector3 pos;

    // Start is called before the first frame update
    void Start()
    {
        if (itemName == "StokerItem")
            this.gameObject.GetComponent<SpriteRenderer>().sprite = spriteArray[0];
        if (itemName == "FireWater")
            this.gameObject.GetComponent<SpriteRenderer>().sprite = spriteArray[1];
        if (itemName == "Instruments")
            this.gameObject.GetComponent<SpriteRenderer>().sprite = spriteArray[2];
    }

    // Update is called once per frame
    void Update()
    {
        if ((!wasClicked) || (player ==null))
        {
            return;
        }
        transform.position += transform.up * Mathf.Sin(Time.time * frequency + index) * magnitude;
        transform.position = Vector3.MoveTowards(transform.position, behindPlayer.transform.position, moveSpeed);
        
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.tag == "Player")
        {
            Debug.Log("click!");
            player = other.GetComponent<Player>();
            player.ItemPickedUp(gameObject);
        }
    }
}
