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
        public int F { get { return G + H; } }

        public bool isBlocked = false;

        public OverlayTile Previous;
        public Vector3Int gridLocation;
        public Vector2Int grid2DLocation {get { return new Vector2Int(gridLocation.x, gridLocation.z); } }

        private SpriteRenderer spriteRenderer;

        public bool beingHovered;
        public bool occupied;
        private MouseController charactersManager;

        private void Start()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            charactersManager = FindObjectOfType<MouseController>();
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
        }
        
        private void UpdateTileColor()
        {
            if (occupied)
            {
                spriteRenderer.color = Color.blue;
            }
            else if (beingHovered)
            {
                spriteRenderer.color = Color.green;
            }
            else
            {
                spriteRenderer.color = Color.white;
            }
        }
        
       
    }
}