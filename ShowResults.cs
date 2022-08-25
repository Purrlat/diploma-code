using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mono.Data.Sqlite;
using System.Data;
using TMPro;

public class ShowResults : MonoBehaviour
{
    private string db_name = "URI=file:Results.db";
    public GameObject content;
    public GameObject textPrefab;

    // Start is called before the first frame update
    void Start()
    {
        DisplayDB();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void DisplayDB()
    {
        using (var connection = new SqliteConnection(db_name))
        {
            connection.Open();

            using (var command = connection.CreateCommand())
            {
                command.CommandText = "SELECT datetime FROM results;";

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        //Debug.Log(reader["datetime"]);
                        GameObject newResultContent = textPrefab;
                        newResultContent.GetComponent<TextMeshProUGUI>().text = reader["datetime"].ToString();
                        Instantiate(newResultContent, content.transform);
                    }
                    reader.Close();
                }
                command.ExecuteNonQuery();
            }
            connection.Close();
        }
    }

    public void ShowHidePanel()
    {
        if (gameObject.activeSelf)
        {
            gameObject.SetActive(false);
        } else gameObject.SetActive(true);
    }
}
