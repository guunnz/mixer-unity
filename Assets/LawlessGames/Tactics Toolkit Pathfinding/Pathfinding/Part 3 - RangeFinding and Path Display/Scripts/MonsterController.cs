using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using static GetMonstersExample;
using Random = System.Random;

[System.Serializable]
public class IngameStats
{
    public float Range = 1f;
    public string monsterId;
    public float AttackSpeed;
    public float MinEnergy;
    public float MaxEnergy;
    public float CurrentEnergy;
    public float maxHP;
    public float currentHP;
    public float currentShield;
    public int totalComboCost;
    public MonsterClass monsterClass;
}

public enum MonsterMode
{
    Battle,
    Menu,
    Win
}

public class MonsterController : MonoBehaviour
{
    internal int MonsterId;
    internal MonsterSkillEffectManager monsterSkillEffectManager;
    private MonsterController[] allCharacters;
    private Team.CharacterState state;
    public MonsterSkillController monsterSkillController;
    public MonsterController CurrentTarget;
    public Team enemyTeam;
    public Team myTeam;
    public MonsterBehavior monsterBehavior;
    public IngameStats monsterIngameStats;
    private OverlayTile _standingOnTile;
    public OverlayTile standingOnTile
    {
        get { return _standingOnTile; }
        set
        {
            // If the current tile is not null, set its 'occupied' to false
            if (_standingOnTile != null && _standingOnTile.currentOccupier == this)
            {
                _standingOnTile.occupied = false;
                _standingOnTile.currentOccupier = null;
            }

            // Set the new tile
            _standingOnTile = value;

            // If the new tile is not null, set its 'occupied' to true
            if (_standingOnTile != null)
            {
                _standingOnTile.occupied = true;
                _standingOnTile.currentOccupier = this;
            }
        }
    }
    public GetMonstersExample.Stats stats;
    public VanillaMonsterVisual Visual;
    public MonsterVisualDescriptor visualDescriptor;

    public StatsManager statsManagerUI;
    public List<SkillName> monsterBodyParts = new List<SkillName>();
    internal int startingCol;
    internal int startingRow;
    public Team goodTeam;
    public Team badTeam;
    internal bool imGood;
    public int Range = 1;
    public string Genes;
    public bool ShrimpOnStart;
    internal bool Shrimped = false;
    public MonsterMode mode;
    private Vector3 lastMovedPosition;
    private float TimerMove = 0f;
    private Coroutine menuWalkCoroutine;

    private void Awake()
    {
        MonsterScale.ApplyGrabCollider(gameObject);
    }

    public List<MonsterController> GetAdjacent()
    {
        try
        {
            if (this.standingOnTile == null)
                return null;
            if (MapManager.Instance.GetAdjacentTiles(this.standingOnTile).All(x => x.currentOccupier == null))
            {
                return null;
            }

            return MapManager.Instance.GetAdjacentTiles(this.standingOnTile)
                .Where(x => x.currentOccupier != null && x.currentOccupier.imGood == this.imGood)
                .Select(x => x.currentOccupier).ToList();
        }
        catch (Exception e)
        {
            return null;
        }
    }

    public void DoOnStart()
    {
        monsterSkillController.OnBattleStart();
    }

    public Vector3 GetPartPosition(BodyPart part)
    {
        return Visual != null ? Visual.GetPartAnchor(part).position : transform.position;
    }

    public void AddStatusEffect(SkillEffect skillEffect)
    {
        monsterSkillEffectManager.AddStatusEffect(skillEffect);
    }

    public void DoHeal(float healAmount, string monsterId)
    {
        if (healAmount <= 0 || this.monsterBehavior.monsterState == MonsterState.Killed)
            return;
        if (this.monsterSkillEffectManager.IsHealingBlocked())
            return;
        PostBattleManager.Instance.SumHealing(monsterId, healAmount, imGood);
        this.monsterIngameStats.currentHP += healAmount;
    }


    public void RemoveStatusEffect(SkillEffect skillEffect)
    {
        monsterSkillEffectManager.RemoveAllEffects();
    }

