using UnityEngine;

public static class PodiumSystem
{
    public static void AddRunToPodium()
    {
        string playerName = PlayerPrefs.GetString("PlayerName", "Inconnu");
        string roleName = PlayerPrefs.GetString("SelectedRoleName", "Aucun rôle");
        int totalScore = GameManager.Instance.GetTotalScore(); // 0..100

        int count = PlayerPrefs.GetInt("Podium_Count", 0);
        int index = count + 1;

        string prefix = "Podium_" + index + "_";

        PlayerPrefs.SetString(prefix + "PlayerName", playerName);
        PlayerPrefs.SetString(prefix + "RoleName", roleName);
        PlayerPrefs.SetInt(prefix + "TotalScore", totalScore);

        PlayerPrefs.SetInt("Podium_Count", index);
        PlayerPrefs.Save();

        Debug.Log($"[Podium] Partie enregistrée au podium (index={index}, joueur={playerName}, score={totalScore}/100).");
    }
}
