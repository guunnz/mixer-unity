using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using enemies;
using TMPro;
using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;
using UnityEngine.TextCore.Text;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Team : MonoBehaviour
{
    public float speed;
    public int movementRange = 300;

    internal Dictionary<MonsterController, CharacterState> characters = new Dictionary<MonsterController, CharacterState>();
    private PathFinder pathFinder;
    public Team enemyTeam;
    public bool battleStarted = false;
    public GameObject BattleOverlay;
    public GameObject IngameOverlay;
    public GameObject PostBattleContainer;
    public TextMeshProUGUI YouWinLose;
    public TextMeshProUGUI DetailsText;
    public Image DetailsImage;
    public MonsterLandBattleTarget target;
    private float resetTimer;
    internal bool ChimeraSpawned;
    public bool isGoodTeam;
    public LandType landType;
    internal bool battleEnded = false;
    private TeamCaptainManager teamCaptainManager;
    public List<Action> OnBattleStartActions = new List<Action>();
    internal int MonsterAliveAmount
    {
        get
        {
            int count = 0;
            foreach (MonsterController character in characters.Keys)
            {
                if (IsAlive(character))
                    count++;
            }

            return count;
        }
    }

    public IEnumerator Start()
    {
        pathFinder = new PathFinder();
        while (teamCaptainManager == null)
        {

            teamCaptainManager = TeamCaptainManager.Instance;
            yield return null;
        }
    }

    public List<MonsterController> GetAliveCharacters()
    {
        List<MonsterController> aliveCharacters = new List<MonsterController>(characters.Count);
        foreach (MonsterController character in characters.Keys)
        {
            if (IsAlive(character))
                aliveCharacters.Add(character);
        }

        return aliveCharacters;
    }

    public List<MonsterController> GetAliveCharactersByClass(MonsterClass @class)
    {
        List<MonsterController> aliveCharacters = new List<MonsterController>(characters.Count);
        foreach (MonsterController character in characters.Keys)
        {
            if (IsAlive(character) && character.monsterIngameStats.monsterClass == @class)
                aliveCharacters.Add(character);
        }

        return aliveCharacters;
    }


    public List<MonsterController> GetCharactersAll()
    {
        return new List<MonsterController>(characters.Keys).ToList();
    }

    public List<MonsterController> GetCharactersAllByClass(MonsterClass @class)
    {
        return new List<MonsterController>(characters.Keys).Where(x => x.monsterIngameStats.monsterClass == @class).ToList();
    }

    public CharacterState GetCharacterState(string monsterId)
    {
        foreach (KeyValuePair<MonsterController, CharacterState> pair in characters)
        {
            MonsterController character = pair.Key;
            if (IsAlive(character) && character.monsterIngameStats.monsterId == monsterId)
                return pair.Value;
        }

        throw new InvalidOperationException($"No alive monster state found for id {monsterId}.");
    }

    private static bool IsAlive(MonsterController character)
    {
        return character != null &&
               character.monsterBehavior != null &&
               character.monsterBehavior.monsterState != MonsterState.Killed;
    }

    private void RecalculatePath(MonsterController character, CharacterState state)
    {
        if (character.CurrentTarget != null)
        {
            state.isMoving = false;
            return;
        }

        MonsterController closestCharacter = FindClosestCharacter(character);
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
        this.GetCharactersAll().ForEach(x => x.monsterIngameStats.CurrentEnergy = MonsterStatCalculator.GetMonsterMinEnergy(x.stats) / x.monsterSkillController.GetComboCost());
        int randomMusic = Random.Range(0, 5);
        PostBattleManager.Instance.FillList(this);
        if (isGoodTeam)
        {
            switch (randomMusic)
            {
                case 0:
                    MusicManager.Instance.PlayMusic(MusicTrack.Shldslep);
                    break;
                case 1:
                    MusicManager.Instance.PlayMusic(MusicTrack.GO);
                    break;
                case 2:
                    MusicManager.Instance.PlayMusic(MusicTrack.PunchiEpic);
                    break;
                case 3:
                    MusicManager.Instance.PlayMusic(MusicTrack.NeeEtheAhPehro);
                    break;
                case 4:
                    MusicManager.Instance.PlayMusic(MusicTrack.Laingved);
                    break;
            }
        }


        battleEnded = false;
        ChimeraSpawned = false;
        battleStarted = true;
        foreach (var inRangeTile in FindObjectsOfType<OverlayTile>())
        {
            inRangeTile.ToggleRectangle(false);
        }

        foreach (var monsterController in GetAliveCharacters())
        {
            monsterController.DoOnStart();
        }

        foreach (var onBattleStartAction in OnBattleStartActions)
        {
            onBattleStartAction.Invoke();
        }
    }

    void RestartTeam()
    {
        var skillList = FindObjectsOfType<Skill>();
        for (int i = 0; i < skillList.Length; i++)
        {
            Destroy(skillList[i].gameObject);
        }
        IngameOverlay.SetActive(true);
        battleStarted = false;
        enemyTeam.battleStarted = false;
        var enemyList = enemyTeam.GetCharactersAll();

        for (int i = 0; i < enemyList.Count; i++)
        {
            enemyList[i].standingOnTile.occupied = false;
            enemyList[i].standingOnTile.currentOccupier = null;
            Destroy(enemyList[i].gameObject);
        }

        enemyTeam.characters = new Dictionary<MonsterController, CharacterState>();

        foreach (var character in GetCharactersAll())
        {
            character.monsterBehavior.monsterState = MonsterState.Idle;
            character.gameObject.SetActive(true);
            Vector2Int gridLocation = new Vector2Int(character.startingRow, character.startingCol);
            character.standingOnTile.occupied = false;
            character.standingOnTile.currentOccupier = null;
            OverlayTile startingTile = MapManager.Instance.map[gridLocation];
            character.transform.localScale = Vector3.one;
            MonsterScale.SetFacing(character.transform, gridLocation.x >= 4);
            character.monsterBehavior.DoAction(MonsterState.Idle);
            character.monsterSkillEffectManager.RemoveAllEffects();
            character.monsterIngameStats.currentHP = character.monsterIngameStats.maxHP;
            character.statsManagerUI.SetHP(character.monsterIngameStats.currentHP / character.monsterIngameStats.maxHP);
            character.monsterIngameStats.CurrentEnergy = MonsterStatCalculator.GetMonsterMinEnergy(character.stats) / character.monsterSkillController.GetComboCost();
            character.statsManagerUI.SetMana(character.monsterIngameStats.CurrentEnergy /
                                             character.monsterSkillController.GetComboCost());
            character.Visual?.Initialize(true);
            PositionCharacterOnTile(character, startingTile, true);
        }
    }

    internal void PostTeam()
    {
        target.PostTeam(RunManagerSingleton.instance.wins + RunManagerSingleton.instance.losses,
            RunManagerSingleton.instance.goodTeam.GetCharactersAll());
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
            Loading.instance.EnableLoading();
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        if (battleStarted)
        {
            if (isGoodTeam)
            {
                teamCaptainManager.myTeamHP.fillAmount = GetTeamTotalHPBar();
            }
            else
            {
                teamCaptainManager.opponentHP.fillAmount = GetTeamTotalHPBar();
            }

            if (characters.All(x => x.Key.monsterBehavior.monsterState == MonsterState.Killed))
            {
                RunManagerSingleton.instance.ShopBlocker.SetActive(true);
                if (!battleEnded)
                {
                    EnemyLandAnimation.instance.ResetAnimation();
                    battleEnded = true;
                    FightManagerSingleton.Instance.StopFight();
                    BattleOverlay.SetActive(false);
                    YouWinLose.gameObject.SetActive(true);

                    //hacer color con battledetails
                    Color DetailsTextColor = DetailsText.color;
                    Color DetailsColor = DetailsImage.color;

                    Color youwinlosecolor = YouWinLose.color;
                    YouWinLose.color = Color.clear;
                    DetailsImage.color = Color.clear;
                    DetailsText.color = Color.clear;
                    if (isGoodTeam)
                    {
                        EndOfRunResults.Instance.SetMatchData(false);
                        Debug.Log("SFX PLAYED");
                        SFXManager.instance.PlaySFX(SFXType.Lost, 0.12f, false);
                        MusicManager.Instance.Stop();

                        RunManagerSingleton.instance.SetResultUI(!isGoodTeam);
                        YouWinLose.text = "You Lost.\n<size=12>[Click to continue]";
                        YouWinLose.DOColor(youwinlosecolor, 1);
                        DetailsImage.DOColor(DetailsColor, 1);
                        DetailsText.DOColor(DetailsTextColor, 1);
                    }
                    else
                    {
                        EndOfRunResults.Instance.SetMatchData(true);
                        RunManagerSingleton.instance.SetResultUI(!isGoodTeam);
                        Debug.Log("SFX PLAYED");
                        SFXManager.instance.PlaySFX(SFXType.Win, 0.12f, false);
                        MusicManager.Instance.Stop();
                        YouWinLose.DOColor(youwinlosecolor, 1);
                        DetailsImage.DOColor(DetailsColor, 1);
                        DetailsText.DOColor(DetailsTextColor, 1);
                        YouWinLose.text = "You Win!\n<size=12>[Click to continue]";
                    }
                }


                if (Input.GetMouseButtonDown(0) && !PostBattleContainer.activeSelf && (EventSystem.current.currentSelectedGameObject == null || EventSystem.current.currentSelectedGameObject.name != DetailsImage.name))
                {
                    Chimera[] chimeras = FindObjectsByType<Chimera>(FindObjectsSortMode.None);

                    for (int i = 0; i < chimeras.Length; i++)
                    {
                        Destroy(chimeras[i].gameObject);
                    }

                    MapManager.Instance.ToggleRectangles();
                    foreach (var monsterController in RunManagerSingleton.instance.goodTeam.GetCharactersAll())
                    {
                        monsterController.monsterIngameStats.currentShield = 0;
                        monsterController.statsManagerUI.SetShield(Mathf.RoundToInt(0));
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


    public float GetTeamTotalHPBar()
    {
        var allCharacters = GetCharactersAll();

        return allCharacters.Sum(x => x.monsterIngameStats.currentHP) / allCharacters.Sum(x => x.monsterIngameStats.maxHP);
    }

    void FixedUpdate()
    {
        if (battleStarted)
        {
            foreach (var character in characters)
            {
                if (character.Key == null)
                    continue;
                if (character.Key.monsterBehavior.monsterState == MonsterState.Killed ||
                    character.Key.monsterBehavior.monsterState == MonsterState.Stunned || character.Key.monsterBehavior.shrimping)
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
                        character.Key.CurrentTarget = null;
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

    public void AddCharacter(MonsterController character, Vector2Int? gridLocation = null)
    {
        OverlayTile startingTile = null;
        if (isGoodTeam)
        {
            landType = RunManagerSingleton.instance.landType;
            startingTile = MapManager.Instance.map[gridLocation.Value];
        }
        else
        {
            gridLocation = new Vector2Int(Mathf.Abs(character.startingRow - 7), Mathf.Abs(character.startingCol - 4));


            startingTile = MapManager.Instance.map[gridLocation.Value];
        }

        PositionCharacterOnTile(character, startingTile, true);
        character.transform.localScale = Vector3.one;
        MonsterScale.SetFacing(character.transform, gridLocation.Value.x >= 4);

        character.startingCol = startingTile.grid2DLocation.y;
        character.startingRow = startingTile.grid2DLocation.x;

        characters[character] = new CharacterState
        {
            path = new List<OverlayTile>(),
            isMoving = false
        };
    }


    private bool IsPathPossible(MonsterController monsterController, MonsterController enemy)
    {
        return pathFinder.FindPath(monsterController.standingOnTile, enemy.standingOnTile,
            GetInRangeTiles(monsterController)) != null;
    }

    private void MoveTowardsClosestCharacter(MonsterController character)
    {
        //if (character.monsterBehavior.monsterState != MonsterState.Moving && character.monsterBehavior.monsterState != MonsterState.None && character.monsterBehavior.monsterState != MonsterState.Idle || character.monsterBehavior.monsterState != MonsterState.Casting)
        //{
        //    if (character.CurrentTarget != null && !character.CurrentTarget.monsterSkillEffectManager.IsStenched())
        //    {
        //        var step = speed * Time.fixedDeltaTime;
        //        if (Vector3.Distance(character.transform.position, character.standingOnTile.transform.position) > 0.1f)
        //        {
        //            character.transform.position = Vector3.MoveTowards(character.transform.position,
        //                character.standingOnTile.transform.position, step);
        //        }

        //        return;
        //    }
        //}

        if (character.CurrentTarget != null)
        {
            if (character.Range == 1 && !IsPathPossible(character, character.CurrentTarget))
            {
                character.CurrentTarget = null;
            }
        }

        var state = characters[character];
        if (state.isMoving) return;
        MonsterController closestCharacter = character.CurrentTarget;


        if (character.CurrentTarget == null)
        {
            closestCharacter = FindClosestCharacter(character);
        }

        if (closestCharacter != null)
        {
            int distanceToClosestCharacterGrid =
                GetManhattanDistance(character.standingOnTile, closestCharacter.standingOnTile);

            character.CurrentTarget = closestCharacter;
            if (character.monsterIngameStats.Range > 1)
            {
                if (distanceToClosestCharacterGrid <= (int)character.monsterIngameStats.Range)
                {
                    state.path = null;

                    var step = speed * Time.fixedDeltaTime;
                    if (Vector3.Distance(character.transform.position,
                            character.standingOnTile.transform.position) > 0.1f)
                    {
                        character.transform.position = Vector3.MoveTowards(character.transform.position,
                            character.standingOnTile.transform.position, step);
                    }


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
            if (distanceToClosestCharacterGrid > character.monsterIngameStats.Range + 1 ||
                distanceToClosestCharacterGrid >= 1 &&
                (character.standingOnTile.grid2DLocation.y == closestCharacter.standingOnTile.grid2DLocation.y
                    ? distanceToClosestCharacterX > character.monsterIngameStats.Range
                    : distanceToClosestCharacterX > 0.1f ||
                      distanceToClosestCharacterGrid <= character.monsterIngameStats.Range + 1))
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

    private MonsterController FindClosestCharacter(MonsterController character)
    {
        MonsterController closestCharacter = null;
        int minManhattanDistance = int.MaxValue;
        float minTransformDistance = float.MaxValue;

        var characters = enemyTeam.GetAliveCharacters();
        var stenchCount = characters.Count(x => x.monsterSkillEffectManager.IsStenched());
        if (stenchCount > 0 && characters.Count != stenchCount)
        {
            characters.RemoveAll(x => x.monsterSkillEffectManager.IsStenched());
        }

        foreach (var other in characters)
        {
            int manhattanDistance =
                Mathf.Abs(character.standingOnTile.gridLocation.x - other.standingOnTile.gridLocation.x) +
                Mathf.Abs(character.standingOnTile.gridLocation.z - other.standingOnTile.gridLocation.z);

            if ((int)character.monsterIngameStats.Range == 1)
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

    public MonsterController FindFurthestCharacter(MonsterController character)
    {
        MonsterController furthestCharacter = null;
        int maxManhattanDistance = 0;
        var characters = enemyTeam.GetAliveCharacters();

        var stenchCount = characters.Count(x => x.monsterSkillEffectManager.IsStenched());
        if (stenchCount > 0 && characters.Count != stenchCount)
        {
            characters.RemoveAll(x => x.monsterSkillEffectManager.IsStenched());
        }
        foreach (var other in characters)
        {
            int manhattanDistance =
                Mathf.Abs(character.standingOnTile.gridLocation.x - other.standingOnTile.gridLocation.x) +
                Mathf.Abs(character.standingOnTile.gridLocation.z - other.standingOnTile.gridLocation.z);

            if ((int)character.monsterIngameStats.Range == 1)
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

    public MonsterController FindFurthestCharacter(MonsterController character, List<MonsterController> potentialCharacters)
    {
        MonsterController furthestCharacter = null;
        int maxManhattanDistance = 0;

        foreach (var other in potentialCharacters)
        {
            int manhattanDistance =
                Mathf.Abs(character.standingOnTile.gridLocation.x - other.standingOnTile.gridLocation.x) +
                Mathf.Abs(character.standingOnTile.gridLocation.z - other.standingOnTile.gridLocation.z);

            if ((int)character.monsterIngameStats.Range == 1)
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

    private void MoveAlongPath(MonsterController character, CharacterState state)
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
            if (character.monsterIngameStats.Range > 1)
            {
                if (distanceToClosestCharacterGrid <= (int)character.monsterIngameStats.Range)
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

    private void PositionCharacterOnTile(MonsterController character, OverlayTile tile, bool force = false)
    {
        if (!tile.occupied || force)
        {
            if (character.standingOnTile != null)
            {
                character.standingOnTile.occupied = false;
            }
            character.transform.position = tile.transform.position;
            tile.occupied = true;
            tile.currentOccupier = character;
            character.standingOnTile = tile;
        }
    }

    public List<OverlayTile> GetInRangeTiles(MonsterController character)
    {
        return MapManager.Instance.overlayTiles.Where(t => !t.occupied)
            .ToList();
    }

    public class CharacterState
    {
        public List<OverlayTile> path;
        public bool isMoving;
    }
}
