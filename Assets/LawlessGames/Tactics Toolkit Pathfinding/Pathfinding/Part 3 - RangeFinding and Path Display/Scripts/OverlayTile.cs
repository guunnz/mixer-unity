using System.Collections.Generic;
using System.Linq;
using Shapes;
using UnityEngine;
using UnityEngine.EventSystems;

public class OverlayTile : MonoBehaviour
{
    public int G;
    public int H;

    public int F
    {
        get { return G + H; }
    }

    public OverlayTile Previous;
    public Vector3Int gridLocation;

    public Vector2Int grid2DLocation
    {
        get { return new Vector2Int(gridLocation.x, gridLocation.z); }
    }

    private Rectangle spriteRenderer;

    public bool beingHovered;
    private Team goodTeam;
    private Team badTeam;
    public bool occupied
    {
        get { return currentOccupier != null; }
        set
        {
            if (!value)
            {
                currentOccupier = null;
            }
        }
    }

    internal AxieController currentOccupier;
    public Rectangle rectangle;
    private bool untoggleable;
    public MaterialTipColorChanger tipColorChanger;

    public static AxieController GetCurrentOccupierByY(int yValue, List<OverlayTile> allTiles)
    {
        return allTiles.FirstOrDefault(tile => tile.grid2DLocation.y == yValue)?.currentOccupier;
    }

    private void Start()
    {
        spriteRenderer = GetComponent<Shapes.Rectangle>();
        goodTeam = FindObjectsOfType<Team>().Single(x => x.isGoodTeam);
        badTeam = FindObjectsOfType<Team>().Single(x => !x.isGoodTeam);
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
        if (EventSystem.current.IsPointerOverGameObject())
        {
            beingHovered = false;
            return;
        }
        // This method is called when the mouse is over the collider of this GameObject
        beingHovered = true;
    }

    private void OnMouseExit()
    {
        // This method is called when the mouse leaves the collider
        beingHovered = false;
    }

    private void Update()
    {
        UpdateTileColor();
    }

    private void CheckOccupied()
    {
        if (currentOccupier == null)
        {
            occupied = false;
            return;
        }
        var allCharacters = goodTeam.GetAliveCharacters();
        occupied = allCharacters.Any(character => character.standingOnTile == this);
        if (!occupied)
        {
            var allCharacters2 = badTeam.GetAliveCharacters();
            occupied = allCharacters2.Any(character =>
                character.standingOnTile == this && character.axieBehavior.axieState != AxieState.Killed);
        }
    }

    public List<OverlayTile> AdjacentTiles()
    {
        return MapManager.Instance.GetAdjacentTiles(this).Where(x => !x.occupied).ToList();
    }

    private void UpdateTileColor()
    {
        if (occupied)
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