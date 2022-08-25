using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using TMPro;

public class GameTimer : NetworkBehaviour
{
    //��� ������:
    [SyncVar] public float gameTime; //����� ����, � ��������
    [SyncVar] public float timer; //��� ����� ���� ����, � ��������
    [SyncVar] public int marker = -1; //-1 = ���� �������, -2 = ���� ��������
    [SyncVar] public float maxTime; //���� �����
    [SyncVar] public int minPlayers; //����������� ��� ������ ���� (�������) ���-�� �������
    [SyncVar] public bool masterTimer = false; //��� ������� ������?
                                               //public ServerTimer timerObj
    
    //���������
    public TextMeshProUGUI timeText; //����� ������� � ����������
    int min; int sec; //������ � �������

    //����� ��� �������
    public float secondsTillNavigation; //����. �������� ����� ������� ���������
    public float currentSecondsTillNavigation; //�������� ����� ������� ���������
    public float navigationInterval; //�������� ��� ����, ����� ������ ������ �������� ����������
    public float currentIntervalTime; //������� ��������� ���������. ���� ������ ����� ^, �� ���� ����������
    bool checkCoords = true;
    int currentPlayers;
    float coalInterval;
    float currentCoalInterval = 0;

    //������� �����
    GameTimer serverTimer;
    public NetworkManagerEdited netman;
    public CoordsManager coordsChange;

    //���-�� �������
    public int captainEvent;
    public int fixerEvent;
    public int stokerEvent;

    void Start()
    {
        if (isServer)
        { // For the host to do: use the timer and control the time.
            if (isLocalPlayer)
            {
                serverTimer = this;
                masterTimer = true;
            }
        }
        else if (isLocalPlayer)
        { //For all the boring old clients to do: get the host's timer.
            GameTimer[] timers = FindObjectsOfType<GameTimer>();
            for (int i = 0; i < timers.Length; i++)
            {
                if (timers[i].masterTimer)
                {
                    serverTimer = timers[i];
                }
            }
        }
        coordsChange = GameObject.Find("CoordinatesManager").GetComponent<CoordsManager>();
        currentPlayers = NetworkServer.connections.Count;
        coalInterval = maxTime/(currentPlayers*/*4*/20);
        netman = GameObject.Find("NetManager").GetComponent<NetworkManagerEdited>();
    }

    void SceneChange()
    {
        if (isServer) RpcNextScene();
        else CmdNextScene();
    }

    [Command]
    public void CmdNextScene()
    {
        RpcNextScene();
    }

    [ClientRpc]
    public void RpcNextScene()
    {
        //NetMan.singleton.StopHost();
        NetworkManagerEdited.singleton.ServerChangeScene("ResultsScene");
        //NetMan.Shutdown();
        //Destroy(NetMan.singleton);
        //NetMan.singleton.OnApplicationQuit();
    }

    void Update()
    {
        //Debug.Log(NetworkServer.connections.Count);
        if (masterTimer)
        {
            //������ ������� ������ ������������ �����
            if (timer <= 0) //���� ����� ��������� - ���� ��������
            {
                marker = -2;
            }
            else if (marker == -1) //���� ���� �������
            {
                if (NetworkServer.connections.Count >= minPlayers) //���� ����� ������� ���������� �� �������, ������
                {
                    timer = maxTime;
                    marker = 0;
                }
            }
            else if (marker == -2) //���� ���� ���������
            {
                //Game done.
                //netman = GameObject.Find("NetManager").GetComponent<NetworkManagerEdited>();
                //NetworkManagerEdited.singleton.ServerChangeScene("ResultsScene");
                //SceneChange();
            }
            else //���� ���� ������������
            {
                timer -= Time.deltaTime; //����� ������� �������
                min = Mathf.FloorToInt(timer / 60);
                sec = Mathf.FloorToInt(timer % 60);
                timeText.text = min.ToString("00") + ":" + sec.ToString("00"); //������� �����

                if (currentSecondsTillNavigation <= 0) //���� ��������� ����� ���� ������ ��������� 
                {
                    if (isServer)
                    {
                        if (/*coordsChange.neededCoordsObject.needChange*/checkCoords) { //���� �������� ������ ��
                            int need_x = coordsChange.x_coord + Random.Range(-5, 5); 
                            int need_y = coordsChange.y_coord + Random.Range(-5, 5);
                            coordsChange.SetNeededCoords(need_x, need_y); //����� ������ ����������
                            captainEvent++;
                            checkCoords = false;
                        }
                    }
                    currentIntervalTime += Time.deltaTime; //���������� ����� ��������� �������� ����� ������� ������ ��������� � ������ ���������� �������
                    //currentSecondsTillNavigation = secondsTillNavigation;
                    if ((currentIntervalTime >= navigationInterval) && (!coordsChange.CheckCoords())) //���� ����� ���������� � ���������� �� ������������� ������, ������� "��������"
                    {
                        int rand = Random.Range(0, 2);
                        string randName;
                        switch (rand)
                        {
                            case 0:
                                randName = "FireWater"; break;
                            case 1:
                                randName = "Instruments"; break;
                            default:
                                randName = "no"; break;
                        }
                        //Debug.Log(randName + " was the name we got.");
                        //currentIntervalTime = 0; 
                        currentSecondsTillNavigation = secondsTillNavigation; //���������� ����� �� ����. ���������
                        //netID = NetworkClient.connection.identity;
                        netman.SpawnProblem(randName); //������� ��������
                        fixerEvent++;
                        //checkCoords = true;
                    }
                    if (currentIntervalTime >= navigationInterval)
                    {
                        currentIntervalTime = 0; //���������� ����� ���������
                        checkCoords = true;
                    }
                }
                else currentSecondsTillNavigation -= Time.deltaTime; //���� ����� �� ����. ����� ��������� �� �����������, ������� ������

                currentCoalInterval += Time.deltaTime;
                if (currentCoalInterval >= coalInterval)
                {
                    netman.SpawnProblem("StokerItem");
                    currentCoalInterval = 0;
                    stokerEvent++;
                }
            }
        }

        if (isLocalPlayer)
        { //EVERYBODY updates their own time accordingly.
            if (serverTimer)
            {
                gameTime = serverTimer.gameTime;
                timer = serverTimer.timer;
                minPlayers = serverTimer.minPlayers;
            }
            else
            { //Maybe we don't have it yet?
                GameTimer[] timers = FindObjectsOfType<GameTimer>();
                for (int i = 0; i < timers.Length; i++)
                {
                    if (timers[i].masterTimer)
                    {
                        serverTimer = timers[i];
                    }
                }
            }
        }

        if (marker == -2)
        {
            DataHold DH = GameObject.Find("DataHolder").GetComponent<DataHold>();
            DH.GetEventCount(captainEvent,fixerEvent,stokerEvent);
            SceneChange();
        }
    }

    public void ClickTest()
    {
        if (NetworkServer.connections.Count >= /*2*/1)
            CmdClickTest();
    }

    [Command(requiresAuthority = false)]
    void CmdClickTest()
    {
        minPlayers = NetworkServer.connections.Count;
        Debug.Log(NetworkServer.connections.Count);
        //RpcStartGame();

        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        foreach (GameObject player in players)
        {
            player.transform.position = Vector3.zero;
        }
    }

    /*[ClientRpc]
    void RpcStartGame()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        foreach (GameObject player in players)
        {
            
        }
    }*/
}
