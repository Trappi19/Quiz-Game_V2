using MySqlConnector;   // Pense à ajouter le connector .dll
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class RoleSystem : MonoBehaviour
{
    [Header("Boutons de rôle dans l'ordre d'affichage")]
    [SerializeField] private List<Button> roleButtons;

    [Header("Boutons UI")]
    [SerializeField] private Button infoButton;
    [SerializeField] private Button saveButton;

    [Header("Navigation")]
    [SerializeField] private string firstThemeSceneName = "Theme1";
    [SerializeField] private string quitSceneName = "Menu";

    [Header("Transition")]
    [SerializeField] private Animator transition;
    [SerializeField] private string transitionTriggerName = "FadeOut";
    [SerializeField] private float transitionDuration = 1f;

    [Header("Config BDD")]
    [SerializeField] private string host = "localhost";
    [SerializeField] private int port = 3306;
    [SerializeField] private string database = "quizgame";
    [SerializeField] private string user = "root";
    [SerializeField] private string password = "rootroot";

    private List<RoleData> roles = new List<RoleData>();
    private bool showDescriptions = false;
    private int selectedRoleIndex = -1;

    private void Start()
    {
        if (infoButton == null) Debug.LogError("InfoButton n'est pas assigné !");
        if (saveButton == null) Debug.LogError("SaveButton n'est pas assigné !");
        if (roleButtons == null || roleButtons.Count == 0) Debug.LogError("RoleButtons vide !");

        LoadRolesFromDatabase();
        BindButtons();
        UpdateButtonsText();

        infoButton.onClick.AddListener(ToggleInfoMode);
        saveButton.onClick.AddListener(SaveSelectedRole);
    }

    private string GetConnectionString()
    {
        return $"Server={host};Port={port};Database={database};User={user};Password={password};";
    }

    private void LoadRolesFromDatabase()
    {
        roles.Clear();

        using (MySqlConnection conn = new MySqlConnection(GetConnectionString()))
        {
            try
            {
                conn.Open();

                string query = "SELECT id, role_name, description_role FROM role ORDER BY id ASC;";
                MySqlCommand cmd = new MySqlCommand(query, conn);

                using (MySqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        int id = 0;

                        // id en int (même si la colonne est BIGINT, on passe par Convert)
                        object idObj = reader["id"];
                        if (idObj != null && idObj != System.DBNull.Value)
                            id = System.Convert.ToInt32(idObj);

                        string name = reader["role_name"] != System.DBNull.Value
                            ? reader["role_name"].ToString()
                            : string.Empty;

                        string description = reader["description_role"] != System.DBNull.Value
                            ? reader["description_role"].ToString()
                            : string.Empty;

                        roles.Add(new RoleData(id, name, description));
                    }
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError("Erreur BDD rôles : " + e.Message);
            }
        }
    }


    private void BindButtons()
    {
        for (int i = 0; i < roleButtons.Count; i++)
        {
            int index = i;
            roleButtons[i].onClick.RemoveAllListeners();
            roleButtons[i].onClick.AddListener(() => OnRoleButtonClicked(index));
        }
    }

    private void UpdateButtonsText()
    {
        for (int i = 0; i < roleButtons.Count; i++)
        {
            Text txt = roleButtons[i].GetComponentInChildren<Text>();
            if (txt == null) continue;

            if (i < roles.Count)
            {
                txt.text = showDescriptions ? roles[i].description : roles[i].name;
                txt.resizeTextForBestFit = showDescriptions;
                roleButtons[i].interactable = true;
            }
            else
            {
                txt.text = "-";
                txt.resizeTextForBestFit = false;
                roleButtons[i].interactable = false;
            }
        }
    }

    private void ToggleInfoMode()
    {
        showDescriptions = !showDescriptions;
        UpdateButtonsText();
    }

    private void OnRoleButtonClicked(int index)
    {
        if (index >= roles.Count) return;

        selectedRoleIndex = index;
        RoleData r = roles[index];
        Debug.Log("Rôle sélectionné : " + r.name + " (id=" + r.id + ")");

        // Ici tu peux par exemple mettre en surbrillance le bouton sélectionné
        // (changer la couleur du Background, etc.)
    }

    private void SaveSelectedRole()
    {
        if (selectedRoleIndex < 0 || selectedRoleIndex >= roles.Count)
        {
            Debug.LogWarning("Aucun rôle sélectionné.");
            return;
        }

        RoleData chosen = roles[selectedRoleIndex];

        PlayerPrefs.SetInt("SelectedRoleId", chosen.id);
        PlayerPrefs.SetString("SelectedRoleName", chosen.name ?? string.Empty);
        PlayerPrefs.Save();

        Debug.Log("Rôle sauvegardé : " + chosen.name);

        if (!string.IsNullOrEmpty(firstThemeSceneName))
            StartCoroutine(LoadSceneWithTransition(firstThemeSceneName));
    }

    public void QuitRoleScene()
    {
        if (!string.IsNullOrEmpty(quitSceneName))
        {
            StartCoroutine(LoadSceneWithTransition(quitSceneName));
            return;
        }

        StartCoroutine(QuitApplicationWithTransition());
    }

    private IEnumerator LoadSceneWithTransition(string sceneName)
    {
        if (transition != null && !string.IsNullOrEmpty(transitionTriggerName))
            transition.SetTrigger(transitionTriggerName);

        if (transitionDuration > 0f)
            yield return new WaitForSeconds(transitionDuration);

        SceneManager.LoadScene(sceneName);
    }

    private IEnumerator QuitApplicationWithTransition()
    {
        if (transition != null && !string.IsNullOrEmpty(transitionTriggerName))
            transition.SetTrigger(transitionTriggerName);

        if (transitionDuration > 0f)
            yield return new WaitForSeconds(transitionDuration);

        Application.Quit();
    }

    public RoleData GetSelectedRole()
    {
        if (selectedRoleIndex < 0 || selectedRoleIndex >= roles.Count)
            return null;
        return roles[selectedRoleIndex];
    }
}
