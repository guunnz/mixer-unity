using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using Spine.Unity;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using Random = System.Random;

[System.Serializable]
public class IngameStats
{
    public float Range = 1f;
    public string axieId;
    public float AttackSpeed;
    public float MinEnergy;
    public float MaxEnergy;
    public float CurrentEnergy;
    public float maxHP;
    public float currentHP;
    public float currentShield;
    public int totalComboCost;
    public AxieClass axieClass;
}

public enum AxieMode
{
    Battle,
    Menu,
    Win
}

public class AxieController : MonoBehaviour
{
    internal int AxieId;
    internal AxieSkillEffectManager axieSkillEffectManager;
    private AxieController[] allCharacters;
    private Team.CharacterState state;
    public AxieSkillController axieSkillController;
    public AxieController CurrentTarget;
    public Team enemyTeam;
    public Team myTeam;
    public AxieBehavior axieBehavior;
    public IngameStats axieIngameStats;
    private OverlayTile _standingOnTile;
    public OverlayTile standingOnTile
    {
        get { return _standingOnTile; }
        set
        {
            // If the current tile is not null, set its 'occupied' to false
            if (_standingOnTile != null)
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
            }
        }
    }
    public GetAxiesExample.Stats stats;
    public SkeletonAnimation SkeletonAnim;

    public SkeletonDataAsset skeletonDataAsset;
    public Material skeletonMaterial;

    public StatsManager statsManagerUI;
    public List<SkillName> axieBodyParts = new List<SkillName>();
    internal int startingCol;
    internal int startingRow;
    public Team goodTeam;
    public Team badTeam;
    internal bool imGood;
    public int Range = 1;
    public string Genes;
    public bool ShrimpOnStart;
    internal bool Shrimped = false;
    public AxieMode mode;
    private Vector3 lastMovedPosition;
    private float TimerMove = 0f;
    private Coroutine menuWalkCoroutine;
    public List<AxieController> GetAdjacent()
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
        axieSkillController.OnBattleStart();
    }

    public Vector3 GetPartPosition(BodyPart part)
    {
        return transform.position;
    }

    public void AddStatusEffect(SkillEffect skillEffect)
    {
        axieSkillEffectManager.AddStatusEffect(skillEffect);
    }

    public void DoHeal(float healAmount)
    {
        if (healAmount <= 0 || this.axieBehavior.axieState == AxieState.Killed)
            return;
        if (this.axieSkillEffectManager.IsHealingBlocked())
            return;
        this.axieIngameStats.currentHP += healAmount;
    }


    public void RemoveStatusEffect(SkillEffect skillEffect)
    {
        axieSkillEffectManager.RemoveAllEffects();
    }

    public List<SkillEffect> GetAllSkillEffectsNotPassives()
    {
        return axieSkillEffectManager.GetAllSkillEffectsNotPassives().ToList();
    }

    public void RemoveAllEffects()
    {
        axieSkillEffectManager.RemoveAllEffects();
    }

    public void UpdateStats()
    {
        if (axieIngameStats.axieClass == AxieClass.Bird || axieIngameStats.axieClass == AxieClass.Dusk ||
            axieIngameStats.axieClass == AxieClass.Bug)
        {
            axieIngameStats.Range = 4;
            Range = 4;
        }

        axieIngameStats.maxHP = AxieStatCalculator.GetHP(stats);

        axieIngameStats.currentHP = axieIngameStats.maxHP;
    }

    private IEnumerator Start()
    {
        yield return new WaitForFixedUpdate();
        goodTeam = FindObjectsOfType<Team>().Single(x => x.isGoodTeam);
        badTeam = FindObjectsOfType<Team>().Single(x => !x.isGoodTeam);

        axieSkillController.self = this;

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

        state = myTeam.GetCharacterState(axieIngameStats.axieId);


        yield return new WaitForFixedUpdate();


        axieSkillController.self = this;


        UpdateStats();
        if (imGood)
        {
            SkeletonAnim.GetComponent<Renderer>().sortingOrder =
                (int)MathHelpers.InvLerp(0, 7, (standingOnTile.grid2DLocation.y + 1));
        }
        else
        {
            SkeletonAnim.GetComponent<Renderer>().sortingOrder =
                (int)MathHelpers.InvLerp(0, 7, (standingOnTile.grid2DLocation.y));
        }

        axieBehavior.myController = this;
        SkeletonAnim.loop = true;
        axieBehavior.axieSkillEffectManager = axieSkillEffectManager;
        axieBehavior.DoAction(AxieState.Idle);
    }

    private void OnMouseOver()
    {
        if (EventSystem.current.IsPointerOverGameObject() || mode == AxieMode.Win)
            return;
        if (!goodTeam.battleStarted)
        {
            if (mode == AxieMode.Menu)
            {
                transform.DOKill();
            }

            axieBehavior.DoAction(AxieState.Hovered);
        }
    }

    private void OnMouseExit()
    {
        if (!goodTeam.battleStarted)
            axieBehavior.DoAction(AxieState.Idle);
    }

    public void ChangeMode(AxieMode mode)
    {
        if (mode == AxieMode.Battle)
        {
            this.mode = AxieMode.Battle;
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
            if (this.mode == AxieMode.Battle)
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

        transform.localScale = new Vector3(targetX > this.transform.position.x ? -0.2f : 0.2f, transform.localScale.y,
            transform.localScale.z);

        // Calculate distance to the target position
        float distance = Vector3.Distance(transform.position, targetPosition);

        // Calculate moveDuration based on distance and timePerMeter
        float moveDuration = distance * timePerMeter;

        TimerMove = moveDuration + UnityEngine.Random.Range(0, 3f);
        // Set animation to run
        SkeletonAnim.AnimationName = "action/run";

        // Move towards the target position using DOTween
        transform.DOMove(targetPosition, changingFromMenuToBattle ? moveDuration / 2.5f : moveDuration)
            .SetEase(Ease.Linear) // Use linear easing for constant speed
            .OnComplete(() =>
            {
                // Set animation to idle when movement is completed
                SkeletonAnim.AnimationName = "action/idle/normal";
                if (changingFromMenuToBattle)
                {
                    statsManagerUI.gameObject.SetActive(true);
                    this.mode = AxieMode.Battle;
                    transform.localScale = new Vector3(-0.2f, transform.localScale.y, transform.localScale.z);
                }
            });

        yield return null;
    }
    private bool animationBattleSet = false;
    private void Update()
    {
        if (mode == AxieMode.Win)
            return;
        if (mode == AxieMode.Menu && axieBehavior.axieState != AxieState.Hovered)
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
        else if (!animationBattleSet && SkeletonAnim.AnimationName != "action/idle/normal" && axieBehavior.axieState != AxieState.Hovered)
        {
            animationBattleSet = true;
            SkeletonAnim.AnimationName = "action/idle/normal";
        }
    }

    private void FixedUpdate()
    {
        if (mode == AxieMode.Menu || mode == AxieMode.Win)
            return;

        if (goodTeam == null)
            return;

        if (CurrentTarget != null && CurrentTarget.axieIngameStats.currentHP <= 0)
        {
            CurrentTarget = null;
        }

        if (axieBehavior.axieState == AxieState.Shrimping || !goodTeam.battleStarted)
            return;

        if (Input.GetKeyDown(KeyCode.H))
        {
            if (axieIngameStats.axieClass == AxieClass.Aquatic)
            {
                //axieBehavior.DoAction(AxieState.Shrimping);
                return;
            }
        }

        if (ShrimpOnStart && !Shrimped && CurrentTarget != null)
        {
            axieBehavior.DoAction(AxieState.Shrimping);
            Shrimped = true;
            return;
        }

        if (badTeam.GetAliveCharacters().Count == 0)
        {
            Shrimped = false;
            axieBehavior.DoAction(AxieState.Victory);
            return;
        }

        if (axieIngameStats.currentHP > axieIngameStats.maxHP)
        {
            axieIngameStats.currentHP = axieIngameStats.maxHP;
        }


        if (!axieSkillEffectManager.IsChilled() && !axieSkillEffectManager.IsStunned() && axieBehavior.axieState != AxieState.Casting)
        {
            axieIngameStats.CurrentEnergy += 0.0015f + ((stats.skill * 4) / 10000f);
        }


        if (axieIngameStats.currentHP <= 0)
        {
            Shrimped = false;
            axieBehavior.DoAction(AxieState.Killed);
            return;
        }
        else
        {
            if (axieIngameStats.CurrentEnergy >= axieIngameStats.MaxEnergy)
            {
                axieIngameStats.CurrentEnergy = axieIngameStats.MaxEnergy;
            }

            if (axieBehavior.axieState != AxieState.Casting)
            {
                SetEnergy();
            }

            statsManagerUI.SetHP(axieIngameStats.currentHP / axieIngameStats.maxHP);

            statsManagerUI.SetShield(Mathf.RoundToInt(axieIngameStats.currentShield));
        }

        if (axieBehavior.axieState == AxieState.Killed)
            this.gameObject.SetActive(false);

        if (axieBehavior.axieState == AxieState.Casting)
            return;

        if (state == null)
            return;

        if (axieBehavior.axieState == AxieState.Grabbed)
        {
            CurrentTarget = null;
            axieBehavior.DoAction(AxieState.Grabbed);
            SkeletonAnim.loop = true;
            return;
        }
        else if (axieBehavior.axieState == AxieState.Hovered && CurrentTarget == null)
        {
            axieBehavior.DoAction(AxieState.Hovered);
            return;
        }
        else if (state.isMoving == false && CurrentTarget == null)
        {
            axieBehavior.DoAction(AxieState.Idle);
            return;
        }

        if (axieIngameStats.CurrentEnergy >= axieSkillController.GetComboCost())
        {
            axieBehavior.DoAction(AxieState.Casting);
            return;
        }

        if (CurrentTarget != null && state.isMoving == false && CurrentTarget.standingOnTile != null &&
            standingOnTile != null && GetManhattanDistance(this.standingOnTile, CurrentTarget.standingOnTile) <=
            axieIngameStats.Range
           )
        {
            axieBehavior.DoAction(AxieState.Attacking);

            if (CurrentTarget == null)
            {
                return;
            }

            if (CurrentTarget.standingOnTile.grid2DLocation.y != standingOnTile.grid2DLocation.y)
            {
                if (imGood)
                {
                    transform.localScale = new Vector3(-0.2f, transform.localScale.y, transform.localScale.z);
                }
                else
                {
                    transform.localScale = new Vector3(0.2f, transform.localScale.y, transform.localScale.z);
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
                transform.localScale = new Vector3(moved && direction.x > 0 ? -0.2f : 0.2f, transform.localScale.y,
                    transform.localScale.z);

                // Update the lastMovedPosition
                if (moved)
                {
                    lastMovedPosition = currentPosition;
                }
                else
                {
                    // If no significant movement, use the target position
                    transform.localScale =
                        new Vector3(CurrentTarget.transform.position.x > this.transform.position.x ? -0.2f : 0.2f,
                            transform.localScale.y, transform.localScale.z);
                }
            }

            SkeletonAnim.loop = true;
        }
        else if (state.isMoving)
        {
            SkeletonAnim.AnimationName = "action/run";
            SkeletonAnim.loop = true;
            if (CurrentTarget == null)
                return;
            transform.localScale =
                new Vector3(CurrentTarget.transform.position.x > this.transform.position.x ? -0.2f : 0.2f,
                    transform.localScale.y, transform.localScale.z);
        }
        else
        {
            SkeletonAnim.AnimationName = "action/idle/normal";
            SkeletonAnim.loop = true;
        }
    }


    public void SetEnergy()
    {
        statsManagerUI.SetMana((float)Math.Round(axieIngameStats.CurrentEnergy, 2) /
                           axieSkillController.GetComboCost());
    }
    private int GetManhattanDistance(OverlayTile tile1, OverlayTile tile2)
    {
        return Mathf.Abs(tile1.gridLocation.x - tile2.gridLocation.x) +
               Mathf.Abs(tile1.gridLocation.z - tile2.gridLocation.z);
    }
}