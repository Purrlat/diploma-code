using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Text;
using System.IO;
using System;
using Mono.Data.Sqlite;
using System.Data;
//using Mono.Data.SqliteClient;

public class ResultController : MonoBehaviour
{
    PlayerScore playerScore;

    public DataHold DHComp;
    public TextMeshProUGUI resultsText;
    public TextMeshProUGUI resultsTextSlider;
    public Slider resultsSlider;

    private List<string[]> rowData = new List<string[]>();

    int captainRole;
    int fixerRole;
    int stokerRole;
    int moving;
    int communications;
    int inaction;
    int success;

    string player_Name;
    string datetime;

    private string db_name = "URI=file:Results.db";

    // Start is called before the first frame update
    void Start()
    {
        DHComp = GameObject.Find("DataHolder").GetComponent<DataHold>();
        player_Name = DHComp.player_Name;
        playerScore = DHComp.playerScore;
        datetime = System.DateTime.UtcNow.ToLocalTime().ToString("dd-MM-yyyy HH-mm");

        captainRole = playerScore.captainAreaScore + playerScore.captainScore; //���� ���������� ���� ��������
        fixerRole = playerScore.fixerAreaScore + playerScore.fixerScore; //���� ���������� ���� ��������/��������
        stokerRole = playerScore.stokerAreaScore + playerScore.stokerScore; //���� ���������� ���� ��������
        moving = playerScore.movingScore; //�����������
        communications = playerScore.communicationScore; //������������
        
        inaction = playerScore.inactionScore; //�����������
        if ((captainRole > 0) || (fixerRole > 0) || (stokerRole > 0))
        {
            inaction = inaction - (captainRole + fixerRole + stokerRole);
            if (inaction < 0) inaction = 0;
        }

        string mainRole = ""; int rolePoints = 0;
        if (((captainRole < 15) && (fixerRole < 15) && (stokerRole < 15))||(moving == 0))
        {
            //����������
            success = 0;
        } else
        {
            if (captainRole > fixerRole)
            {
                if (captainRole > stokerRole) { 
                    mainRole = "���� ��������"; 
                    success = playerScore.captainScore;
                }
            }
            if (fixerRole > captainRole) 
            {
                if (fixerRole > stokerRole) { 
                    mainRole = "���� ��������";
                    success = playerScore.fixerScore;
                }
            }
            if (stokerRole >= captainRole) 
            {
                if (stokerRole >= fixerRole)
                {
                    mainRole = "���� ��������";
                    success = playerScore.stokerScore;
                }
            }
        }

        int finalScore = 0;
        finalScore = captainRole + fixerRole + stokerRole + moving + communications - inaction;
        resultsSlider.value = finalScore;
        if (finalScore < 50) resultsTextSlider.text = "�����..."; 
        else if ((finalScore > 50) && (finalScore < 150)) resultsTextSlider.text = "������"; 
        else if ((finalScore > 150) && (finalScore < 300)) resultsTextSlider.text = "�������"; 
        else if (finalScore > 300) resultsTextSlider.text = "��������� �������";

        resultsText.text = $"������� �������� ���� ��������: {captainRole}<br>" +
            $"������� �������� ���� ��������: {fixerRole}<br>" +
            $"������� �������� ���� ��������: {stokerRole}<br>" +
            $"�����������: {moving}<br>" +
            $"������������: {communications}<br>" +
            $"�����������: {inaction}<br><br><br> ���� ������� ����: {mainRole}, ���������� ������ ������: {success} �� {rolePoints}";
        //Save();
        CreateDB();
        AddLine(datetime, captainRole, fixerRole, stokerRole, moving, communications, inaction, success, mainRole);
        //DisplayDB();
    }

    //DATABASE ACTIONS
    public void CreateDB()
    {
        using (var connection = new SqliteConnection(db_name))
        {
            connection.Open();

            using (var command = connection.CreateCommand())
            {
                command.CommandText = "CREATE TABLE IF NOT EXISTS results (datetime VARCHAR(30), captainRole INT, fixerRole INT, stokerRole INT, moving INT, communications INT, inaction INT, success INT, mainRole VARCHAR(30));";
                command.ExecuteNonQuery();
            }
            connection.Close();
        }
    }

    public void AddLine(/*���������*/string dateTime, int captainRole, int fixerRole, int stokerRole, int moving, int communications, int inaction, int success, string mainRole)
    {
        using (var connection = new SqliteConnection(db_name))
        {
            connection.Open();

            using (var command = connection.CreateCommand())
            {
                command.CommandText = "INSERT INTO results (datetime, captainRole, fixerRole, stokerRole, moving, communications, inaction, success, mainRole)" +
                    " VALUES ('" + dateTime + "', '" + captainRole + "', '" + fixerRole + "', '" + stokerRole + "', '" + moving + "', '" + communications + "', '" + inaction + "', '" + success + "', '" + mainRole + "');";
                Debug.Log("Added " + command.CommandText);
                command.ExecuteNonQuery();
            }
            connection.Close();
        }
    }

    public void DisplayDB()
    {
        using (var connection = new SqliteConnection(db_name))
        {
            connection.Open();

            using (var command = connection.CreateCommand())
            {
                command.CommandText = "SELECT * FROM results;";

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                        Debug.Log(reader["captainRole"].ToString() + " < br > " + reader["fixerRole"].ToString() + " < br > " + reader["stokerRole"].ToString() + " < br > " + reader["moving"].ToString() + " < br > " + reader["inaction"].ToString() + " < br > " + reader["success"].ToString() + " < br > " + reader["mainRole"].ToString());
                    reader.Close();
                }
                command.ExecuteNonQuery();
            }
            connection.Close();
        }
    }

    private string getPath(string datetime)
    {
#if UNITY_EDITOR
        return Application.dataPath +"/SavedData/"+ player_Name + " " + datetime +".csv";
#elif UNITY_ANDROID
        return Application.persistentDataPath+"Saved_data.csv";
#elif UNITY_IPHONE
        return Application.persistentDataPath+"/"+"Saved_data.csv";
#else
        return Application.dataPath + "/" + player_Name + " " + datetime + ".csv";
#endif
    }
}
