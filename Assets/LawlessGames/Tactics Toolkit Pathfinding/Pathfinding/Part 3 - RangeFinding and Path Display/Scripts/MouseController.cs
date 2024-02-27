using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

namespace finished3
{
    public class MouseController : MonoBehaviour
    {
        public float speed;
        public int movementRange = 300;
        private Dictionary<CharacterInfo, CharacterState> characters = new Dictionary<CharacterInfo, CharacterState>();
        private PathFinder pathFinder;

        private void Start()
        {
            pathFinder = new PathFinder();
        }

        void Update()
        {
            // Example update loop functionality
            if (Input.GetKeyDown(KeyCode.Space)) // Move all characters to random cells
            {
                foreach (var character in characters.Keys)
                {
                    MoveToRandomCell(character);
                    break;
                }
            }

            // Movement logic
            foreach (var character in characters)
            {
                if (character.Value.isMoving && character.Value.path.Count > 0)
                {
                    MoveAlongPath(character.Key, character.Value);
                }
            }
        }

        public void AddCharacter(CharacterInfo character, Vector2Int? gridLocation = null)
        {
            OverlayTile startingTile = null;

            // Loop until an unblocked starting tile is found
            while (startingTile == null || startingTile.isBlocked)
            {
                if (gridLocation == null)
                {
                    gridLocation = new Vector2Int(Random.Range(0, 4), Random.Range(0, 6));
                }

                startingTile = MapManager.Instance.map[gridLocation.Value];

                if (startingTile.isBlocked)
                {
                    // If the selected tile is blocked, nullify gridLocation to pick a new random location in the next iteration
                    gridLocation = null;
                }
            }

            startingTile.isBlocked = true;
            PositionCharacterOnTile(character, startingTile);

            // Set initial local scale for character orientation
            character.transform.localScale = new Vector3(gridLocation.Value.x < 4 ? -0.2f : 0.2f,
                character.transform.localScale.y, character.transform.localScale.z);

            characters[character] = new CharacterState
            {
                path = new List<OverlayTile>(),
                isMoving = false
            };
        }


        private void MoveToRandomCell(CharacterInfo character)
        {
            var state = characters[character];
            if (!state.isMoving)
            {
                character.standingOnTile.isBlocked = false;
                List<OverlayTile> rangeFinderTiles = GetInRangeTiles(character);

                // Ensure that there's at least one unblocked tile to move to
                if (rangeFinderTiles.Any(tile => !tile.isBlocked))
                {
                    OverlayTile targetTile;

                    do
                    {
                        targetTile = rangeFinderTiles[Random.Range(0, rangeFinderTiles.Count)];
                    } while (targetTile.isBlocked); // Keep selecting a random tile until an unblocked one is found

                    float initialScaleX = targetTile.grid2DLocation.x < 4 ? -0.2f : 0.2f;
                    character.transform.localScale = new Vector3(initialScaleX,
                        character.transform.localScale.y, character.transform.localScale.z);

                    // Adjust local scale based on the x position of the target tile
                    if (character.standingOnTile.gridLocation.x < targetTile.gridLocation.x)
                    {
                        character.transform.localScale = new Vector3(-0.2f, character.transform.localScale.y,
                            character.transform.localScale.z);
                    }
                    else
                    {
                        character.transform.localScale = new Vector3(0.2f, character.transform.localScale.y,
                            character.transform.localScale.z);
                    }

                    state.path = pathFinder.FindPath(character.standingOnTile, targetTile, rangeFinderTiles);
                    state.isMoving = true;
                    character.standingOnTile = targetTile;
                    targetTile.isBlocked = true;
                }
            }
            else
            {
                state.isMoving = false;
                MoveToRandomCell(character);
            }
        }


        public void MoveTowardsCharacter(CharacterInfo character, CharacterInfo targetCharacter)
        {
            var state = characters[character];
            if (!state.isMoving)
            {
                OverlayTile targetTile = targetCharacter.standingOnTile;
                state.path = pathFinder.FindPath(character.standingOnTile, targetTile, GetInRangeTiles(character));
                state.isMoving = true;
            }
        }

        private void MoveAlongPath(CharacterInfo character, CharacterState state)
        {
            var step = speed * Time.deltaTime;
            var targetPosition = state.path[0].transform.position;

            character.transform.position = Vector3.MoveTowards(character.transform.position, targetPosition, step);

            if (Vector3.Distance(character.transform.position, targetPosition) < 0.1f)
            {
                PositionCharacterOnTile(character, state.path[0]);
                state.path.RemoveAt(0);

                if (state.path.Count == 0)
                {
                    state.isMoving = false;
                }
            }
        }

        private void PositionCharacterOnTile(CharacterInfo character, OverlayTile tile)
        {
            character.transform.position = tile.transform.position;
            character.standingOnTile = tile;
        }

        private List<OverlayTile> GetInRangeTiles(CharacterInfo character)
        {
            return FindObjectsOfType<OverlayTile>().Where(t => t != character.standingOnTile).ToList();
        }

        private class CharacterState
        {
            public List<OverlayTile> path;
            public bool isMoving;
        }
    }
}