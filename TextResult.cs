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
                        textResults.text = "Решение проблемы роли капитана: " + reader["captainRole"].ToString() +
                            "<br>Решение проблемы роли механика: " + reader["fixerRole"].ToString() +
                            "<br>Решение проблемы роли кочегара: " + reader["stokerRole"].ToString() + 
                            "<br>Активность: " + reader["moving"].ToString() + 
                            "<br>Социальность: " + reader["communications"].ToString() +
                            "<br>Бездействие: " + reader["inaction"].ToString() + 
                            "<br>Показатель успеха: " + reader["success"].ToString() +
                            "<br><br>Главная роль: " + reader["mainRole"].ToString();
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