    public List<SkillEffect> GetAllSkillEffectsNotPassives()
    {
        return monsterSkillEffectManager.GetAllSkillEffectsNotPassives().ToList();
    }

    public void RemoveAllEffects()
    {
        monsterSkillEffectManager.RemoveAllEffects();
    }

    public void UpdateStats()
    {
        if (monsterIngameStats.monsterClass == MonsterClass.Bird || monsterIngameStats.monsterClass == MonsterClass.Dusk ||
            monsterIngameStats.monsterClass == MonsterClass.Bug)
        {
            monsterIngameStats.Range = 4;
            Range = 4;
        }

        monsterIngameStats.maxHP = MonsterStatCalculator.GetHP(stats);

        monsterIngameStats.currentHP = monsterIngameStats.maxHP;
    }

    private IEnumerator Start()
    {
        yield return new WaitForFixedUpdate();
        goodTeam = FindObjectsOfType<Team>().Single(x => x.isGoodTeam);
        badTeam = FindObjectsOfType<Team>().Single(x => !x.isGoodTeam);

        monsterSkillController.self = this;

        if (this.standingOnTile.grid2DLocation.x >= 4)
        {

        }
        else
        {
            imGood = true;
        }

        if (imGood)
        {
            enemyTeam = badTeam;
            myTeam = goodTeam;
        }
        else
        {
            enemyTeam = goodTeam;

            myTeam = badTeam;
        }



        if (imGood)
        {
            goodTeam = FindObjectsOfType<Team>().Single(x => x.isGoodTeam);
            badTeam = FindObjectsOfType<Team>().Single(x => !x.isGoodTeam);
        }
        else
        {
            goodTeam = FindObjectsOfType<Team>().Single(x => !x.isGoodTeam);
            badTeam = FindObjectsOfType<Team>().Single(x => x.isGoodTeam);
        }

        state = myTeam.GetCharacterState(monsterIngameStats.monsterId);


        yield return new WaitForFixedUpdate();


        monsterSkillController.self = this;


        UpdateStats();
        if (imGood)
        {
            Visual?.SetSortingOrder((int)MathHelpers.InvLerp(0, 7, (standingOnTile.grid2DLocation.y + 1)));
        }
        else
        {
            Visual?.SetSortingOrder((int)MathHelpers.InvLerp(0, 7, (standingOnTile.grid2DLocation.y)));
        }

        monsterBehavior.myController = this;
        Visual?.Play(MonsterVisualState.Idle, true);
        monsterBehavior.monsterSkillEffectManager = monsterSkillEffectManager;
        monsterBehavior.DoAction(MonsterState.Idle);
    }

    private void OnMouseOver()
    {
        if (EventSystem.current.IsPointerOverGameObject() || mode == MonsterMode.Win)
            return;
        if (!goodTeam.battleStarted)
        {
            if (mode == MonsterMode.Menu)
            {
                transform.DOKill();
            }

            monsterBehavior.DoAction(MonsterState.Hovered);
        }
    }

    private void OnMouseExit()
    {
        if (!goodTeam.battleStarted)
            monsterBehavior.DoAction(MonsterState.Idle);
    }

    public void ChangeMode(MonsterMode mode)
    {
        if (mode == MonsterMode.Battle)
        {
            this.mode = MonsterMode.Battle;
            TimerMove = 10000f;
            if (menuWalkCoroutine != null)
            {
                StopCoroutine(menuWalkCoroutine);
                menuWalkCoroutine = null;
            }
            menuWalkCoroutine = StartCoroutine(MoveToRandomPosition(true));
        }
        else
        {
            if (this.mode == MonsterMode.Battle)
            {
                TimerMove = UnityEngine.Random.Range(2.5f, 10f);
            }
            statsManagerUI.gameObject.SetActive(false);
            this.mode = mode;
        }
    }

    public float timePerMeter = 0.8f; // Time it takes to move 1 meter

