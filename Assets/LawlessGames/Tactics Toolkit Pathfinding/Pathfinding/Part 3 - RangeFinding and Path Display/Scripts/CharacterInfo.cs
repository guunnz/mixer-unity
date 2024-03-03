using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Spine.Unity;
using UnityEngine;
using UnityEngine.Serialization;

namespace finished3
{
    public class CharacterInfo : MonoBehaviour
    {
        public OverlayTile standingOnTile;
        public string axieId;
        public float Range = 1f;
        public CharacterInfo CurrentTarget;
        public SkeletonAnimation SkeletonAnim;
        public bool Grabbed = false;
        [FormerlySerializedAs("hpManager")] public StatsManager statsManager;

        public bool beingHovered;

        [FormerlySerializedAs("movementController")]
        public MyTeam goodTeam;

        [FormerlySerializedAs("movementController2")]
        public EnemyTeam badTeam;

        public bool Killed;

        private MyTeam.CharacterState state;

        private bool fighting = false;
        private CharacterInfo[] allCharacters;

        private bool imGood;
        public bool CastingSpell;

        public float Mana = 0;
        private float MaxManaAux = 50;
        private float MaxHPAux = 50;
        internal float MinManaAux = 0;

        public SkillName skillName;
        public AxieClass axieClass;
        public BodyPart bodyPart;

        internal float HP;
        private float attackSpeedTime = 0;
        private float attackSpeedDuration = 1f;

        public bool shrimping = false;
        private void Start()
        {
            if (this.standingOnTile.grid2DLocation.x >= 4)
            {
                badTeam = FindObjectOfType<EnemyTeam>();
                goodTeam = FindObjectOfType<MyTeam>();
                state = badTeam.GetCharacterState(axieId);
            }
            else
            {
                imGood = true;
                badTeam = FindObjectOfType<EnemyTeam>();
                goodTeam = FindObjectOfType<MyTeam>();
                state = goodTeam.GetCharacterState(axieId);
            }

            if (this.axieClass == AxieClass.Bird)
                Range = 4;


            MaxHPAux = HP;
            SetAllCharacters();
            if (imGood)
            {
                SkeletonAnim.GetComponent<Renderer>().sortingOrder = 50;
            }


        }
        
        public IEnumerator GoBackdoor()
        {
            shrimping = true;
            SkeletonAnim.AnimationName = "attack/melee/shrimp";
            
            yield return new WaitForSeconds(SkeletonAnim.AnimationState.GetCurrent(0).AnimationEnd/2);
            SkeletonAnim.GetComponent<Renderer>().enabled = false;
            Vector3 positionToMove = Vector3.zero;
            List<OverlayTile> possibleTiles = goodTeam.GetInRangeTiles(this);

            List<OverlayTile> jumpToTiles = possibleTiles.Where(x => x.grid2DLocation.x == (imGood ? 7 : 0)).ToList();



            OverlayTile overlayTile = jumpToTiles.FirstOrDefault(x => x.grid2DLocation.y == this.standingOnTile.grid2DLocation.y);
            if (jumpToTiles.Count == 0 || overlayTile == null)
            {
                jumpToTiles = possibleTiles.Where(x => x.grid2DLocation.x == (imGood ? 6 : 1)).ToList();
            }
            
            overlayTile = jumpToTiles.FirstOrDefault(x => x.grid2DLocation.y == this.standingOnTile.grid2DLocation.y);

            
            
            if (overlayTile != null)
            {
                positionToMove = overlayTile.transform.position;
            }

            this.standingOnTile = overlayTile;
            this.transform.position = positionToMove;
            
            yield return new WaitForSeconds(0.1f);
            SkeletonAnim.GetComponent<Renderer>().enabled = true;
            yield return new WaitForSeconds(SkeletonAnim.AnimationState.GetCurrent(0).AnimationEnd/2 - 0.1f);

            this.transform.localScale = new Vector3(this.transform.localScale.x * -1, this.transform.localScale.y,
                this.transform.localScale.z);
            shrimping = false;
        }

        
        
        public void SetAllCharacters()
        {
            allCharacters = FindObjectsOfType<CharacterInfo>();
        }

        private void OnMouseOver()
        {
            // This method is called when the mouse is over the collider of this GameObject
            beingHovered = true;
        }

        private void OnMouseExit()
        {
            // This method is called when the mouse leaves the collider
            beingHovered = false;
        }

        public void CastSpell()
        {
            CastingSpell = true;
            SkeletonAnim.loop = false;
            StartCoroutine(ICastSpell());
        }

