using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using finished3;
using CharacterInfo = finished3.CharacterInfo;

public class DragAndDropCharacter : MonoBehaviour
{
    private Camera mainCamera;
    private GameObject selectedCharacter;
    private Vector3 originalPosition;
    private OverlayTile originalTile;
    private List<OverlayTile> allOverlayTiles;

    void Start()
    {
        mainCamera = Camera.main; // Assuming the main camera is tagged as "MainCamera"
        allOverlayTiles = FindObjectsOfType<OverlayTile>().ToList();
    }

    void Update()
    {
        // Detect mouse click
        if (Input.GetMouseButtonDown(0))
        {
            RaycastHit hit;
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out hit))
            {
                if (hit.collider != null && hit.collider.gameObject.CompareTag("Character"))
                {
                    selectedCharacter = hit.collider.gameObject;
                    originalPosition = selectedCharacter.transform.position;
                    originalTile = selectedCharacter.GetComponent<finished3.CharacterInfo>().standingOnTile;
                    originalTile.isBlocked = false; // Free the original tile
                    selectedCharacter.transform.SetParent(mainCamera.transform);
                }
            }
        }

        // Move character with mouse
        if (selectedCharacter != null && Input.GetMouseButton(0))
        {
            Vector3 mousePosition = new Vector3(Input.mousePosition.x, Input.mousePosition.y,
                mainCamera.WorldToScreenPoint(selectedCharacter.transform.position).z);
            Vector3 newPosition = mainCamera.ScreenToWorldPoint(mousePosition);
            selectedCharacter.transform.position = newPosition;
        }

        // Release character and place on the closest tile
        if (selectedCharacter != null && Input.GetMouseButtonUp(0))
        {
            OverlayTile closestTile = GetClosestTile(selectedCharacter.transform.position);

            if (closestTile != null && !closestTile.isBlocked)
            {
                selectedCharacter.transform.position = closestTile.transform.position; // Move to the new tile
                closestTile.isBlocked = true; // Block the new tile
                selectedCharacter.GetComponent<CharacterInfo>().standingOnTile = closestTile; // Update standingOnTile

                // Set scale based on grid X position
                float initialScaleX = closestTile.grid2DLocation.x < 4 ? -0.2f : 0.2f;
                selectedCharacter.transform.localScale = new Vector3(initialScaleX,
                    selectedCharacter.transform.localScale.y, selectedCharacter.transform.localScale.z);
            }
            else
            {
                selectedCharacter.transform.position = originalPosition; // Return to original position
                originalTile.isBlocked = true; // Re-block the original tile
            }

            selectedCharacter.transform.SetParent(null); // Unparent the character
            selectedCharacter = null; // Reset the selected character
        }
    }

    private OverlayTile GetClosestTile(Vector3 position)
    {
        OverlayTile closestTile = null;
        float minDistance = float.MaxValue;

        if (allOverlayTiles.Count == 0)
        {
            allOverlayTiles = FindObjectsOfType<OverlayTile>().ToList();
        }

        foreach (var tile in allOverlayTiles)
        {
            float distance = Vector3.Distance(position, tile.transform.position);
            if (distance < minDistance)
            {
                minDistance = distance;
                closestTile = tile;
            }
        }

        return closestTile;
    }
}