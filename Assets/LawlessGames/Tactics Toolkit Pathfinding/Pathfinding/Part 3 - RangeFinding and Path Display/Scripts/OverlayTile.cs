using Shapes;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static finished3.ArrowTranslator;

namespace finished3
{
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
        public bool occupied;
        private MyTeam charactersManager;
        private EnemyTeam charactersManager2;

        private void Start()
        {
            spriteRenderer = GetComponent<Shapes.Rectangle>();
            charactersManager = FindObjectOfType<MyTeam>();
            charactersManager2 = FindObjectOfType<EnemyTeam>();

            if (this.grid2DLocation.x >= 4)
            {
                spriteRenderer.enabled = false;
                Destroy(this.GetComponent<BoxCollider>());
            }
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

        private void Update()
        {
            CheckOccupied();
            UpdateTileColor();
        }

        private void CheckOccupied()
        {
            // Assuming you have a static method to get all characters and their current tiles
            var allCharacters = charactersManager.GetCharacters();
            occupied = allCharacters.Any(character => character.standingOnTile == this);
            if (!occupied)
            {
                var allCharacters2 = charactersManager2.GetCharacters();
                occupied = allCharacters2.Any(character => character.standingOnTile == this && !character.Killed);
            }
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
}