    IEnumerator MoveToRandomPosition(bool changingFromMenuToBattle = false)
    {
        transform.DOKill();
        float targetX =
            UnityEngine.Random.Range(MapManager.Instance.minMapBounds.x, MapManager.Instance.maxMapBounds.x);
        float targetY =
            UnityEngine.Random.Range(MapManager.Instance.minMapBounds.y, MapManager.Instance.maxMapBounds.y);

        if (standingOnTile != null)
        {
            if (changingFromMenuToBattle)
            {
                targetX = standingOnTile.grid2DLocation.x;
                targetY = standingOnTile.grid2DLocation.y;
            }
        }


        Vector3 targetPosition = new Vector3(targetX, 0, targetY);

        MonsterScale.SetFacing(transform, targetX <= this.transform.position.x);

        // Calculate distance to the target position
        float distance = Vector3.Distance(transform.position, targetPosition);

        // Calculate moveDuration based on distance and timePerMeter
        float moveDuration = distance * timePerMeter;

        TimerMove = moveDuration + UnityEngine.Random.Range(0, 3f);
        // Set animation to run
        Visual?.Play(MonsterVisualState.Run, true);

        // Move towards the target position using DOTween
        transform.DOMove(targetPosition, changingFromMenuToBattle ? moveDuration / 2.5f : moveDuration)
            .SetEase(Ease.Linear) // Use linear easing for constant speed
            .OnComplete(() =>
            {
                // Set animation to idle when movement is completed
                Visual?.Play(MonsterVisualState.Idle, true);
                if (changingFromMenuToBattle)
                {
                    statsManagerUI.gameObject.SetActive(true);
                    this.mode = MonsterMode.Battle;
                    MonsterScale.SetFacing(transform, false);
                }
            });

        yield return null;
    }
    private bool animationBattleSet = false;
    private void Update()
    {
        if (mode == MonsterMode.Win)
            return;
        if (mode == MonsterMode.Menu && monsterBehavior.monsterState != MonsterState.Hovered)
        {
            animationBattleSet = false;
            if (TimerMove <= 0)
            {
                menuWalkCoroutine = StartCoroutine(MoveToRandomPosition());
            }
            else
            {
                TimerMove -= Time.deltaTime;
            }
        }
        else if (!animationBattleSet && Visual != null && Visual.CurrentState != MonsterVisualState.Idle && monsterBehavior.monsterState != MonsterState.Hovered)
        {
            animationBattleSet = true;
            Visual.Play(MonsterVisualState.Idle, true);
        }
    }

