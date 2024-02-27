using System;
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
        public List<CharacterInfo> GetCharacters()
        {
            return new List<CharacterInfo>(characters.Keys);
        }
        
        public CharacterState GetCharacterState(string axieId)
        {
            return characters[GetCharacters().Single(x => x.axieId == axieId)];
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

            if (Input.GetKeyDown(KeyCode.C)) // Move all characters to random cells
            {
                foreach (var character in characters.Keys)
                {
                    MoveTowardsCharacter(character.axieId,
                        characters.Keys.ToList()[Random.Range(0, characters.Keys.Count)].axieId);
                    break;
                }
            }

            if (Input.GetKeyDown(KeyCode.H)) // Move all characters to random cells
            {
                foreach (var character in characters.Keys)
                {
                    MoveTowardsClosestCharacter(character);
                    break; // Remove this break if you want all characters to move towards their closest character
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
                    gridLocation = new Vector2Int(Random.Range(0, 4), Random.Range(0, 5));
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


        public void MoveTowardsCharacter(string character, string targetCharacter)
        {
            CharacterInfo myCharacter = characters.Single(x => x.Key.axieId == character).Key;
            CharacterInfo theTargetCharacter = characters.Single(x => x.Key.axieId == targetCharacter).Key;
            var state = characters[myCharacter];
            if (character == targetCharacter)
            {
                state.isMoving = false;
                return;
            }

            if (!state.isMoving)
            {
                myCharacter.standingOnTile.isBlocked = false;
                OverlayTile targetTile = theTargetCharacter.standingOnTile;
                state.path = pathFinder.FindPath(myCharacter.standingOnTile, targetTile, GetInRangeTiles(myCharacter));
                float initialScaleX = targetTile.grid2DLocation.x < 4 ? -0.2f : 0.2f;
                myCharacter.transform.localScale = new Vector3(initialScaleX,
                    myCharacter.transform.localScale.y, myCharacter.transform.localScale.z);


                // Adjust local scale based on the x position of the target tile
                if (myCharacter.standingOnTile.gridLocation.x < targetTile.gridLocation.x)
                {
                    myCharacter.transform.localScale = new Vector3(-0.2f, myCharacter.transform.localScale.y,
                        myCharacter.transform.localScale.z);
                }
                else
                {
                    myCharacter.transform.localScale = new Vector3(0.2f, myCharacter.transform.localScale.y,
                        myCharacter.transform.localScale.z);
                }

                myCharacter.standingOnTile = state.path.Last();
                myCharacter.standingOnTile.isBlocked = true;
                state.isMoving = true;
            }
            else
            {
                state.isMoving = false;
                MoveTowardsCharacter(character, targetCharacter);
            }
        }

        private void MoveTowardsClosestCharacter(CharacterInfo character)
        {
            var state = characters[character];
            if (state.isMoving) return; // If already moving, do nothing

            character.standingOnTile.isBlocked = false;

            // Find the closest character
            CharacterInfo closestCharacter = null;
            int minDistance = int.MaxValue;
            foreach (var other in characters.Keys)
            {
                if (other == character) continue; // Skip the same character

                int distance =
                    Mathf.Abs(character.standingOnTile.gridLocation.x - other.standingOnTile.gridLocation.x) +
                    Mathf.Abs(character.standingOnTile.gridLocation.z - other.standingOnTile.gridLocation.z);

                if (distance < minDistance)
                {
                    minDistance = distance;
                    closestCharacter = other;
                }
            }

            if (closestCharacter != null)
            {
                // Generate path towards closest character
                OverlayTile targetTile = closestCharacter.standingOnTile;
                state.path = pathFinder.FindPath(character.standingOnTile, targetTile, GetInRangeTiles(character));

                // Set the current target
                character.CurrentTarget = closestCharacter;

                // If character is already within the preferred range, no need to move
                if (minDistance <= character.Range)
                {
                    state.path.Clear();
                    state.isMoving = false;
                }
                else
                {
                    // Adjust the path to stop at a distance equal to the range
                    int stepsToMove = minDistance - character.Range;
                    if (stepsToMove < state.path.Count)
                    {
                        state.path = state.path.GetRange(0, stepsToMove);
                    }

                    // Adjust the final standing on tile
                    character.standingOnTile = state.path.Last();
                    character.standingOnTile.isBlocked = true;
                    state.isMoving = true;
                }
            }
        }


        private void MoveAlongPath(CharacterInfo character, CharacterState state)
        {
            var step = speed * Time.deltaTime;
            var targetPosition = state.path[0].transform.position;

            // Determine the direction of movement to adjust the scale
            if (character.transform.position.x < targetPosition.x)
            {
                // Moving right
                character.transform.localScale = new Vector3(-0.2f, character.transform.localScale.y,
                    character.transform.localScale.z);
            }
            else if (character.transform.position.x > targetPosition.x)
            {
                // Moving left
                character.transform.localScale = new Vector3(0.2f, character.transform.localScale.y,
                    character.transform.localScale.z);
            }

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

        public class CharacterState
        {
            public List<OverlayTile> path;
            public bool isMoving;
        }
    }
}