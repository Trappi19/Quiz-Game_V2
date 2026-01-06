using MySqlConnector;
using System;
using UnityEngine;

public class MariaDBConnection : MonoBehaviour
{
    private string connStr = "Server=localhost;Database=quizgame;User ID=root;Password=rootroot;Port=3306;";
    void Start()
    {
        ConnectToDatabase();
    }

    void ConnectToDatabase()
    {
        using (MySqlConnection conn = new MySqlConnection(connStr))
        {
            try
            {
                conn.Open();
                Debug.Log("Connexion MariaDB réussie !");
                // Exemple simple de lecture
                string query = "SELECT * FROM themes LIMIT 10;";
                MySqlCommand cmd = new MySqlCommand(query, conn);
                MySqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    Debug.Log("Donnée : " + reader[0].ToString());
                }
                reader.Close();
            }
            catch (Exception ex)
            {
                Debug.LogError("Erreur MariaDB : " + ex.Message);
            }
        }
    }
}
