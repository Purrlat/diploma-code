using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class ZoneBehav : MonoBehaviour
{
    public string zoneName;
    public int zonePointPerSecond;

    private NetworkIdentity netID;

    private float timeSinceLastScoreUpdate = 0f;
    private float timeBetweenScoreUpdates = 1f;

    public void Update()
    {
        timeSinceLastScoreUpdate += Time.deltaTime;
    }

    public void OnTriggerEnter2D(Collider2D other)
    {
        netID = other.GetComponent<NetworkIdentity>();
        if (other.tag == "Player")
        {
            //Debug.Log(netID.netId + " Entered zone");
        }
    }

    public void OnTriggerStay2D(Collider2D other)
    {
        netID = other.GetComponent<NetworkIdentity>();
        if (other.tag == "Player")
        {
            if (timeSinceLastScoreUpdate >= timeBetweenScoreUpdates)
            {
                //Update the score UI here with the current value of ThisPlayerScore
                Player playerComp = other.GetComponent<Player>();
                playerComp.RecordScore(zoneName, zonePointPerSecond);
                timeSinceLastScoreUpdate /*-= timeBetweenScoreUpdates*/ = 0;
            }

            //Debug.Log(netID.netId + " Staying in zone");
        }
    }

    public void OnTriggerExit2D(Collider2D other)
    {
        netID = other.GetComponent<NetworkIdentity>();
        if (other.tag == "Player")
        {
            //Debug.Log(netID.netId + " Exited zone");
        }
    }
}
