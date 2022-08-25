using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class NetworkManagerEdited : NetworkManager
{
    public List<PlayerScore> players_List = new List<PlayerScore>();

    public CoordsManager coordinates;
    bool setup_happened = false;

    public GameObject instrumentLocation1;
    public GameObject problemLocation1;

    public GameObject instrumentLocation2;
    public GameObject problemLocation2;

    public GameObject stokerItemLocation;
    public GameObject stokerProblemLocation;

    public Transform startPosition;

    public void Update()
    {
        /*string result = "List contains: ";
        foreach (var item in players_List)
        {
            result += item.player_name + ", " + item.player_gender.ToString() + ", " + item.player_conn.ToString();
        }
        Debug.Log(result);*/
    }

    public override void OnStartServer()
    {
        base.OnStartServer();
    }

    public override void OnServerConnect(NetworkConnection conn)
    {
        base.OnServerConnect(conn);
    }

    public override void OnServerReady(NetworkConnection conn)
    {
        base.OnServerReady(conn);
    }

    public override void OnServerAddPlayer(NetworkConnection conn)
    {
        //Transform startPos = GetStartPosition();
        GameObject player = startPosition != null
            ? Instantiate(playerPrefab, startPosition.position, startPosition.rotation)
            : Instantiate(playerPrefab);

        player.name = $"{playerPrefab.name} [connId={conn.connectionId}]";

        NetworkServer.AddPlayerForConnection(conn, player);

        if (setup_happened) return;
        SetupObjects();
    }

    public override void OnServerDisconnect(NetworkConnection conn)
    {
        base.OnServerDisconnect(conn);
    }

    public override void OnStopServer()
    {
        base.OnStopServer();
    }

    public override void OnClientConnect(NetworkConnection conn)
    {
        base.OnClientConnect(conn);
        NetworkClient.AddPlayer(); //убрала авто спавн игрока если мешает то надо убрать эту строку и вкл обратно
        NetworkManagerHUD hud = FindObjectOfType<NetworkManagerHUD>();
        DataHold data = FindObjectOfType<DataHold>();

        /*PlayerData hold = new PlayerData(data.player_Name, data.gender, conn);
        players_List.Add(hold);*/

        if (hud != null)
            hud.showGUI = false;
    }

    public override void OnClientDisconnect(NetworkConnection conn)
    {
        base.OnClientDisconnect(conn);
        NetworkManagerHUD hud = FindObjectOfType<NetworkManagerHUD>();
        if (hud == null)
            hud.showGUI = true;
    }

    private void SetupObjects()
    {
        // оординаты дл€ корабл€
        coordinates = GameObject.Find("CoordinatesManager").GetComponent<CoordsManager>();
        coordinates.x_coord = Random.Range(-100, 100);
        coordinates.y_coord = Random.Range(-100, 100);

        coordinates.needed_x = coordinates.x_coord;
        coordinates.needed_y = coordinates.y_coord;

        //¬се приготовили и игнорим вызов функции при присоединении следующего клиента:
        setup_happened = true;
    }

    [Server]
    public void SpawnProblem(string instName)
    {
        if (instName == "StokerItem")
        {
            GameObject prLocThisTime = stokerProblemLocation;
            GameObject problem = Instantiate(spawnPrefabs.Find(x => x.name == "itemReciever"), prLocThisTime.transform.position, prLocThisTime.transform.rotation);
            problem.GetComponent<ItemRecieverBehav>().neededItem = instName;
            NetworkServer.Spawn(problem);

            GameObject inLocThisTime = stokerItemLocation;
            GameObject instrument = Instantiate(spawnPrefabs.Find(x => x.name == "keyItem"), inLocThisTime.transform.position, inLocThisTime.transform.rotation);
            instrument.GetComponent<ItemBehav>().itemName = instName;
            NetworkServer.Spawn(instrument);
        }
        else
        {
            int random = Random.Range(0, 2);

            GameObject prLocThisTime;
            if (random == 0) prLocThisTime = problemLocation1; else prLocThisTime = problemLocation2;
            GameObject problem = Instantiate(spawnPrefabs.Find(x => x.name == "itemReciever"), prLocThisTime.transform.position, prLocThisTime.transform.rotation);
            problem.GetComponent<ItemRecieverBehav>().neededItem = instName;
            NetworkServer.Spawn(problem);

            GameObject inLocThisTime;
            if (random == 0) inLocThisTime = instrumentLocation1; else inLocThisTime = instrumentLocation2;
            GameObject instrument = Instantiate(spawnPrefabs.Find(x => x.name == "keyItem"), inLocThisTime.transform.position, inLocThisTime.transform.rotation);
            instrument.GetComponent<ItemBehav>().itemName = instName;
            NetworkServer.Spawn(instrument);
        }
    }
}
