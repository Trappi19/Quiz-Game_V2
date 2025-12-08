using UnityEditor.Timeline.Actions;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StartOption : MonoBehaviour
{
    public GameObject MenueChargement;
    public GameObject MenuePanel;
    private object MenueManager;

    public void StartGame()
    {
        SceneManager.LoadScene("Theme1");
    }

    public void QuitGame()
    {
        Application.Quit();
        Debug.Log("Quit Game");
    }
    

}
