using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class CursorImageController : MonoBehaviour
{
    public Sprite normalSprite;
    public Sprite clickedSprite;
    private Image uiImage;
    private RectTransform rectTransform;
    private Camera uiCamera;
    private Tween momentumTween;

    public float offsetY;
    private bool isMoving;
    void Start()
    {
        Cursor.visible = false;
        
        uiImage = GetComponent<Image>();
        if (uiImage == null)
        {
            Debug.LogError("Image component not found on the GameObject.");
            return;
        }

        rectTransform = GetComponent<RectTransform>();
        if (rectTransform == null)
        {
            Debug.LogError("RectTransform not found on the GameObject.");
            return;
        }

        // Determine the correct camera to use based on the canvas render mode
        Canvas canvas = GetComponentInParent<Canvas>();
        if (canvas.renderMode == RenderMode.ScreenSpaceOverlay)
        {
            uiCamera = null; // No camera for Overlay mode
        }
        else
        {
            uiCamera = canvas.worldCamera;
        }
    }

    void Update()
    {
        // Update mouse position for UI
        Vector2 screenPoint = new Vector2(Input.mousePosition.x, Input.mousePosition.y + offsetY);
        RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform.parent as RectTransform, screenPoint, uiCamera, out Vector2 localPoint);
        
        rectTransform.localPosition = localPoint;

        // Check for mouse click
        if (Input.GetMouseButtonDown(0))
        {
            uiImage.sprite = clickedSprite;
        }
        else if (Input.GetMouseButtonUp(0))
        {
            uiImage.sprite = normalSprite;
        }

        // Check if the mouse is moving
        if (Input.GetAxis("Mouse X") != 0 || Input.GetAxis("Mouse Y") != 0)
        {
            if (!isMoving)
            {
                isMoving = true;
                if (momentumTween != null && momentumTween.IsPlaying())
                {
                    // momentumTween.Kill(false);
                }
            }
        }
        else
        {
            if (isMoving)
            {
                isMoving = false;
                // ApplyMomentumEffect();
            }
        }
    }

    // private void ApplyMomentumEffect()
    // {
    //     // Apply a wiggle effect using DoTween
    //     momentumTween = rectTransform.DOShakePosition(0.5f, 0.2f, 10, 90, false, true);
    // }
}
