using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

public class FakeDragAndDropCharacter : MonoBehaviour
{
    private const float DragLiftHeight = 0.75f;

    private Camera mainCamera;
    private GameObject selectedCharacter;
    private Vector3 originalPosition;
    private FakeOverlayTile originalTile;
    public Transform MonstersParent;

    private List<FakeOverlayTile> allOverlayTiles = new List<FakeOverlayTile>();
    [SerializeField] private List<FakeMonsterController> allCharacters = new List<FakeMonsterController>();

    private float moveDelay = 0.1f;
    void Start()
    {
        mainCamera = Camera.main;
    }

    void Update()
    {
        if (EventSystem.current.IsPointerOverGameObject() && selectedCharacter == null)
            return;
        moveDelay -= Time.deltaTime;
        if (Input.GetMouseButtonDown(0) && moveDelay <= 0)
        {
            if (TryGetCharacterUnderPointer(out FakeMonsterController monsterController))
            {
                selectedCharacter = monsterController.gameObject;
                SFXManager.instance.PlaySFX(SFXType.GrabMonster, 0.12f);
                monsterController.Grab(true);
                originalPosition = selectedCharacter.transform.position;
                originalTile = monsterController.standingOnTile;
                selectedCharacter.transform.SetParent(mainCamera.transform);
            }
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
        if (selectedCharacter != null && Input.GetMouseButtonUp(0))
        {
            moveDelay = 0.1f;
            selectedCharacter.GetComponent<BoxCollider>().enabled = true;
            var fakeMonsterController = selectedCharacter.GetComponent<FakeMonsterController>();
            fakeMonsterController.Grab(false);
            FakeOverlayTile closestTile = GetClosestTile();
            SFXManager.instance.PlaySFX(SFXType.GrabMonster, 0.12f);
            if (closestTile == null)
            {
                MoveCharacterToTile(fakeMonsterController, fakeMonsterController.standingOnTile);
                selectedCharacter.transform.SetParent(MonstersParent);
                selectedCharacter.GetComponent<BoxCollider>().enabled = true;
                selectedCharacter = null;
                return;
            }

            TryPlaceCharacterOnTile(selectedCharacter, closestTile);

            selectedCharacter.transform.SetParent(MonstersParent); // Unparent the character
            selectedCharacter = null; // Reset the selected character
        }
    }

    private Vector3 KeepDraggedCharacterAboveFloor(Vector3 worldPosition)
    {
        float floorHeight = originalTile != null ? originalTile.transform.position.y : originalPosition.y;
        worldPosition.y = Mathf.Max(worldPosition.y, floorHeight + DragLiftHeight);
        return worldPosition;
    }

    private FakeOverlayTile GetClosestTile()
    {
        if (allOverlayTiles.Count == 0)
        {
            allOverlayTiles = FindObjectsOfType<FakeOverlayTile>().ToList();
        }

        FakeOverlayTile tile = allOverlayTiles.FirstOrDefault(x => x.beingHovered);

        return tile;
    }

    private bool TryGetCharacterUnderPointer(out FakeMonsterController controller)
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
                Controller = hit.collider.GetComponent<FakeMonsterController>()
            })
            .Where(candidate => candidate.Controller != null && candidate.Controller.visible)
            .OrderBy(candidate => GetPointerDistance(candidate.Controller, pointerPosition))
            .ThenBy(candidate => candidate.Hit.distance)
            .Select(candidate => candidate.Controller)
            .FirstOrDefault();

        return controller != null;
    }

    private float GetPointerDistance(FakeMonsterController controller, Vector3 pointerPosition)
    {
        Vector3 worldPosition = controller.visual != null && controller.visual.BodyAnchor != null
            ? controller.visual.BodyAnchor.position
            : controller.transform.position;

        Vector3 screenPosition = mainCamera.WorldToScreenPoint(worldPosition);
        return Vector2.SqrMagnitude((Vector2)screenPosition - (Vector2)pointerPosition);
    }

    private void TryPlaceCharacterOnTile(GameObject character, FakeOverlayTile targetTile)
    {
        FakeMonsterController selectedMonsterController = character.GetComponent<FakeMonsterController>();

        if (targetTile.occupied)
        {
            FakeMonsterController occupyingCharacter = allCharacters.FirstOrDefault(c => c.standingOnTile == targetTile);

            if (occupyingCharacter != null && selectedMonsterController != occupyingCharacter)
            {
                SwapCharacters(selectedMonsterController, occupyingCharacter);
            }
            else
            {
                MoveCharacterToTile(selectedMonsterController, targetTile);
            }
        }
        else
        {
            MoveCharacterToTile(selectedMonsterController, targetTile);
        }
    }

    private void SwapCharacters(FakeMonsterController characterA, FakeMonsterController characterB)
    {
        FakeOverlayTile tileA = characterA.standingOnTile;
        FakeOverlayTile tileB = characterB.standingOnTile;

        // Set the target positions
        Vector3 targetPositionA = tileB.transform.position;
        Vector3 targetPositionB = tileA.transform.position;

        StartCoroutine(MoveCharacter(characterA, targetPositionA));
        StartCoroutine(MoveCharacter(characterB, targetPositionB));

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

        tileA.currentOccupier = characterB;
        tileB.currentOccupier = characterA;
    }

    IEnumerator MoveCharacter(FakeMonsterController character, Vector3 targetPosition)
    {
        while (character.transform.position != targetPosition)
        {
            character.transform.position =
                Vector3.MoveTowards(character.transform.position, targetPosition, Time.deltaTime * 10);
            yield return null;
        }
    }

    private void MoveCharacterToTile(FakeMonsterController character, FakeOverlayTile targetTile)
    {
        character.standingOnTile.occupied = false;
        character.standingOnTile.currentOccupier = null;

        character.transform.position = targetTile.transform.position;
        character.standingOnTile = targetTile;
        character.standingOnTile.currentOccupier = character;
        character.standingOnTile.occupied = true;

        MonsterScale.SetFacing(character.transform, targetTile.grid2DLocation.x >= 4);
    }
}
