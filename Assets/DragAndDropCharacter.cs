using System;
using System.Collections;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using finished3;
using UnityEngine.EventSystems;

public class DragAndDropCharacter : MonoBehaviour
{
    private Camera mainCamera;
    private GameObject selectedCharacter;
    private Vector3 originalPosition;
    private OverlayTile originalTile;
    private List<OverlayTile> allOverlayTiles;
    private Team team;
    private float moveDelay = 0.1f;
    private float holdAux = 0.13f;
    private float hold = 0f;
    public AxieStatsTooltip statsToolitp;
    private Vector3 mousePos;
    bool swappingChar1 = false;
    bool swappingChar2 = false;
    void Start()
    {
        mainCamera = Camera.main; // Assuming the main camera is tagged as "MainCamera"
        team = FindObjectsByType<Team>(FindObjectsSortMode.None)
            .Single(x => x.isGoodTeam); // Get the MouseController instance
    }

    void Update()
    {
        // Detect mouse click
        if (team.battleStarted)
        {
            if (Input.GetMouseButtonDown(0))
            {
                DoStats();
            }
            return;
        }

        if (EventSystem.current.IsPointerOverGameObject() && selectedCharacter == null || swappingChar1 || swappingChar2)
            return;

        moveDelay -= Time.deltaTime;
        if (Input.GetMouseButton(0))
        {
            if (mousePos == Vector3.zero)
                mousePos = Input.mousePosition;

            hold += Time.deltaTime;
            if (moveDelay <= 0 && selectedCharacter == null && (hold >= holdAux || mousePos != Input.mousePosition))
            {
                RaycastHit hit;
                Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);

                if (Physics.Raycast(ray, out hit))
                {
                    if (hit.collider != null && hit.collider.gameObject.CompareTag("Character"))
                    {
                        selectedCharacter = hit.collider.gameObject;
                        AxieController axieController = selectedCharacter.GetComponent<AxieController>();
                        if (axieController.mode == AxieMode.Menu)
                        {
                            selectedCharacter = null;
                            return;
                        }

                        SFXManager.instance.PlaySFX(SFXType.GrabAxie);
                        axieController.axieBehavior.DoAction(AxieState.Grabbed);
                        originalPosition = selectedCharacter.transform.position;
                        originalTile = selectedCharacter.GetComponent<AxieController>().standingOnTile;
                        selectedCharacter.transform.SetParent(mainCamera.transform);
                    }
                }
            }
            mousePos = Input.mousePosition;
        }


        // Move character with mouse
        if (selectedCharacter != null && Input.GetMouseButton(0))
        {
            moveDelay = 0.1f;
            selectedCharacter.GetComponent<BoxCollider>().enabled = false;
            Vector3 mousePosition = new Vector3(Input.mousePosition.x, Input.mousePosition.y,
                mainCamera.WorldToScreenPoint(selectedCharacter.transform.position).z);
            Vector3 newPosition = mainCamera.ScreenToWorldPoint(mousePosition);
            selectedCharacter.transform.position = newPosition;
        }

