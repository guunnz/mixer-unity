using System;
using System.Collections.Generic;
using System.Linq;
using enemies;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

public class Team : MonoBehaviour
{
    public float speed;
    public int movementRange = 300;
    internal Dictionary<AxieController, CharacterState> characters = new Dictionary<AxieController, CharacterState>();
    private PathFinder pathFinder;
    public Team enemyTeam;
    public bool battleStarted = false;
    [FormerlySerializedAs("Timer")] public GameObject BattleOverlay;
    public GameObject IngameOverlay;
    public TextMeshProUGUI YouWinLose;
    public AxieLandBattleTarget target;
    private float resetTimer;
    public bool isGoodTeam;

    internal int AxieAliveAmount => characters.Keys.Count(x => x.axieBehavior.axieState != AxieState.Killed);

    private void Start()
    {
        pathFinder = new PathFinder();
    }

    public List<AxieController> GetCharacters()
    {
        return new List<AxieController>(characters.Keys).Where(x =>
                x.axieBehavior.axieState != AxieState.Killed)
            .ToList();
    }

    public List<AxieController> GetCharactersAll()
    {
        return new List<AxieController>(characters.Keys).ToList();
    }

    public CharacterState GetCharacterState(string axieId)
    {
        return characters[
            GetCharacters().Single(x =>
                x.axieIngameStats.axieId == axieId && x.axieBehavior.axieState != AxieState.Killed)];
    }

