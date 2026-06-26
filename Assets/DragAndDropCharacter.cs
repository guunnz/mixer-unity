using System;
using System.Collections;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using finished3;
using UnityEngine.EventSystems;

public class DragAndDropCharacter : MonoBehaviour
{
    private const float DragLiftHeight = 0.75f;

    private Camera mainCamera;
    private GameObject selectedCharacter;
    private Vector3 originalPosition;
    private OverlayTile originalTile;
    private List<OverlayTile> allOverlayTiles;
    private Team team;
    private float moveDelay = 0.1f;
    private float holdAux = 0.13f;
    private float hold = 0f;
    public MonsterStatsTooltip statsToolitp;
    private Vector3 mousePos;
    bool swappingChar1 = false;
    bool swappingChar2 = false;
    void Start()
    {
        mainCamera = Camera.main; // Assuming the main camera is tagged as "MainCamera"
        if (statsToolitp == null)
            statsToolitp = FindObjectOfType<MonsterStatsTooltip>(true);

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

        if ((EventSystem.current != null && EventSystem.current.IsPointerOverGameObject() && selectedCharacter == null) || swappingChar1 || swappingChar2)
            return;

        moveDelay -= Time.deltaTime;
        if (Input.GetMouseButton(0))
        {
            if (mousePos == Vector3.zero)
                mousePos = Input.mousePosition;

            hold += Time.deltaTime;
            if (moveDelay <= 0 && selectedCharacter == null && (hold >= holdAux || mousePos != Input.mousePosition))
            {
                if (TryGetCharacterUnderPointer(out MonsterController monsterController))
                {
                    selectedCharacter = monsterController.gameObject;
                    if (monsterController.mode == MonsterMode.Menu)
                    {
                        selectedCharacter = null;
                        return;
                    }

                    SFXManager.instance.PlaySFX(SFXType.GrabMonster);
                    monsterController.monsterBehavior.DoAction(MonsterState.Grabbed);
                    originalPosition = selectedCharacter.transform.position;
                    originalTile = monsterController.standingOnTile;
                    selectedCharacter.transform.SetParent(mainCamera.transform);
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
            selectedCharacter.transform.position = KeepDraggedCharacterAboveFloor(newPosition);
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
                    selectedCharacter.GetComponent<MonsterController>().monsterBehavior.DoAction(MonsterState.None);
                    OverlayTile closestTile = GetClosestTile(selectedCharacter.transform.position);

                    SFXManager.instance.PlaySFX(SFXType.GrabMonster, 0.12f);
                    if (closestTile == null)
                    {
                        selectedCharacter.GetComponent<BoxCollider>().enabled = true;
                        selectedCharacter.GetComponent<MonsterController>().monsterBehavior.DoAction(MonsterState.None);
                        MoveCharacterToTile(selectedCharacter.GetComponent<MonsterController>(),
                            selectedCharacter.GetComponent<MonsterController>().standingOnTile);
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

    private Vector3 KeepDraggedCharacterAboveFloor(Vector3 worldPosition)
    {
        float floorHeight = originalTile != null ? originalTile.transform.position.y : originalPosition.y;
        worldPosition.y = Mathf.Max(worldPosition.y, floorHeight + DragLiftHeight);
        return worldPosition;
    }

    public void DoStats()
    {
        if (mainCamera == null)
            mainCamera = Camera.main;

        if (mainCamera == null)
            return;

        if (statsToolitp == null)
            statsToolitp = FindObjectOfType<MonsterStatsTooltip>(true);

        if (statsToolitp == null)
            return;

        if (!TryGetCharacterUnderPointer(out MonsterController monsterController))
            return;

        if (monsterController.mode == MonsterMode.Menu)
            return;

        SFXManager.instance.PlaySFX(SFXType.UIButtonTap);
        statsToolitp.Enable(monsterController);
    }

    private bool TryGetCharacterUnderPointer(out MonsterController controller)
    {
        controller = null;
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit[] hits = Physics.RaycastAll(ray);
        if (hits == null || hits.Length == 0)
            return false;

        Vector3 pointerPosition = Input.mousePosition;
        controller = hits
            .Where(hit => hit.collider != null && hit.collider.CompareTag("Character"))
            .Select(hit => new
            {
                Hit = hit,
                Controller = hit.collider.GetComponent<MonsterController>()
            })
            .Where(candidate => candidate.Controller != null && candidate.Controller.mode != MonsterMode.Menu)
            .OrderBy(candidate => GetPointerDistance(candidate.Controller, pointerPosition))
            .ThenBy(candidate => candidate.Hit.distance)
            .Select(candidate => candidate.Controller)
            .FirstOrDefault();

        return controller != null;
    }

    private float GetPointerDistance(MonsterController controller, Vector3 pointerPosition)
    {
        Vector3 worldPosition = controller.Visual != null && controller.Visual.BodyAnchor != null
            ? controller.Visual.BodyAnchor.position
            : controller.transform.position;

        Vector3 screenPosition = mainCamera.WorldToScreenPoint(worldPosition);
        return Vector2.SqrMagnitude((Vector2)screenPosition - (Vector2)pointerPosition);
    }

    private OverlayTile GetClosestTile(Vector3 position)
    {
        if (allOverlayTiles == null || allOverlayTiles.Count == 0)
        {
            allOverlayTiles = FindObjectsOfType<OverlayTile>().ToList();
        }

        OverlayTile tile = allOverlayTiles.FirstOrDefault(x => x.beingHovered);
        if (tile == null)
        {
            tile = team.GetAliveCharacters().FirstOrDefault(x => x.monsterBehavior.monsterState == MonsterState.Hovered)
                ?.standingOnTile;
        }

        return tile;
    }

    private void TryPlaceCharacterOnTile(GameObject character, OverlayTile targetTile)
    {
        MonsterController selectedMonsterController = character.GetComponent<MonsterController>();

        if (targetTile.occupied)
        {
            var allCharacters = team.GetAliveCharacters();
            MonsterController occupyingCharacter = allCharacters.FirstOrDefault(c => c.standingOnTile == targetTile);

            if (occupyingCharacter != null)
            {
                SwapCharacters(selectedMonsterController, occupyingCharacter);
                occupyingCharacter.startingCol = occupyingCharacter.standingOnTile.grid2DLocation.y;
                occupyingCharacter.startingRow = occupyingCharacter.standingOnTile.grid2DLocation.x;
            }
            else
            {
                MoveCharacterToTile(selectedMonsterController, targetTile);
            }

            selectedMonsterController.startingCol = selectedMonsterController.standingOnTile.grid2DLocation.y;
            selectedMonsterController.startingRow = selectedMonsterController.standingOnTile.grid2DLocation.x;
        }
        else
        {
            targetTile.currentOccupier = selectedMonsterController;
            MoveCharacterToTile(selectedMonsterController, targetTile);
            selectedMonsterController.startingCol = selectedMonsterController.standingOnTile.grid2DLocation.y;
            selectedMonsterController.startingRow = selectedMonsterController.standingOnTile.grid2DLocation.x;
        }
    }

    private void SwapCharacters(MonsterController characterA, MonsterController characterB)
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
            MonsterScale.SetFacing(characterA.transform, true);
        }
        else
        {
            MonsterScale.SetFacing(characterA.transform, false);
        }

        if (characterB.standingOnTile.grid2DLocation.x >= 4)
        {
            MonsterScale.SetFacing(characterB.transform, true);
        }
        else
        {
            MonsterScale.SetFacing(characterB.transform, false);
        }
    }

    IEnumerator MoveCharacter(MonsterController character, Vector3 targetPosition)
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

    private void MoveCharacterToTile(MonsterController character, OverlayTile targetTile)
    {
        character.transform.position = targetTile.transform.position;
        character.standingOnTile.occupied = false;
        character.standingOnTile = targetTile;
        character.standingOnTile.occupied = true;
        MonsterScale.SetFacing(character.transform, targetTile.grid2DLocation.x >= 4);
    }
}