    private void FixedUpdate()
    {
        if (mode == MonsterMode.Menu || mode == MonsterMode.Win || monsterBehavior.monsterState == MonsterState.Killed)
            return;

        if (goodTeam == null)
            return;

        if (CurrentTarget != null && CurrentTarget.monsterIngameStats.currentHP <= 0)
        {
            CurrentTarget = null;
        }
        if (monsterBehavior.monsterState == MonsterState.Shrimping || !goodTeam.battleStarted)
            return;

        if (Input.GetKeyDown(KeyCode.H))
        {
            if (monsterIngameStats.monsterClass == MonsterClass.Aquatic)
            {
                //monsterBehavior.DoAction(MonsterState.Shrimping);
                return;
            }
        }

        if (ShrimpOnStart && !Shrimped && CurrentTarget != null)
        {
            monsterBehavior.DoAction(MonsterState.Shrimping);
            Shrimped = true;
            return;
        }

        if (badTeam.MonsterAliveAmount == 0)
        {
            Shrimped = false;
            monsterBehavior.DoAction(MonsterState.Victory);
            return;
        }

        if (monsterIngameStats.currentHP > monsterIngameStats.maxHP)
        {
            monsterIngameStats.currentHP = monsterIngameStats.maxHP;
        }


        if (!monsterSkillEffectManager.IsChilled() && !monsterSkillEffectManager.IsStunned() && monsterBehavior.monsterState != MonsterState.Casting)
        {
            monsterIngameStats.CurrentEnergy += 0.001f + ((stats.skill * 2) / 10000f);
        }


        if (monsterIngameStats.currentHP <= 0)
        {
            Shrimped = false;
            monsterBehavior.DoAction(MonsterState.Killed);
            monsterBehavior.monsterState = MonsterState.Killed;
            return;
        }
        else
        {
            if (monsterIngameStats.CurrentEnergy >= monsterIngameStats.MaxEnergy)
            {
                monsterIngameStats.CurrentEnergy = monsterIngameStats.MaxEnergy;
            }

            if (monsterBehavior.monsterState != MonsterState.Casting)
            {
                SetEnergy();
            }

            statsManagerUI.SetHP(monsterIngameStats.currentHP / monsterIngameStats.maxHP);

            statsManagerUI.SetShield(Mathf.RoundToInt(monsterIngameStats.currentShield));
        }

        if (monsterBehavior.monsterState == MonsterState.Killed)
            this.gameObject.SetActive(false);

        if (monsterBehavior.monsterState == MonsterState.Casting)
            return;

        if (state == null)
            return;

        if (monsterBehavior.monsterState == MonsterState.Grabbed)
        {
            CurrentTarget = null;
            monsterBehavior.DoAction(MonsterState.Grabbed);
            return;
        }
        else if (monsterBehavior.monsterState == MonsterState.Hovered && CurrentTarget == null)
        {
            monsterBehavior.DoAction(MonsterState.Hovered);
            return;
        }
        else if (state.isMoving == false && CurrentTarget == null)
        {
            monsterBehavior.DoAction(MonsterState.Idle);
            return;
        }

        if (monsterIngameStats.CurrentEnergy >= monsterSkillController.GetComboCost())
        {
            monsterBehavior.DoAction(MonsterState.Casting);
            return;
        }

        if (CurrentTarget != null && state.isMoving == false && CurrentTarget.standingOnTile != null &&
            standingOnTile != null && GetManhattanDistance(this.standingOnTile, CurrentTarget.standingOnTile) <=
            monsterIngameStats.Range
           )
        {
            monsterBehavior.DoAction(MonsterState.Attacking);

            if (CurrentTarget == null)
            {
                return;
            }

            if (CurrentTarget.standingOnTile.grid2DLocation.y != standingOnTile.grid2DLocation.y)
            {
                if (imGood)
                {
                    MonsterScale.SetFacing(transform, false);
                }
                else
                {
                    MonsterScale.SetFacing(transform, true);
                }
            }
            else
            {
                // Assuming targetX and targetY are defined elsewhere

                // Compare the last moved position with the current position
                Vector3 currentPosition = transform.position;
                Vector3 direction = currentPosition - lastMovedPosition;

                // Check if the direction is significant (not negligible)
                float epsilon = 0.001f;
                bool moved = direction.magnitude > epsilon;

                // Update the scale based on movement direction or use the target position if there's no significant movement
                MonsterScale.SetFacing(transform, !moved || direction.x <= 0);

                // Update the lastMovedPosition
                if (moved)
                {
                    lastMovedPosition = currentPosition;
                }
                else
                {
                    // If no significant movement, use the target position
                    MonsterScale.SetFacing(transform, CurrentTarget.transform.position.x <= this.transform.position.x);
                }
            }

        }
        else if (state.isMoving)
        {
            monsterBehavior.DoAction(MonsterState.Moving);
            if (CurrentTarget == null)
                return;
            MonsterScale.SetFacing(transform, CurrentTarget.transform.position.x <= this.transform.position.x);
        }
        else
        {
            monsterBehavior.DoAction(MonsterState.Idle);
        }
    }


    public void SetEnergy()
    {
        statsManagerUI.SetMana((float)Math.Round(monsterIngameStats.CurrentEnergy, 2) /
                           monsterSkillController.GetComboCost());
    }
    private int GetManhattanDistance(OverlayTile tile1, OverlayTile tile2)
    {
        return Mathf.Abs(tile1.gridLocation.x - tile2.gridLocation.x) +
               Mathf.Abs(tile1.gridLocation.z - tile2.gridLocation.z);
    }
}