    private void RecalculatePath(AxieController character, CharacterState state)
    {
        if (character.CurrentTarget != null)
        {
            state.isMoving = false;
            return;
        }

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

    public void StartBattle()
    {
        battleStarted = true;
        foreach (var inRangeTile in FindObjectsOfType<OverlayTile>())
        {
            inRangeTile.ToggleRectangle(false);
        }
    }

    void RestartTeam()
    {
        IngameOverlay.SetActive(true);
        battleStarted = false;
        enemyTeam.battleStarted = false;
        var enemyList = enemyTeam.GetCharactersAll();

        for (int i = 0; i < enemyList.Count; i++)
        {
            Destroy(enemyList[i].gameObject);
        }

        enemyTeam.characters = new Dictionary<AxieController, CharacterState>();

        foreach (var character in GetCharactersAll())
        {
            character.axieBehavior.axieState = AxieState.Idle;
            character.gameObject.SetActive(true);
            Vector2Int gridLocation = new Vector2Int(character.startingRow, character.startingCol);
            OverlayTile startingTile = MapManager.Instance.map[gridLocation];
            character.transform.localScale = new Vector3(gridLocation.x < 4 ? -0.2f : 0.2f,
                0.2f, character.transform.localScale.z);
            character.axieBehavior.DoAction(AxieState.Idle);
            character.axieSkillEffectManager.RemoveAllEffects();
            character.axieIngameStats.currentHP = character.axieIngameStats.HP;
            character.statsManagerUI.SetHP(character.axieIngameStats.currentHP / character.axieIngameStats.HP);
            character.axieIngameStats.CurrentEnergy = character.axieIngameStats.MinEnergy;
            character.statsManagerUI.SetMana(character.axieIngameStats.CurrentEnergy /
                                             character.axieSkillController.GetComboCost());
            character.SkeletonAnim.Initialize(true);
            PositionCharacterOnTile(character, startingTile, true);
        }
    }

    private void Update()
    {
        if (Input.GetKey(KeyCode.R))
        {
            resetTimer += Time.deltaTime;
        }
        else if (resetTimer != 0)
        {
            resetTimer -= Time.deltaTime;
        }

        if (resetTimer >= 3)
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        if (battleStarted)
        {
            if (characters.All(x => x.Key.axieBehavior.axieState == AxieState.Killed))
            {
                BattleOverlay.SetActive(false);
                YouWinLose.gameObject.SetActive(true);

                if (isGoodTeam)
                {
                    YouWinLose.text = "You Lost.";
                }
                else
                {
                    YouWinLose.text = "You Win!";
                }

                if (Input.GetMouseButtonDown(0))
                {
                    target.PostTeam(RunManagerSingleton.instance.wins + RunManagerSingleton.instance.losses,
                        RunManagerSingleton.instance.goodTeam.GetCharactersAll());
                    foreach (var axieController in RunManagerSingleton.instance.goodTeam.GetCharactersAll())
                    {
                        axieController.axieIngameStats.currentShield = 0;
                        axieController.statsManagerUI.SetShield(Mathf.RoundToInt(0));
                    }

                    if (isGoodTeam)
                    {
                        RunManagerSingleton.instance.SetResult(false);
                    }
                    else
                    {
                        RunManagerSingleton.instance.SetResult(true);
                    }

                    YouWinLose.gameObject.SetActive(false);
                    if (isGoodTeam)
                    {
                        RestartTeam();
                    }
                    else
                    {
                        enemyTeam.RestartTeam();
                    }
                }

                return;
            }
        }
    }

    void FixedUpdate()
    {
        if (battleStarted)
        {
            foreach (var character in characters)
            {
                if (character.Key == null)
                    continue;
                if (character.Key.axieBehavior.axieState == AxieState.Shrimping ||
                    character.Key.axieBehavior.axieState == AxieState.Killed ||
                    character.Key.axieBehavior.axieState == AxieState.Stunned)
                    continue;
                if (character.Value.isMoving && character.Value.path != null && character.Value.path.Count > 0)
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
        if (isGoodTeam)
        {
            int minRange = isGoodTeam ? 0 : 4;
            int maxRange = isGoodTeam ? 4 : 8;
            int index = 0;
            while (startingTile == null || startingTile.occupied)
            {
                if (gridLocation == null)
                {
                    gridLocation = new Vector2Int(0, index);
                }

                startingTile = MapManager.Instance.map[gridLocation.Value];
                if (startingTile.occupied)
                {
                    gridLocation = null;
                    index++;
                }
            }
        }
        else
        {
            gridLocation = new Vector2Int(character.startingRow, character.startingCol);


            startingTile = MapManager.Instance.map[gridLocation.Value];
        }

        PositionCharacterOnTile(character, startingTile);
        character.transform.localScale = new Vector3(gridLocation.Value.x < 4 ? -0.2f : 0.2f,
            0.2f, character.transform.localScale.z);

        character.startingCol = startingTile.grid2DLocation.y;
        character.startingRow = startingTile.grid2DLocation.x;

        characters[character] = new CharacterState
        {
            path = new List<OverlayTile>(),
            isMoving = false
        };
    }


    private bool IsPathPossible(AxieController axieController, AxieController enemy)
    {
        return pathFinder.FindPath(axieController.standingOnTile, enemy.standingOnTile,
            GetInRangeTiles(axieController)) != null;
    }

    private void MoveTowardsClosestCharacter(AxieController character)
    {
        if (character.CurrentTarget != null)
        {
            if (character.Range == 1 && !IsPathPossible(character, character.CurrentTarget))
            {
                character.CurrentTarget = null;
            }
        }

        var state = characters[character];
        if (state.isMoving) return;
        AxieController closestCharacter = character.CurrentTarget;


        if (character.CurrentTarget == null)
        {
            closestCharacter = FindClosestCharacter(character);
        }

        if (closestCharacter != null)
        {
            int distanceToClosestCharacterGrid =
                GetManhattanDistance(character.standingOnTile, closestCharacter.standingOnTile);

            character.CurrentTarget = closestCharacter;
            if (character.axieIngameStats.Range > 1)
            {
                if (distanceToClosestCharacterGrid <= (int)character.axieIngameStats.Range)
                {
                    state.isMoving = false;
                    return;
                }
            }

            OverlayTile targetTile = closestCharacter.standingOnTile;
            state.path = pathFinder.FindPath(character.standingOnTile, targetTile, GetInRangeTiles(character));


            if (state.path == null || state.path.Count == 0)
            {
                var step = speed * Time.fixedDeltaTime;
                if (Vector3.Distance(character.transform.position, character.standingOnTile.transform.position) > 0.1f)
                {
                    character.transform.position = Vector3.MoveTowards(character.transform.position,
                        character.standingOnTile.transform.position, step);
                }
            }


            float distanceToClosestCharacterX =
                Mathf.Abs(character.transform.position.x - closestCharacter.transform.position.x);
            if (distanceToClosestCharacterGrid > character.axieIngameStats.Range + 1 ||
                distanceToClosestCharacterGrid >= 1 &&
                (character.standingOnTile.grid2DLocation.y == closestCharacter.standingOnTile.grid2DLocation.y
                    ? distanceToClosestCharacterX > character.axieIngameStats.Range
                    : distanceToClosestCharacterX > 0.1f ||
                      distanceToClosestCharacterGrid <= character.axieIngameStats.Range + 1))
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

        var characters = enemyTeam.GetCharacters();
        if (!characters.All(x => x.axieSkillEffectManager.IsStenched()))
        {
            characters.RemoveAll(x => x.axieSkillEffectManager.IsStenched());
        }

        foreach (var other in characters)
        {
            int manhattanDistance =
                Mathf.Abs(character.standingOnTile.gridLocation.x - other.standingOnTile.gridLocation.x) +
                Mathf.Abs(character.standingOnTile.gridLocation.z - other.standingOnTile.gridLocation.z);

            if ((int)character.axieIngameStats.Range == 1)
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
                continue;
            }
        }

        return closestCharacter;
    }

    public AxieController FindFurthestCharacter(AxieController character)
    {
        AxieController furthestCharacter = null;
        int maxManhattanDistance = 0;
        var characters = enemyTeam.GetCharacters();

        if (characters.Count > 1)
        {
            characters.RemoveAll(x => x.axieSkillEffectManager.IsStenched());
        }

        foreach (var other in characters)
        {
            int manhattanDistance =
                Mathf.Abs(character.standingOnTile.gridLocation.x - other.standingOnTile.gridLocation.x) +
                Mathf.Abs(character.standingOnTile.gridLocation.z - other.standingOnTile.gridLocation.z);

            if ((int)character.axieIngameStats.Range == 1)
            {
                var path = pathFinder.FindPath(character.standingOnTile, other.standingOnTile,
                    GetInRangeTiles(character));
                if (path == null) continue;
            }

            if (manhattanDistance > maxManhattanDistance)
            {
                maxManhattanDistance = manhattanDistance;
                furthestCharacter = other;
            }
        }

        return furthestCharacter;
    }

    public AxieController FindFurthestCharacter(AxieController character, List<AxieController> potentialCharacters)
    {
        AxieController furthestCharacter = null;
        int maxManhattanDistance = 0;

        foreach (var other in potentialCharacters)
        {
            int manhattanDistance =
                Mathf.Abs(character.standingOnTile.gridLocation.x - other.standingOnTile.gridLocation.x) +
                Mathf.Abs(character.standingOnTile.gridLocation.z - other.standingOnTile.gridLocation.z);

            if ((int)character.axieIngameStats.Range == 1)
            {
                var path = pathFinder.FindPath(character.standingOnTile, other.standingOnTile,
                    GetInRangeTiles(character));
                if (path == null) continue;
            }

            if (manhattanDistance > maxManhattanDistance)
            {
                maxManhattanDistance = manhattanDistance;
                furthestCharacter = other;
            }
        }

        return furthestCharacter;
    }

    private void MoveAlongPath(AxieController character, CharacterState state)
    {
        var step = speed * Time.fixedDeltaTime;
        if (state.path == null)
        {
            state.isMoving = false;
            return;
        }

        int distanceToClosestCharacterGrid = 0;
        if (character.CurrentTarget != null)
        {
            state.path = pathFinder.FindPath(character.standingOnTile, character.CurrentTarget.standingOnTile,
                GetInRangeTiles(character));
            distanceToClosestCharacterGrid =
                GetManhattanDistance(character.standingOnTile, character.CurrentTarget.standingOnTile);
            if (character.axieIngameStats.Range > 1)
            {
                if (distanceToClosestCharacterGrid <= (int)character.axieIngameStats.Range)
                {
                    state.isMoving = false;
                    return;
                }
            }
        }


        if (state.path == null || state.path.Count == 0)
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

    private void PositionCharacterOnTile(AxieController character, OverlayTile tile, bool force = false)
    {
        if (!tile.occupied || force)
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
                character.Range > 1 || !t.occupied &&
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