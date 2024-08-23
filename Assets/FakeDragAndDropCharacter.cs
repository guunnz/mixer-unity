using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

public class FakeDragAndDropCharacter : MonoBehaviour
{
    private Camera mainCamera;
    private GameObject selectedCharacter;
    public Transform AxiesParent;

    private List<FakeOverlayTile> allOverlayTiles = new List<FakeOverlayTile>();
    [SerializeField] private List<FakeAxieController> allCharacters = new List<FakeAxieController>();

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
            RaycastHit hit;
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out hit))
            {
                if (hit.collider != null && hit.collider.gameObject.CompareTag("Character"))
                {
                    selectedCharacter = hit.collider.gameObject;
                    FakeAxieController axieController = selectedCharacter.GetComponent<FakeAxieController>();
                    SFXManager.instance.PlaySFX(SFXType.GrabAxie, 0.12f);
                    axieController.Grab(true);
                    selectedCharacter.transform.SetParent(mainCamera.transform);
                }
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
            selectedCharacter.transform.position = newPosition;
        }

        // Release character and attempt to place on the closest tile
        if (selectedCharacter != null && Input.GetMouseButtonUp(0))
        {
            moveDelay = 0.1f;
            selectedCharacter.GetComponent<BoxCollider>().enabled = true;
            var fakeAxieController = selectedCharacter.GetComponent<FakeAxieController>();
            fakeAxieController.Grab(false);
            FakeOverlayTile closestTile = GetClosestTile();
            SFXManager.instance.PlaySFX(SFXType.GrabAxie, 0.12f);
            if (closestTile == null)
            {
                selectedCharacter.GetComponent<BoxCollider>().enabled = true;
                MoveCharacterToTile(fakeAxieController, fakeAxieController.standingOnTile);
                return;
            }

            TryPlaceCharacterOnTile(selectedCharacter, closestTile);

            selectedCharacter.transform.SetParent(AxiesParent); // Unparent the character
            selectedCharacter = null; // Reset the selected character
        }
    }

    private FakeOverlayTile GetClosestTile()
    {
        FakeOverlayTile closestTile = null;
        float minDistance = float.MaxValue;

        if (allOverlayTiles.Count == 0)
        {
            allOverlayTiles = FindObjectsOfType<FakeOverlayTile>().ToList();
        }

        FakeOverlayTile tile = allOverlayTiles.FirstOrDefault(x => x.beingHovered);

        return tile;
    }

    private void TryPlaceCharacterOnTile(GameObject character, FakeOverlayTile targetTile)
    {
        FakeAxieController selectedAxieController = character.GetComponent<FakeAxieController>();

        if (targetTile.occupied)
        {
            FakeAxieController occupyingCharacter = allCharacters.FirstOrDefault(c => c.standingOnTile == targetTile);

            if (occupyingCharacter != null)
            {
                SwapCharacters(selectedAxieController, occupyingCharacter);
            }
            else
            {
                MoveCharacterToTile(selectedAxieController, targetTile);
            }
        }
        else
        {
            MoveCharacterToTile(selectedAxieController, targetTile);
        }
    }

    private void SwapCharacters(FakeAxieController characterA, FakeAxieController characterB)
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

        tileA.currentOccupier = characterB;
        tileB.currentOccupier = characterA;
    }

    IEnumerator MoveCharacter(FakeAxieController character, Vector3 targetPosition)
    {
        while (character.transform.position != targetPosition)
        {
            character.transform.position =
                Vector3.MoveTowards(character.transform.position, targetPosition, Time.deltaTime * 10);
            yield return null;
        }
    }

    private void MoveCharacterToTile(FakeAxieController character, FakeOverlayTile targetTile)
    {
        character.standingOnTile.occupied = false;
        character.standingOnTile.currentOccupier = null;

        character.transform.position = targetTile.transform.position;
        character.standingOnTile = targetTile;
        character.standingOnTile.currentOccupier = character;
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