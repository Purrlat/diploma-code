using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class DataHold : MonoBehaviour
{
    public bool gender = false; //false = girl, true = boy
    public string player_Name = "Player";
    public TextMeshProUGUI player_Text;
    public string game_scene = "GameLoop";
    public GameObject player_name_panel;

    public PlayerScore playerScore;
    //Кол-во событий
    public int captainEvent;
    public int fixerEvent;
    public int stokerEvent;

    void Awake()
    {
        DontDestroyOnLoad(this.gameObject);
    }

    void Start()
    {
        if ((PlayerPrefs.HasKey("PlayerName")) && (PlayerPrefs.GetString("PlayerName") == player_Name))
        {
            player_Name = PlayerPrefs.GetString("PlayerName");
        }
    }

    public void GetEventCount(int cpt, int fxr, int stkr)
    {
        captainEvent = cpt;
        fixerEvent = fxr;
        stokerEvent = stkr;
        playerScore.captainScore = captainEvent - fixerEvent;
    }

    public void Male_Click()
    {
        if (!gender) gender = true;
    }

    public void Female_Click()
    {
        if (gender) gender = false;
    }

    public void End_Name_Edit()
    {
        player_Name = player_Text.text;
        PlayerPrefs.SetString("PlayerName", player_Name);
        player_name_panel.SetActive(false);
    }

    public void Set_Panel_Active()
    {
        player_name_panel.SetActive(true);
    }

    public void StartGame()
    {
        if ((player_Name == "Player") || (player_Name == ""))
        {
            Set_Panel_Active();
        } 
        else SceneManager.LoadScene(game_scene);
    }

    public void ExitGame()
    {
        Application.Quit();
    }
}
