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
    private RoleRepository roleRepository;
    private bool showDescriptions = false;
    private int selectedRoleIndex = -1;

    private void Start()
    {
        if (infoButton == null) Debug.LogError("[Role] InfoButton non assigné.");
        if (saveButton == null) Debug.LogError("[Role] SaveButton non assigné.");
        if (roleButtons == null || roleButtons.Count == 0) Debug.LogError("[Role] Liste des boutons de rôle vide.");

        roleRepository = new RoleRepository(GetConnectionString());
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

        try
        {
            roles = roleRepository.LoadRoles();
            Debug.Log($"[Role] {roles.Count} rôle(s) chargé(s) depuis la base.");
        }
        catch (System.Exception e)
        {
            Debug.LogError("[Role] Erreur lors du chargement des rôles : " + e.Message);
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
        Debug.Log($"[Role] Rôle sélectionné: {r.name} (id={r.id}).");

        // Ici tu peux par exemple mettre en surbrillance le bouton sélectionné
        // (changer la couleur du Background, etc.)
    }

    private void SaveSelectedRole()
    {
        if (selectedRoleIndex < 0 || selectedRoleIndex >= roles.Count)
        {
            Debug.LogWarning("[Role] Sauvegarde refusée: aucun rôle sélectionné.");
            return;
        }

        RoleData chosen = roles[selectedRoleIndex];

        PlayerPrefs.SetInt("SelectedRoleId", chosen.id);
        PlayerPrefs.SetString("SelectedRoleName", chosen.name ?? string.Empty);
        PlayerPrefs.Save();

        Debug.Log($"[Role] Rôle sauvegardé: {chosen.name} (id={chosen.id}).");

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
