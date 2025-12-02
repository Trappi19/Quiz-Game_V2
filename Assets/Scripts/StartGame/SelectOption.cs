using UnityEditor.Timeline.Actions;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StartOption : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    //public void Start()
    //{
    //    Text MenuSelect = GetComponent<string>().color;
    //}
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
