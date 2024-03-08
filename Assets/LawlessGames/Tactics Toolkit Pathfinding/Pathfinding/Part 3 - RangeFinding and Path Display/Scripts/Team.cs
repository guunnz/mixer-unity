using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Team : MonoBehaviour
{
    public float speed;
    public int movementRange = 300;
    private Dictionary<AxieController, CharacterState> characters = new Dictionary<AxieController, CharacterState>();
    private PathFinder pathFinder;
    public Team enemyTeam;
    public bool battleStarted = false;

    public AxieLandBattleTarget target;

    // Boolean flag to differentiate between good and bad teams
    public bool isGoodTeam;

    private void Start()
    {
        pathFinder = new PathFinder();
    }

    public List<AxieController> GetCharacters()
    {
        return new List<AxieController>(characters.Keys).Where(x => x.axieBehavior.axieState != AxieState.Killed)
            .ToList();
    }

    public CharacterState GetCharacterState(string axieId)
    {
        return characters[GetCharacters().Single(x => x.spawnedAxie.axieId == axieId)];
    }


    private void RecalculatePath(AxieController character, CharacterState state)
    {
        AxieController closestCharacter = FindClosestCharacter(character);
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
            battleStarted = true;
            foreach (var inRangeTile in FindObjectsOfType<OverlayTile>())
            {
                inRangeTile.ToggleRectangle(false);
            }
        }

        if (Input.GetKeyDown(KeyCode.L)) // Move all characters
        {
            target.PostTeam(1, characters.Select(x => x.Key.spawnedAxie).ToList());
        }

        if (battleStarted)
        {
            foreach (var character in characters)
            {
                if (character.Key == null)
                    continue;
                if (character.Key.axieBehavior.axieState == AxieState.Shrimping)
                    continue;
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
                else
                {
                    character.Value.isMoving = false;
                }
            }
        }
    }

    public void AddCharacter(AxieController character, Vector2Int? gridLocation = null)
    {
        OverlayTile startingTile = null;

        int index = 0;
        while (startingTile == null || startingTile.occupied)
        {
            if (gridLocation == null)
            {
                int minRange = isGoodTeam ? 0 : 4;
                int maxRange = isGoodTeam ? 4 : 8;
                gridLocation = new Vector2Int(Random.Range(minRange, maxRange), Random.Range(0, 5));
            }

            startingTile = MapManager.Instance.map[gridLocation.Value];
            if (startingTile.occupied)
            {
                gridLocation = null;
                index++;
            }
        }

        PositionCharacterOnTile(character, startingTile);
        character.transform.localScale = new Vector3(gridLocation.Value.x < 4 ? -0.2f : 0.2f,
            character.transform.localScale.y, character.transform.localScale.z);

        characters[character] = new CharacterState
        {
            path = new List<OverlayTile>(),
            isMoving = false
        };
    }

    private void MoveTowardsClosestCharacter(AxieController character)
    {
        var state = characters[character];
        if (state.isMoving) return;

        AxieController closestCharacter = FindClosestCharacter(character);
        if (closestCharacter != null)
        {
            OverlayTile targetTile = closestCharacter.standingOnTile;
            state.path = pathFinder.FindPath(character.standingOnTile, targetTile, GetInRangeTiles(character));
            int distanceToClosestCharacterGrid =
                GetManhattanDistance(character.standingOnTile, closestCharacter.standingOnTile);

            character.CurrentTarget = closestCharacter;
            if (character.spawnedAxie.Range > 1)
            {
                if (distanceToClosestCharacterGrid <= (int)character.spawnedAxie.Range)
                {
                    state.isMoving = false;
                    return;
                }
            }

            float distanceToClosestCharacterX =
                Mathf.Abs(character.transform.position.x - closestCharacter.transform.position.x);
            if (distanceToClosestCharacterGrid > character.spawnedAxie.Range + 1 ||
                distanceToClosestCharacterGrid >= 1 &&
                (character.standingOnTile.grid2DLocation.y == closestCharacter.standingOnTile.grid2DLocation.y
                    ? distanceToClosestCharacterX > character.spawnedAxie.Range
                    : distanceToClosestCharacterX > 0.1f ||
                      distanceToClosestCharacterGrid <= character.spawnedAxie.Range + 1))
            {
                MoveAlongPath(character, state);
            }
            else
            {
                if (state.path == null)
                {
                    state.isMoving = false;
                    return;
                }

                state.path.Clear();
            }
        }
    }

    private int GetManhattanDistance(OverlayTile tile1, OverlayTile tile2)
    {
        return Mathf.Abs(tile1.gridLocation.x - tile2.gridLocation.x) +
               Mathf.Abs(tile1.gridLocation.z - tile2.gridLocation.z);
    }

    private AxieController FindClosestCharacter(AxieController character)
    {
        AxieController closestCharacter = null;
        int minManhattanDistance = int.MaxValue;
        float minTransformDistance = float.MaxValue;

        foreach (var other in enemyTeam.GetCharacters())
        {
            int manhattanDistance =
                Mathf.Abs(character.standingOnTile.gridLocation.x - other.standingOnTile.gridLocation.x) +
                Mathf.Abs(character.standingOnTile.gridLocation.z - other.standingOnTile.gridLocation.z);

            if ((int)character.spawnedAxie.Range == 1)
            {
                var path = pathFinder.FindPath(character.standingOnTile, other.standingOnTile,
                    GetInRangeTiles(character));
                if (path == null) continue;
            }

            if (manhattanDistance < minManhattanDistance)
            {
                minManhattanDistance = manhattanDistance;
                minTransformDistance = Vector3.Distance(character.transform.position, other.transform.position);
                closestCharacter = other;
            }
            else if (manhattanDistance == minManhattanDistance)
            {
                float transformDistance = Vector3.Distance(character.transform.position, other.transform.position);
                if (transformDistance < minTransformDistance)
                {
                    minTransformDistance = transformDistance;
                    closestCharacter = other;
                }
            }
        }

        return closestCharacter;
    }

    private void MoveAlongPath(AxieController character, CharacterState state)
    {
        AxieController closestCharacter = FindClosestCharacter(character);
        if (closestCharacter != null)
        {
            OverlayTile targetTile = closestCharacter.standingOnTile;
            state.path = pathFinder.FindPath(character.standingOnTile, targetTile, GetInRangeTiles(character));
            int distanceToClosestCharacterGrid =
                GetManhattanDistance(character.standingOnTile, closestCharacter.standingOnTile);

            character.CurrentTarget = closestCharacter;
            if (character.spawnedAxie.Range > 1)
            {
                if (distanceToClosestCharacterGrid <= (int)character.spawnedAxie.Range)
                {
                    state.isMoving = false;
                    return;
                }
            }
        }

        var step = speed * Time.deltaTime;
        if (state.path == null)
        {
            state.isMoving = false;
            return;
        }

        if (state.path.Count == 0)
        {
            character.transform.position = Vector3.MoveTowards(character.transform.position,
                character.standingOnTile.transform.position, step);
            state.isMoving = false;
            return;
        }

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
                state.isMoving = false;
                RecalculatePath(character, state);
                return;
            }
        }
        else
        {
            state.isMoving = true;
        }
    }

    private void PositionCharacterOnTile(AxieController character, OverlayTile tile)
    {
        if (!tile.occupied)
        {
            character.transform.position = tile.transform.position;
            tile.occupied = true;
            tile.currentOccupier = character;
            character.standingOnTile = tile;
        }
    }

    public List<OverlayTile> GetInRangeTiles(AxieController character)
    {
        return FindObjectsOfType<OverlayTile>().Where(t =>
                !t.occupied &&
                Vector2Int.Distance(new Vector2Int(t.gridLocation.x, t.gridLocation.z),
                    new Vector2Int(character.standingOnTile.gridLocation.x,
                        character.standingOnTile.gridLocation.z)) <= movementRange)
            .ToList();
    }

    public class CharacterState
    {
        public List<OverlayTile> path;
        public bool isMoving;
    }
}