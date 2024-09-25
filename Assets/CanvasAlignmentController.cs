using CGTespy.UI;
using UnityEngine;
using UnityEngine.UI;

public class CanvasAlignmentController : MonoBehaviour
{
    public enum Alignment
    {
        Center,
        Left,
        Right,
        Top,
        Bottom,
        TopLeft,
        TopRight,
        BottomLeft,
        BottomRight,
        Stretch,
        StretchHorizontal,
        StretchVertical
    }

    private RectTransform targetCanvasRectTransform;


    private void Start()
    {
        targetCanvasRectTransform = GetComponent<RectTransform>();
    }

    public Vector4 stretchedSize = new Vector4(325, 325, 161, 161);
    public Alignment alignment;

    public void SetAlignment(TextAnchor alignment)
    {
        targetCanvasRectTransform = GetComponent<RectTransform>();
        //if (alignment == Alignment.Center)
        //{
        //    targetCanvasRectTransform.sizeDelta = centeredSize;

        //}
        targetCanvasRectTransform.ApplyAnchorPreset(alignment);


    }

    public void SetStretch()
    {
        SetAnchorAndPivot(new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(1f, 1f));
        targetCanvasRectTransform.offsetMin = new Vector2(stretchedSize.x, stretchedSize.w);
        targetCanvasRectTransform.offsetMax = new Vector2(-stretchedSize.z, -stretchedSize.y);
    }

    private void SetAnchorAndPivot(Vector2 pivot, Vector2 anchorMin, Vector2 anchorMax)
    {
        targetCanvasRectTransform.pivot = pivot;
        targetCanvasRectTransform.anchorMin = anchorMin;
        targetCanvasRectTransform.anchorMax = anchorMax;
    }
}