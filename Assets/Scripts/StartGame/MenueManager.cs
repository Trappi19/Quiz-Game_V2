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

    [Header("UI Chargement")]
    [SerializeField] private GameObject UImenuChargementPanel;
    [SerializeField] private Text slot1Text, slot2Text, slot3Text;
    [SerializeField] private Button slot1Btn, slot2Btn, slot3Btn;

    public Animator transition;

    void Start()
    {
        btnConfirmer.onClick.AddListener(ConfirmerNom);
        UImenuChargementPanel.SetActive(false);
        MenuePanel.SetActive(true);
        RefreshSaves();
    }

    public void NouvellePartie()
    {
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
        RefreshSaves();
        UImenuChargementPanel.SetActive(true);
    }

    public void RefreshSaves()
    {
        // SLOT 1
        if (PlayerPrefs.HasKey("Save1_PlayerName"))
        {
            slot1Text.text = PlayerPrefs.GetString("Save1_PlayerName") + " - T" +
                           PlayerPrefs.GetInt("Save1_Theme");
            slot1Btn.interactable = true;
            slot1Btn.onClick.RemoveAllListeners();
            slot1Btn.onClick.AddListener(() => LoadSave(1));
        }
        else
        {
            slot1Text.text = "Vide";
            slot1Btn.interactable = false;
        }

        // SLOT 2 (copiez-collez pour 2 et 3)
        if (PlayerPrefs.HasKey("Save2_PlayerName"))
        {
            slot2Text.text = PlayerPrefs.GetString("Save2_PlayerName") + " - T" +
                           PlayerPrefs.GetInt("Save2_Theme");
            slot2Btn.interactable = true;
        }
        else
        {
            slot2Text.text = "Vide";
            slot2Btn.interactable = false;
        }
    }

    void LoadSave(int slot)
    {
        string prefix = "Save" + slot + "_";
        PlayerPrefs.SetString("PlayerName", PlayerPrefs.GetString(prefix + "PlayerName"));
        SceneManager.LoadScene("Theme" + PlayerPrefs.GetInt(prefix + "Theme"));
    }
}