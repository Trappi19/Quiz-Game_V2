using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class MenuManager : MonoBehaviour
{
    [SerializeField] private GameObject nomJoueurPanel;
    [SerializeField] private GameObject MenuePanel;
    [SerializeField] private InputField inputNomJoueur;
    [SerializeField] private Button btnConfirmer;
    [SerializeField] private GameObject HistoryPanel;


    [Header("UI Chargement")]
    [SerializeField] private GameObject UImenuChargementPanel;
    [SerializeField] private Text slot1Text, slot2Text, slot3Text;
    [SerializeField] private Button slot1Btn, slot2Btn, slot3Btn;
    //[SerializeField] private Button slot1DeleteBtn, slot2DeleteBtn, slot3DeleteBtn;

    [Header("MenueSelect")]
    [SerializeField] private GameObject DeleteSavePanel;

    [Header("Transition")]
    public Animator transition;

    bool actived = false;

    void Start()
    {
        btnConfirmer.onClick.AddListener(ConfirmerNom);
        UImenuChargementPanel.SetActive(false);
        MenuePanel.SetActive(true);
        HistoryPanel.SetActive(false);
    }

    public void NouvellePartie()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.ResetRun();   // appel sécurisé
        }

        nomJoueurPanel.SetActive(true);
        inputNomJoueur.ActivateInputField();
        Debug.Log("Nouvelle Partie");
    }

    public void ConfirmerNom()
    {
        string pseudo = inputNomJoueur.text;
        if (!string.IsNullOrEmpty(pseudo))
        {
            PlayerPrefs.SetString("PlayerName", pseudo); // Sauvegarde nom
            //nomJoueurPanel.SetActive(false); Facultatif
            StartCoroutine(Starttheme1()); // Charge thème 1
        }
    }

    public IEnumerator Starttheme1()
    {
        transition.SetTrigger("FadeOut");

        yield return new WaitForSeconds(1);
        SceneManager.LoadScene("Theme1");
    }

    public void OuvrirMenuChargement()
    {
        MenuePanel.SetActive(false);
        RefreshSaves();                // ← on met à jour les slots ici
        UImenuChargementPanel.SetActive(true);

        Debug.Log(">>> OuvrirMenuChargement appelé");
    }

    public void RefreshSaves()
    {
        // Sécurité de base
        if (slot1Text == null || slot1Btn == null ||
            slot2Text == null || slot2Btn == null ||
            slot3Text == null || slot3Btn == null)
        {
            Debug.LogError("Un des slots n'est pas assigné dans l'Inspector !");
            return;
        }

        // SLOT 1
        if (PlayerPrefs.HasKey("Save1_PlayerName"))
        {
            string nom = PlayerPrefs.GetString("Save1_PlayerName");
            int theme = PlayerPrefs.GetInt("Save1_Theme", 1);
            slot1Text.text = nom + " - T" + theme;
            slot1Btn.interactable = true;

            slot1Btn.onClick.RemoveAllListeners();
            slot1Btn.onClick.AddListener(() => LoadSave(1));
        }
        else
        {
            slot1Text.text = "Vide";
            slot1Btn.interactable = false;
        }

        // SLOT 2
        if (PlayerPrefs.HasKey("Save2_PlayerName"))
        {
            string nom = PlayerPrefs.GetString("Save2_PlayerName");
            int theme = PlayerPrefs.GetInt("Save2_Theme", 1);
            slot2Text.text = nom + " - T" + theme;
            slot2Btn.interactable = true;

            slot2Btn.onClick.RemoveAllListeners();
            slot2Btn.onClick.AddListener(() => LoadSave(2));
        }
        else
        {
            slot2Text.text = "Vide";
            slot2Btn.interactable = false;
        }

        // SLOT 3
        if (PlayerPrefs.HasKey("Save3_PlayerName"))
        {
            string nom = PlayerPrefs.GetString("Save3_PlayerName");
            int theme = PlayerPrefs.GetInt("Save3_Theme", 1);
            slot3Text.text = nom + " - T" + theme;
            slot3Btn.interactable = true;

            slot3Btn.onClick.RemoveAllListeners();
            slot3Btn.onClick.AddListener(() => LoadSave(3));
        }
        else
        {
            slot3Text.text = "Vide";
            slot3Btn.interactable = false;
        }
    }

    public void LoadSave(int slot)
    {
        string prefix = "Save" + slot + "_";

        string nom = PlayerPrefs.GetString(prefix + "PlayerName", "Inconnu");
        int theme = PlayerPrefs.GetInt(prefix + "Theme", 1);
        int question = PlayerPrefs.GetInt(prefix + "Question", 0);

        // CHARGE TOUS LES SCORES DES 5 THÈMES
        for (int i = 0; i < 5; i++)
        {
            GameManager.Instance.themeScores[i] = PlayerPrefs.GetInt(prefix + "ScoreTheme" + i, 0);
        }

        Debug.Log($"LOAD SAVE slot {slot}: theme={theme}, question={question}, scores=[{string.Join(",", GameManager.Instance.themeScores)}]");

        PlayerPrefs.SetString("PlayerName", nom);
        PlayerPrefs.SetInt("Resume_Question", question);
        PlayerPrefs.SetInt("Resume_Theme", theme);

        GameManager.Instance.currentThemeIndex = theme - 1;

        SceneManager.LoadScene("Theme" + theme);
    }

    public void DeleteSave(int slot)
    {
        string prefix = "Save" + slot + "_";

        // Supprime TOUTES les clés de ce slot
        PlayerPrefs.DeleteKey(prefix + "PlayerName");
        PlayerPrefs.DeleteKey(prefix + "Theme");
        PlayerPrefs.DeleteKey(prefix + "Question");

        // Supprime tous les scores des 5 thèmes
        for (int i = 0; i < 5; i++)
        {
            PlayerPrefs.DeleteKey(prefix + "ScoreTheme" + i);
        }

        PlayerPrefs.Save();

        Debug.Log($"❌ Sauvegarde slot {slot} supprimée");

        // Rafraîchit l'affichage
        RefreshSaves();
    }
    
    public void ExtendDeleteSave()
    {
        if (actived == false)
        {
            DeleteSavePanel.SetActive(true);
            actived = true;
        }
        else
        {
            DeleteSavePanel.SetActive(false);
            actived = false;
        }
    }

    public void ReturnToMenue()
    {
        MenuePanel.SetActive(true);
        UImenuChargementPanel.SetActive(false);
        DeleteSavePanel.SetActive(false);
        HistoryPanel.SetActive(false);
    }

    public void QuitGame()
    {
        Application.Quit();
        Debug.Log("Quit Game");
    }

    public void OpenHistoryPanel()
    {
        MenuePanel.SetActive(false);
        HistoryPanel.SetActive(true);
    }
}