using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using TMPro;

public class GameTimer : NetworkBehaviour
{
    //—ам таймер:
    [SyncVar] public float gameTime; //ƒлина игры, в секундах
    [SyncVar] public float timer; // ак долго игра идет, в секундах
    [SyncVar] public int marker = -1; //-1 = ждем игроков, -2 = игра окончена
    [SyncVar] public float maxTime; //макс врем€
    [SyncVar] public int minPlayers; //Ќеобходимое дл€ начала игры (таймера) кол-во игроков
    [SyncVar] public bool masterTimer = false; //Ёто главный таймер?
                                               //public ServerTimer timerObj
    
    //»нтерфейс
    public TextMeshProUGUI timeText; //вывод времени в интерфейсе
    int min; int sec; //минуты и секунды

    //¬рем€ дл€ событий
    public float secondsTillNavigation; //макс. интервал между сменами координат
    public float currentSecondsTillNavigation; //интервал между сменами координат
    public float navigationInterval; //интервал дл€ того, чтобы игроки успели помен€ть координаты
    public float currentIntervalTime; //текущее состо€ние интервала. если станет равно ^, то чота произойдет
    bool checkCoords = true;
    int currentPlayers;
    float coalInterval;
    float currentCoalInterval = 0;

    //ќбъекты сцены
    GameTimer serverTimer;
    public NetworkManagerEdited netman;
    public CoordsManager coordsChange;

    // ол-во событий
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
            //“олько главный таймер контролирует врем€
            if (timer <= 0) //если врем€ кончилось - игра окончена
            {
                marker = -2;
            }
            else if (marker == -1) //если ждем игроков
            {
                if (NetworkServer.connections.Count >= minPlayers) //если колво игроков перевалило за минимум, пошоши
                {
                    timer = maxTime;
                    marker = 0;
                }
            }
            else if (marker == -2) //если игра закончена
            {
                //Game done.
                //netman = GameObject.Find("NetManager").GetComponent<NetworkManagerEdited>();
                //NetworkManagerEdited.singleton.ServerChangeScene("ResultsScene");
                //SceneChange();
            }
            else //если игра продолжаетс€
            {
                timer -= Time.deltaTime; //минус секунда таймера
                min = Mathf.FloorToInt(timer / 60);
                sec = Mathf.FloorToInt(timer % 60);
                timeText.text = min.ToString("00") + ":" + sec.ToString("00"); //выводим врем€

                if (currentSecondsTillNavigation <= 0) //если наступило врем€ след набора координат 
                {
                    if (isServer)
                    {
                        if (/*coordsChange.neededCoordsObject.needChange*/checkCoords) { //если проверка прошла ок
                            int need_x = coordsChange.x_coord + Random.Range(-5, 5); 
                            int need_y = coordsChange.y_coord + Random.Range(-5, 5);
                            coordsChange.SetNeededCoords(need_x, need_y); //новые нужные координаты
                            captainEvent++;
                            checkCoords = false;
                        }
                    }
                    currentIntervalTime += Time.deltaTime; //прибавл€ем врем€ интервала ожидани€ между началом выдачи координат и сроком выполнени€ задани€
                    //currentSecondsTillNavigation = secondsTillNavigation;
                    if ((currentIntervalTime >= navigationInterval) && (!coordsChange.CheckCoords())) //если врем€ перевалило » координаты не соответствуют нужным, спавним "проблему"
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
                        currentSecondsTillNavigation = secondsTillNavigation; //сбрасываем врем€ до след. навигации
                        //netID = NetworkClient.connection.identity;
                        netman.SpawnProblem(randName); //спавним проблему
                        fixerEvent++;
                        //checkCoords = true;
                    }
                    if (currentIntervalTime >= navigationInterval)
                    {
                        currentIntervalTime = 0; //сбрасываем врем€ интервала
                        checkCoords = true;
                    }
                }
                else currentSecondsTillNavigation -= Time.deltaTime; //если врем€ до след. новых координат не закончилось, считаем дальше

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
