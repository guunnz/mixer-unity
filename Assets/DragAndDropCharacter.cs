using System;
using System.Collections;
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
    private MyTeam myTeam;

    void Start()
    {
        mainCamera = Camera.main; // Assuming the main camera is tagged as "MainCamera"
        allOverlayTiles = FindObjectsOfType<OverlayTile>().ToList();
        myTeam = FindObjectOfType<MyTeam>(); // Get the MouseController instance
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

                    selectedCharacter.GetComponent<finished3.CharacterInfo>().Grabbed = true;
                    originalPosition = selectedCharacter.transform.position;
                    originalTile = selectedCharacter.GetComponent<finished3.CharacterInfo>().standingOnTile;
                    selectedCharacter.transform.SetParent(mainCamera.transform);
                }
            }
        }

        // Move character with mouse
        if (selectedCharacter != null && Input.GetMouseButton(0))
        {
            selectedCharacter.GetComponent<BoxCollider>().enabled = false;
            Vector3 mousePosition = new Vector3(Input.mousePosition.x, Input.mousePosition.y,
                mainCamera.WorldToScreenPoint(selectedCharacter.transform.position).z);
            Vector3 newPosition = mainCamera.ScreenToWorldPoint(mousePosition);
            selectedCharacter.transform.position = newPosition;
        }

        // Release character and attempt to place on the closest tile
        if (selectedCharacter != null && Input.GetMouseButtonUp(0))
        {
            selectedCharacter.GetComponent<BoxCollider>().enabled = true;
            selectedCharacter.GetComponent<finished3.CharacterInfo>().Grabbed = false;
            OverlayTile closestTile = GetClosestTile(selectedCharacter.transform.position);

            if (closestTile == null)
            {
                selectedCharacter.GetComponent<BoxCollider>().enabled = true;
                selectedCharacter.GetComponent<finished3.CharacterInfo>().Grabbed = false;
                MoveCharacterToTile(selectedCharacter.GetComponent<finished3.CharacterInfo>(),
                    selectedCharacter.GetComponent<finished3.CharacterInfo>().standingOnTile);
                return;
            }

            TryPlaceCharacterOnTile(selectedCharacter, closestTile);

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

        OverlayTile tile = allOverlayTiles.FirstOrDefault(x => x.beingHovered);
        if (tile == null)
        {
            tile = myTeam.GetCharacters().FirstOrDefault(x => x.beingHovered)?.standingOnTile;
        }

        return tile;
    }

    private void TryPlaceCharacterOnTile(GameObject character, OverlayTile targetTile)
    {
        CharacterInfo selectedCharacterInfo = character.GetComponent<CharacterInfo>();

        if (targetTile.occupied)
        {
            var allCharacters = myTeam.GetCharacters();
            CharacterInfo occupyingCharacter = allCharacters.FirstOrDefault(c => c.standingOnTile == targetTile);

            if (occupyingCharacter != null)
            {
                SwapCharacters(selectedCharacterInfo, occupyingCharacter);
            }
            else
            {
                MoveCharacterToTile(selectedCharacterInfo, targetTile);
            }
        }
        else
        {
            MoveCharacterToTile(selectedCharacterInfo, targetTile);
        }
    }

    private void SwapCharacters(CharacterInfo characterA, CharacterInfo characterB)
    {
        OverlayTile tileA = characterA.standingOnTile;
        OverlayTile tileB = characterB.standingOnTile;

        // Set the target positions
        Vector3 targetPositionA = tileB.transform.position;
        Vector3 targetPositionB = tileA.transform.position;

        StartCoroutine(MoveCharacter(characterA, targetPositionA));
        StartCoroutine(MoveCharacter(characterB, targetPositionB));

        // Swap the 'standingOnTile' properties of the characters
        characterA.standingOnTile = tileB;
        characterB.standingOnTile = tileA;

        if (characterA.standingOnTile.grid2DLocation.x >= 4)
        {
            characterA.transform.localScale =
                new Vector3(0.2f, characterA.transform.localScale.y, characterA.transform.localScale.z);
        }
        else
        {
            characterA.transform.localScale = new Vector3(-0.2f, characterA.transform.localScale.y,
                characterA.transform.localScale.z);
        }

        if (characterB.standingOnTile.grid2DLocation.x >= 4)
        {
            characterB.transform.localScale =
                new Vector3(0.2f, characterB.transform.localScale.y, characterB.transform.localScale.z);
        }
        else
        {
            characterB.transform.localScale = new Vector3(-0.2f, characterB.transform.localScale.y,
                characterB.transform.localScale.z);
        }
    }

    IEnumerator MoveCharacter(CharacterInfo character, Vector3 targetPosition)
    {
        while (character.transform.position != targetPosition)
        {
            character.transform.position =
                Vector3.MoveTowards(character.transform.position, targetPosition, Time.deltaTime * 10);
            yield return null;
        }
    }

    private void MoveCharacterToTile(CharacterInfo character, OverlayTile targetTile)
    {
        character.transform.position = targetTile.transform.position;
        character.standingOnTile = targetTile;

        // Adjust local scale based on grid X value
        if (targetTile.grid2DLocation.x >= 4)
        {
            character.transform.localScale =
                new Vector3(0.2f, character.transform.localScale.y, character.transform.localScale.z);
        }
        else
        {
            character.transform.localScale =
                new Vector3(-0.2f, character.transform.localScale.y, character.transform.localScale.z);
        }
    }
}