        // Release character and attempt to place on the closest tile
        if (Input.GetMouseButtonUp(0))
        {
            mousePos = Vector3.zero;
            if (hold < holdAux && selectedCharacter == null)
            {
                DoStats();
            }
            else
            {
                if (selectedCharacter != null)
                {
                    moveDelay = 0.1f;
                    selectedCharacter.GetComponent<BoxCollider>().enabled = true;
                    selectedCharacter.GetComponent<AxieController>().axieBehavior.DoAction(AxieState.None);
                    OverlayTile closestTile = GetClosestTile(selectedCharacter.transform.position);

                    SFXManager.instance.PlaySFX(SFXType.GrabAxie, 0.12f);
                    if (closestTile == null)
                    {
                        selectedCharacter.GetComponent<BoxCollider>().enabled = true;
                        selectedCharacter.GetComponent<AxieController>().axieBehavior.DoAction(AxieState.None);
                        MoveCharacterToTile(selectedCharacter.GetComponent<AxieController>(),
                            selectedCharacter.GetComponent<AxieController>().standingOnTile);
                        return;
                    }

                    TryPlaceCharacterOnTile(selectedCharacter, closestTile);

                    selectedCharacter.transform.SetParent(null); // Unparent the character
                    selectedCharacter = null; // Reset the selected character
                }
            }
            hold = 0;
        }
    }

    public void DoStats()
    {
        RaycastHit hit;
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out hit))
        {
            if (hit.collider != null && hit.collider.gameObject.CompareTag("Character"))
            {
                var tappedChar = hit.collider.gameObject;
                AxieController axieController = tappedChar.GetComponent<AxieController>();
                if (axieController.mode == AxieMode.Menu)
                    return;
                SFXManager.instance.PlaySFX(SFXType.UIButtonTap);

                statsToolitp.Enable(axieController);
            }
        }
    }

    private OverlayTile GetClosestTile(Vector3 position)
    {
        OverlayTile closestTile = null;
        float minDistance = float.MaxValue;

        if (allOverlayTiles == null || allOverlayTiles.Count == 0)
        {
            allOverlayTiles = FindObjectsOfType<OverlayTile>().ToList();
        }

        OverlayTile tile = allOverlayTiles.FirstOrDefault(x => x.beingHovered);
        if (tile == null)
        {
            tile = team.GetAliveCharacters().FirstOrDefault(x => x.axieBehavior.axieState == AxieState.Hovered)
                ?.standingOnTile;
        }

        return tile;
    }

    private void TryPlaceCharacterOnTile(GameObject character, OverlayTile targetTile)
    {
        AxieController selectedAxieController = character.GetComponent<AxieController>();

        if (targetTile.occupied)
        {
            var allCharacters = team.GetAliveCharacters();
            AxieController occupyingCharacter = allCharacters.FirstOrDefault(c => c.standingOnTile == targetTile);

            if (occupyingCharacter != null)
            {
                SwapCharacters(selectedAxieController, occupyingCharacter);
                occupyingCharacter.startingCol = occupyingCharacter.standingOnTile.grid2DLocation.y;
                occupyingCharacter.startingRow = occupyingCharacter.standingOnTile.grid2DLocation.x;
            }
            else
            {
                MoveCharacterToTile(selectedAxieController, targetTile);
            }

            selectedAxieController.startingCol = selectedAxieController.standingOnTile.grid2DLocation.y;
            selectedAxieController.startingRow = selectedAxieController.standingOnTile.grid2DLocation.x;
        }
        else
        {
            targetTile.currentOccupier = selectedAxieController;
            MoveCharacterToTile(selectedAxieController, targetTile);
            selectedAxieController.startingCol = selectedAxieController.standingOnTile.grid2DLocation.y;
            selectedAxieController.startingRow = selectedAxieController.standingOnTile.grid2DLocation.x;
        }
    }

    private void SwapCharacters(AxieController characterA, AxieController characterB)
    {
        OverlayTile tileA = characterA.standingOnTile;
        OverlayTile tileB = characterB.standingOnTile;

        // Set the target positions
        Vector3 targetPositionA = tileB.transform.position;
        Vector3 targetPositionB = tileA.transform.position;
        swappingChar1 = true;
        swappingChar2 = true;

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

    IEnumerator MoveCharacter(AxieController character, Vector3 targetPosition)
    {
        float timeMoving = 0;
        while (character.transform.position != targetPosition && timeMoving < 1)
        {
            character.transform.position =
                Vector3.MoveTowards(character.transform.position, targetPosition, Time.deltaTime * 10);

            timeMoving += Time.deltaTime;
            yield return null;
        }
        if (swappingChar1)
        {
            swappingChar1 = false;
        }
        if (swappingChar2)
        {
            swappingChar1 = false;
            swappingChar2 = false;
        }
        character.transform.position = targetPosition;
    }

    private void MoveCharacterToTile(AxieController character, OverlayTile targetTile)
    {
        character.transform.position = targetTile.transform.position;
        character.standingOnTile.occupied = false;
        character.standingOnTile = targetTile;
        character.standingOnTile.occupied = true;
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