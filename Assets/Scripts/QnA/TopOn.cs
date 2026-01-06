using UnityEngine;

public class FadeTransition : MonoBehaviour
{
    public CanvasGroup canvasGroup;

    void Start()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        canvasGroup.interactable = false;  // ← CLÉ : bloque PAS les clics !
        canvasGroup.blocksRaycasts = false;
        GetComponent<Canvas>().sortingOrder = 999;
    }

}