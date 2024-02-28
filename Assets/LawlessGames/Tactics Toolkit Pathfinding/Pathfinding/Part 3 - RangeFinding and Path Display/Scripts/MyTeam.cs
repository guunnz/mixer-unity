using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace finished3
{
    public class MyTeam : MonoBehaviour
    {
        public float speed;
        public int movementRange = 300;
        private Dictionary<CharacterInfo, CharacterState> characters = new Dictionary<CharacterInfo, CharacterState>();
        private PathFinder pathFinder;
        public EnemyTeam enemyTeam;

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

        private void RecalculatePath(CharacterInfo character, CharacterState state)
        {
            CharacterInfo closestCharacter = FindClosestCharacter(character);
            if (closestCharacter != null)
            {
                character.CurrentTarget = closestCharacter;
                OverlayTile targetTile = closestCharacter.standingOnTile;

                state.path = pathFinder.FindPath(character.standingOnTile, targetTile, GetInRangeTiles(character));
                state.isMoving = state.path.Count > 0;
            }
            else
            {
                state.isMoving = false;
            }
        }

  
        void Update()
        {
            if (Input.GetKey(KeyCode.H)) // Move all characters
            {
                foreach (var character in characters)
                {
                    if (character.Value.isMoving && character.Value.path.Count > 0)
                    {
                        // Check if next tile in path is occupied
                        if (!character.Value.path[0].occupied)
                        {
                            MoveAlongPath(character.Key, character.Value);
                        }
                        else
                        {
                            // Recalculate path if next tile is occupied
                            RecalculatePath(character.Key, character.Value);
                        }
                    }
                    else if (!character.Value.isMoving)
                    {
                        MoveTowardsClosestCharacter(character.Key);
                    }
                }
            }
        }

        public void AddCharacter(CharacterInfo character, Vector2Int? gridLocation = null)
        {
            OverlayTile startingTile = null;

            while (startingTile == null || startingTile.occupied)
            {
                if (gridLocation == null)
                {
                    gridLocation = new Vector2Int(Random.Range(0, 4), Random.Range(0, 5));
                }

                startingTile = MapManager.Instance.map[gridLocation.Value];
                if (startingTile.occupied)
                {
                    gridLocation = null;
                }
            }

            PositionCharacterOnTile(character, startingTile);
            character.transform.localScale = new Vector3(gridLocation.Value.x < 4 ? -0.2f : 0.2f, character.transform.localScale.y, character.transform.localScale.z);

            characters[character] = new CharacterState
            {
                path = new List<OverlayTile>(),
                isMoving = false
            };
        }

        private void MoveTowardsClosestCharacter(CharacterInfo character)
        {
            var state = characters[character];
            if (state.isMoving) return;

            CharacterInfo closestCharacter = FindClosestCharacter(character);
            if (closestCharacter != null)
            {
                OverlayTile targetTile = closestCharacter.standingOnTile;
                state.path = pathFinder.FindPath(character.standingOnTile, targetTile, GetInRangeTiles(character));

                character.CurrentTarget = closestCharacter;

                // Check if the closest character is already at the minimum allowed distance
                float distanceToClosestCharacter = Vector3.Distance(character.transform.position, closestCharacter.transform.position);
                int distanceToClosestCharacterGrid = GetManhattanDistance(character.standingOnTile, closestCharacter.standingOnTile);
                if (distanceToClosestCharacter > 0.9f && distanceToClosestCharacterGrid >= 1 && distanceToClosestCharacterGrid > character.Range)
                {
                    MoveAlongPath(character, state);
                }
                else
                {
                    // If already at minimum distance, clear the path and stop moving
                    state.path.Clear();
                    state.isMoving = false;
                }
            }
        }

        private int GetManhattanDistance(OverlayTile tile1, OverlayTile tile2)
        {
            return Mathf.Abs(tile1.gridLocation.x - tile2.gridLocation.x) + Mathf.Abs(tile1.gridLocation.z - tile2.gridLocation.z);
        }



        private CharacterInfo FindClosestCharacter(CharacterInfo character)
        {
            CharacterInfo closestCharacter = null;
            int minPathLength = int.MaxValue;

            foreach (var other in enemyTeam.GetCharacters())
            {
                if (other == character) continue; // Skip the same character

                List<OverlayTile> pathToCharacter = pathFinder.FindPath(character.standingOnTile, other.standingOnTile, GetInRangeTiles(character));

                // Check path length
                if (pathToCharacter.Count > 0 && pathToCharacter.Count < minPathLength)
                {
                    minPathLength = pathToCharacter.Count;
                    closestCharacter = other;
                }
            }

            return closestCharacter;
        }


        private void MoveAlongPath(CharacterInfo character, CharacterState state)
        { if (state.path.Count == 0)
            {
                state.isMoving = false;
                return;
            }
            var step = speed * Time.deltaTime;
            var targetPosition = state.path[0].transform.position;

            character.transform.position = Vector3.MoveTowards(character.transform.position, targetPosition, step);

            if (Vector3.Distance(character.transform.position, targetPosition) < 0.1f)
            {
                if (state.path.Count == 0)
                {
                    state.isMoving = false;
                    return;
                }
                if (!state.path[0].occupied)
                {
                    PositionCharacterOnTile(character, state.path[0]);
                    state.path.RemoveAt(0);
                }
                else
                {
                    state.isMoving = false; // Stop moving if next tile is occupied
                    RecalculatePath(character, state);
                }
            }
        }

        private void PositionCharacterOnTile(CharacterInfo character, OverlayTile tile)
        {
            if (!tile.occupied)
            {
                character.transform.position = tile.transform.position;
                tile.occupied = true;
                character.standingOnTile = tile;
            }
        }
        private List<OverlayTile> GetInRangeTiles(CharacterInfo character)
        {
            return FindObjectsOfType<OverlayTile>().Where(t => 
                    !t.occupied && 
                    Vector2Int.Distance(new Vector2Int(t.gridLocation.x, t.gridLocation.z), 
                        new Vector2Int(character.standingOnTile.gridLocation.x, character.standingOnTile.gridLocation.z)) <= movementRange)
                .ToList();
        }

        public class CharacterState
        {
            public List<OverlayTile> path;
            public bool isMoving;
        }
    }
}
