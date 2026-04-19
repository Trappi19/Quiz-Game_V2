using UnityEngine;
using UnityEngine.UI;

public class PodiumManager : MonoBehaviour
{
    [SerializeField] private Text podiumText; // Affichage du podium
    [SerializeField] private GameObject clearButtonGameObject; // Bouton pour vider le podium

    private Color goldColor = new Color(1f, 0.84f, 0f, 1f); // Jaune/Or
    private Color silverColor = new Color(0.75f, 0.75f, 0.75f, 1f); // Argent
    private Color bronzeColor = new Color(0.8f, 0.5f, 0.2f, 1f); // Bronze
    private Color whiteColor = Color.white;

    private void OnEnable()
    {
        RefreshPodium();
    }

    public void RefreshPodium()
    {
        if (podiumText == null) return;

        // Récupérer tous les scores du podium
        int count = PlayerPrefs.GetInt("Podium_Count", 0);

        if (count == 0)
        {
            podiumText.text = "Aucune partie enregistrée au podium.";
            return;
        }

        // Créer une liste de scores avec leurs données
        System.Collections.Generic.List<PodiumEntry> entries = new System.Collections.Generic.List<PodiumEntry>();

        for (int i = 1; i <= count; i++)
        {
            string prefix = "Podium_" + i + "_";
            if (!PlayerPrefs.HasKey(prefix + "PlayerName"))
                continue;

            string playerName = PlayerPrefs.GetString(prefix + "PlayerName", "Inconnu");
            string roleName = PlayerPrefs.GetString(prefix + "RoleName", "Aucun rôle");
            int totalScore = PlayerPrefs.GetInt(prefix + "TotalScore", 0);

            entries.Add(new PodiumEntry
            {
                PlayerName = playerName,
                RoleName = roleName,
                TotalScore = totalScore
            });
        }

        // Trier par score décroissant
        entries.Sort((a, b) => b.TotalScore.CompareTo(a.TotalScore));

        // Construire le texte du podium
        podiumText.text = "";

        for (int i = 0; i < entries.Count; i++)
        {
            int place = i + 1;
            Color placeColor = GetPlaceColor(place);

            string placeText = GetPlaceText(place);
            string entryLine = $"<color=#{ColorUtility.ToHtmlStringRGBA(placeColor)}>{placeText} - {entries[i].PlayerName}</color> ({entries[i].RoleName}) : {entries[i].TotalScore}/100\n";

            podiumText.text += entryLine;
        }
    }

    private Color GetPlaceColor(int place)
    {
        return place switch
        {
            1 => goldColor,
            2 => silverColor,
            3 => bronzeColor,
            _ => whiteColor
        };
    }

    private string GetPlaceText(int place)
    {
        return place switch
        {
            1 => "🥇 1er",
            2 => "🥈 2e",
            3 => "🥉 3e",
            _ => $"{place}e"
        };
    }

    public void ClearPodium()
    {
        int count = PlayerPrefs.GetInt("Podium_Count", 0);

        for (int i = 1; i <= count; i++)
        {
            string prefix = "Podium_" + i + "_";
            PlayerPrefs.DeleteKey(prefix + "PlayerName");
            PlayerPrefs.DeleteKey(prefix + "RoleName");
            PlayerPrefs.DeleteKey(prefix + "TotalScore");
        }

        PlayerPrefs.DeleteKey("Podium_Count");
        PlayerPrefs.Save();

        RefreshPodium();

        Debug.Log($"[Podium] Podium vidé ({count} entrée(s) supprimée(s)).");
    }

    private class PodiumEntry
    {
        public string PlayerName { get; set; }
        public string RoleName { get; set; }
        public int TotalScore { get; set; }
    }
}
