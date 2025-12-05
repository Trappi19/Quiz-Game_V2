using UnityEngine;

public class FadeTransition : MonoBehaviour
{
    public CanvasGroup canvasGroup;

    void Start()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        canvasGroup.interactable = false;  // ← CLÉ : bloque PAS les clics !
        canvasGroup.blocksRaycasts = false;  // ← Bonus : ignore raycasts
        GetComponent<Canvas>().sortingOrder = 999;
    }

}
