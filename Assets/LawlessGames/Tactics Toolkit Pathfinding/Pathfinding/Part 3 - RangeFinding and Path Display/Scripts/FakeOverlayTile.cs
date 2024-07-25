using System.Collections;
using System.Collections.Generic;
using Shapes;
using UnityEngine;

public class FakeOverlayTile : MonoBehaviour
{
    public int G;
    public int H;

    public int F
    {
        get { return G + H; }
    }

    public Vector3Int gridLocation;

    public Vector2Int grid2DLocation
    {
        get { return new Vector2Int(gridLocation.x, gridLocation.z); }
    }

    private Rectangle spriteRenderer;
    public bool beingHovered;
    public bool occupied;
    internal FakeAxieController currentOccupier;
    public Rectangle rectangle;
    private bool untoggleable;

    private void Start()
    {
        spriteRenderer = GetComponent<Shapes.Rectangle>();
        rectangle = GetComponent<Rectangle>();
        if (this.grid2DLocation.x >= 4)
        {
            spriteRenderer.enabled = false;
            Destroy(this.GetComponent<BoxCollider>());
            untoggleable = true;
        }

        ToggleRectangle(false);
    }

    public void ToggleRectangle(bool enabled = false)
    {
        if (untoggleable)
            return;
        if (rectangle != null)
            rectangle.enabled = enabled;
    }

    private void OnMouseOver()
    {
        // This method is called when the mouse is over the collider of this GameObject
        beingHovered = true;
    }

    private void OnMouseExit()
    {
        // This method is called when the mouse leaves the collider
        beingHovered = false;
    }

    private void FixedUpdate()
    {
        UpdateTileColor();
    }

    private void UpdateTileColor()
    {
        if (occupied && currentOccupier.renderer.enabled == true)
        {
            spriteRenderer.Type = Rectangle.RectangleType.RoundedBorder;
            spriteRenderer.Dashed = false;
            spriteRenderer.Color = new Vector4(.9f, 0.9f, 0.9f, 0.1f);
        }
        else if (beingHovered)
        {
            spriteRenderer.Color = new Vector4(.9f, .9f, .9f, 0.7f);
            spriteRenderer.Type = Rectangle.RectangleType.RoundedBorder;
            spriteRenderer.Dashed = true;
            spriteRenderer.DashOffset += 1 * Time.deltaTime;
        }
        else
        {
            spriteRenderer.Type = Rectangle.RectangleType.RoundedBorder;
            spriteRenderer.Dashed = false;
            spriteRenderer.Color = new Vector4(.9f, .9f, .9f, 0.3f);
        }
    }
}