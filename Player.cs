using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using TMPro;

public struct PlayerScore
{
    public int captainAreaScore; //нахождение в области рулевого
    public int captainScore;

    public int fixerAreaScore; //нахождение в области инженера
    public int fixerScore;

    public int stokerAreaScore; //нахождение в области кочегара
    public int stokerScore; 

    public int movingScore; //движение

    public int communicationScore; //чат

    public int inactionScore; //бездействие
}

public class Player : NetworkBehaviour
{
    public float speed = 10.0f;
    private float moveInput;
    private float moveInputJump;
    private float jumpForce = 5f;

    [SyncVar(hook=nameof(OnFlip))]
    private bool facingRight = true;

    public GameObject inventory;
    [SyncVar(hook=nameof(DisplayName))] public string playerName = "Player";
    public TextMeshProUGUI nameText;

    private Rigidbody2D rb;
    private GameObject canvas;
    public DataHold DH;

    public NetworkIdentity netID;
    public NetworkManagerEdited netman;
    //(requiresAuthority = false)

    private float timeSinceLastScoreUpdate = 0f;
    private float timeBetweenScoreUpdates = 1f;

    public PlayerScore score;

    public override void OnStartLocalPlayer()
    {
        Camera.main.GetComponent<CameraController>().setTarget(gameObject.transform);
    }

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        canvas = gameObject.transform.Find("Canvas").gameObject;
        netID = NetworkClient.connection.identity;
        netman = netID.GetComponent<NetworkManagerEdited>();

        DH = GameObject.Find("DataHolder").GetComponent<DataHold>();

        if (!isLocalPlayer) return;
        CmdSendName(DH.player_Name);
    }

    [Command]
    public void CmdSendName(string pName)
    {
        playerName = pName;
    }

    public void DisplayName(string oldName, string newName)
    {
        nameText.text = newName;
    }

    // Update is called once per frame
    void Update()
    {
        if (!hasAuthority) return;
        moveInput = Input.GetAxisRaw("Horizontal");//при нажатии клавиш персонаж будет двигаться по горизонтали. если вправо, то moveInput = 1, иначе -1
        if (facingRight == false && moveInput > 0)
        {
            CmdFlip();
        }
        else if (facingRight == true && moveInput < 0)
        {
            CmdFlip();
        }
        if (timeSinceLastScoreUpdate >= timeBetweenScoreUpdates)
        {
            switch(moveInput){
                case 1:
                    score.movingScore += 1;
                    break;
                case -1:
                    score.movingScore += 1;
                    break;
                default:
                    //score.inactionScore += 1;
                    break;
            }
            timeSinceLastScoreUpdate = 0;
            //SendScoreToDH(score);
        }

        rb.velocity = new Vector2(moveInput * speed, rb.velocity.y);
        if (Input.GetKeyDown(KeyCode.W)) rb.velocity = Vector2.up * jumpForce; //прыжок

        timeSinceLastScoreUpdate += Time.deltaTime; //личный таймер
    }

    public override void OnStopClient()
    {
        SendScoreToDH(score);
    }

    [Command]
    void CmdFlip()
    {
        facingRight = !facingRight;
    }

    public void OnFlip(bool oldFlip, bool newFlip)
    {
        Transform behind = transform.Find("Behinds");
        Vector3 Scaler = behind.transform.localScale;
        Scaler.x *= -1;
        behind.transform.localScale = Scaler;
    }

    public void ItemUsed(GameObject itemReciever)
    {
        ItemRecieverBehav IRB = itemReciever.GetComponent<ItemRecieverBehav>();
        ItemBehav IB = inventory.GetComponent<ItemBehav>();
        if ((inventory != null) && (IRB.neededItem == IB.itemName))
        {
            CmdDestroy(inventory);
            CmdDestroy(itemReciever);
            if ((IB.itemName == "planks") || (IB.itemName == "toolbox"))
            {
                score.fixerScore += 1;
            } else
            {
                score.stokerScore += 1;
            }
            inventory = null;
        } else Debug.Log("nah fam");
    }

    [Command]
    public void CmdDestroy(GameObject toDestroy)
    {
        Destroy(toDestroy);
    }

    public void ItemPickedUp(GameObject clickedItem)
    {
        ItemBehav itemBehav = clickedItem.GetComponent<ItemBehav>();
        if (!itemBehav.wasClicked)
        {
            //добавка в инвентарь
            if (inventory == null)
            {
                itemBehav.index = 0;
                inventory = clickedItem;
            }
            else return; //если нет свободного места в инвентаре - досвидоооос

            itemBehav.wasClicked = true;

            //перемещение
            string behindName = "BehindPlayer" + itemBehav.index;
            itemBehav.behindPlayer = transform.Find("Behinds").transform.Find(behindName).gameObject;
            clickedItem.transform.position = itemBehav.behindPlayer.transform.position;
        }
    }

    public void RecordScore(string zoneName, int zonePoint)
    {
        if (zoneName == "CaptainZone")
        {
            score.captainAreaScore += zonePoint;
        } else if (zoneName == "FixerZone")
        {
            score.fixerAreaScore += zonePoint;
        } else if (zoneName == "Message")
        {
            score.communicationScore += zonePoint;
        }
        //Debug.Log("Score:/nОбласть капитана = " + score.captainAreaScore + "/nОбласть инженера = " + score.fixerAreaScore + "/nПочинка = " + score.fixerScore + "/nДвижение = " + score.movingScore + "/nЧат = " + score.communicationScore + "/nБездействие = " + score.inactionScore);
    }

    public void SendScoreToDH(PlayerScore plSc)
    {
        DH.playerScore = plSc;
    }
    /*public void OnTriggerEnter2D(Collider2D other)
    {
        //Debug.Log("Works!"); //реально работает о.о 
    }*/
    }
