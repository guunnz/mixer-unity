using UnityEngine;
using DG.Tweening;
using UnityEngine.EventSystems;

public class ButtonHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public float hoverScale = 1.2f; // Scale factor for hover
    public float duration = 0.2f;   // Duration of the animation

    private Vector3 originalScale;   // Original scale of the button
    private RectTransform rectTransform;

    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        originalScale = rectTransform.localScale;
    }

    // When the mouse enters the button
    public void OnPointerEnter(PointerEventData eventData)
    {
        // Calculate the new scale equally on all axes
        Vector3 targetScale = originalScale * hoverScale;
        Vector3 scaleChange = targetScale - rectTransform.localScale;

        // Apply the scale change using DOTween
        rectTransform.DOScale(rectTransform.localScale + scaleChange, duration);
    }

    // When the mouse exits the button
    public void OnPointerExit(PointerEventData eventData)
    {
        // Revert back to the original scale
        rectTransform.DOScale(originalScale, duration);
    }
}
