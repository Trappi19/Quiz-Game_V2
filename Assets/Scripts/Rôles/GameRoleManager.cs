using UnityEngine;

public class GameRoleManager : MonoBehaviour
{
    private int currentRoleId;

    private void Awake()
    {
        currentRoleId = PlayerPrefs.GetInt("SelectedRoleId", -1);
        Debug.Log("[Role] Id du rôle actif : " + currentRoleId);

        // Ensuite tu appliques les bonus selon l'id
        // ex: switch(currentRoleId) { case 1: // Debugueur ... }
    }
}
