using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    [SerializeField] private GameObject nomJoueurPanel;
    [SerializeField] private GameObject MenuePanel;
    [SerializeField] private InputField inputNomJoueur;
    [SerializeField] private Button btnConfirmer;

    void Start()
    {
        btnConfirmer.onClick.AddListener(ConfirmerNom);
        nomJoueurPanel.SetActive(false);
    }

    public void NouvellePartie()
    {
        nomJoueurPanel.SetActive(true);
        MenuePanel.SetActive(false);
        inputNomJoueur.ActivateInputField(); // Focus auto [web:7]
    }

    public void ConfirmerNom()
    {
        string nom = inputNomJoueur.text;
        if (!string.IsNullOrEmpty(nom))
        {
            PlayerPrefs.SetString("PlayerName", nom); // Sauvegarde nom [web:29][web:34]
            nomJoueurPanel.SetActive(false);
            SceneManager.LoadScene("Theme1"); // Charge thème 1
        }
    }
}