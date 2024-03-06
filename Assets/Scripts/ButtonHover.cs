using UnityEngine;
using DG.Tweening;
using UnityEngine.EventSystems;

public class ButtonHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public float hoverScale = 1.2f; // Escala a la que crecerá el botón al pasar el mouse
    public float duration = 0.2f;   // Duración de la animación

    private Vector3 originalScale;   // Escala original del botón
    private RectTransform rectTransform;

    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        originalScale = rectTransform.localScale;
    }

    // Cuando el mouse entra en el botón
    public void OnPointerEnter(PointerEventData eventData)
    {
        rectTransform.DOScale(originalScale * hoverScale, duration);
    }

    // Cuando el mouse sale del botón
    public void OnPointerExit(PointerEventData eventData)
    {
        rectTransform.DOScale(originalScale, duration);
    }
}