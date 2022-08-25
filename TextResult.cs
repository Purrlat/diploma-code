using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mono.Data.Sqlite;
using System.Data;
using TMPro;

public class TextResult : MonoBehaviour
{
    private string db_name = "URI=file:Results.db";
    public TextMeshProUGUI textResults;

    // Start is called before the first frame update
    void Start()
    {

    }

    public void OnNeedResult()
    {
        textResults = GameObject.Find("GameObjectText").GetComponent<TextMeshProUGUI>();
        using (var connection = new SqliteConnection(db_name))
        {
            connection.Open();

            using (var command = connection.CreateCommand())
            {
                command.CommandText = "SELECT * FROM results WHERE datetime = '" + gameObject.GetComponent<TextMeshProUGUI>().text + "';";
                //"INSERT INTO results (datetime, captainRole, fixerRole, stokerRole, moving, communications, moving, inaction, success, mainRole)"

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        textResults.text = "������� �������� ���� ��������: " + reader["captainRole"].ToString() +
                            "<br>������� �������� ���� ��������: " + reader["fixerRole"].ToString() +
                            "<br>������� �������� ���� ��������: " + reader["stokerRole"].ToString() + 
                            "<br>����������: " + reader["moving"].ToString() + 
                            "<br>������������: " + reader["communications"].ToString() +
                            "<br>�����������: " + reader["inaction"].ToString() + 
                            "<br>���������� ������: " + reader["success"].ToString() +
                            "<br><br>������� ����: " + reader["mainRole"].ToString();
                    }
                    reader.Close();
                }
                command.ExecuteNonQuery();
            }
            connection.Close();
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