        IEnumerator ICastSpell()
        {
            float timeToWait = SkillLauncher.Instance.ThrowSkill(skillName, axieClass, bodyPart,
                CurrentTarget.transform, this.transform, SkeletonAnim,
                skillName == SkillName.Rosebud ? this : CurrentTarget);
            yield return new WaitForSeconds(timeToWait);
            CastingSpell = false;
            Mana = MinManaAux;
            SkeletonAnim.loop = true;
        }

        private void Update()
        {
            if (shrimping)
                return;
            if (Input.GetKeyDown(KeyCode.H))
            {
                if (axieClass == AxieClass.Aquatic)
                {
                    StartCoroutine(GoBackdoor());
                    return;
                }
            }
            if (imGood && badTeam.GetCharacters().All(x => !x.gameObject.activeSelf) ||
                !imGood && goodTeam.GetCharacters().All(x => !x.gameObject.activeSelf))
            {
                fighting = false;
                SkeletonAnim.AnimationName = "activity/victory-pose-back-flip";
                SkeletonAnim.loop = true;
                return;
            }

            if (HP > MaxHPAux)
            {
                HP = MaxHPAux;
            }

            if (HP <= 0)
            {
                Killed = true;
            }
            else
            {
                statsManager.SetMana(Mana / MaxManaAux);
                statsManager.SetHP(HP / MaxHPAux);
            }

            if (Killed)
                this.gameObject.SetActive(false);

            if (CastingSpell)
                return;

            if (state == null)
                return;

            if (Grabbed)
            {
                CurrentTarget = null;
                SkeletonAnim.AnimationName = "action/idle/random-03";
                SkeletonAnim.loop = true;
                return;
            }
            else if (beingHovered && CurrentTarget == null)
            {
                SkeletonAnim.AnimationName = "action/idle/random-01";
                SkeletonAnim.loop = true;
                return;
            }
            else if (state.isMoving == false && CurrentTarget == null)
            {
                fighting = false;
                SkeletonAnim.AnimationName = "action/idle/normal";
                SkeletonAnim.loop = true;
                return;
            }

            Mana += Time.deltaTime * 3;

            if (Mana >= MaxManaAux)
            {
                CastSpell();
                Mana = 0;
                return;
            }


            if (CurrentTarget != null && state.isMoving == false &&
                GetManhattanDistancePlayer(this.transform.position, CurrentTarget.transform.position) <= Range)
            {
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
                    transform.localScale =
                        new Vector3(CurrentTarget.transform.position.x > this.transform.position.x ? -0.2f : 0.2f,
                            transform.localScale.y, transform.localScale.z);
                }


                fighting = true;

                attackSpeedTime += Time.deltaTime;

                if (attackSpeedTime >= attackSpeedDuration)
                {
                    if (axieClass == AxieClass.Bird)
                    {
                        AutoAttackMaNAGER.instance.SpawnProjectileBird(CurrentTarget.transform, this.transform);
                    }

                    CurrentTarget.HP -= 5;
                    attackSpeedTime = 0;
                }

                if (axieClass == AxieClass.Bird)
                {
                    SkeletonAnim.AnimationName = "attack/ranged/cast-multi";
                }
                else if (axieClass == AxieClass.Aquatic)
                {
                    SkeletonAnim.AnimationName = "attack/melee/horn-gore";
                }
                else if (axieClass == AxieClass.Beast)
                {
                    SkeletonAnim.AnimationName = "attack/melee/tail-roll";
                }
                else if (axieClass == AxieClass.Plant)
                {
                    SkeletonAnim.AnimationName = "attack/melee/mouth-bite";
                }
                else
                {
                    SkeletonAnim.AnimationName = "attack/melee/normal-attack";
                }

                SkeletonAnim.loop = true;
                return;
            }
            else if (state.isMoving)
            {
                fighting = false;
                SkeletonAnim.AnimationName = "action/run";
                SkeletonAnim.loop = true;
                transform.localScale =
                        new Vector3(CurrentTarget.transform.position.x > this.transform.position.x ? -0.2f : 0.2f,
                            transform.localScale.y, transform.localScale.z);
                return;
            }
            else
            {
                fighting = false;
                SkeletonAnim.AnimationName = "action/idle/normal";
                SkeletonAnim.loop = true;
            }
        }

        private int GetManhattanDistancePlayer(Vector3 tile1, Vector3 tile2)
        {
            return Mathf.RoundToInt(Mathf.Abs(tile1.x - tile2.x) +
                                    Mathf.Abs(tile1.z - tile2.z));
        }

        private int GetManhattanDistance(OverlayTile tile1, OverlayTile tile2)
        {
            return Mathf.Abs(tile1.gridLocation.x - tile2.gridLocation.x) +
                   Mathf.Abs(tile1.gridLocation.z - tile2.gridLocation.z);
        }
    }
}