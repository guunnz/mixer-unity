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
        internal float MinManaAux = 0;

        public SkillName skillName;
        public AxieClass axieClass;
        public BodyPart bodyPart;

        public float HP = 100;
        private float attackSpeedTime = 0;
        private float attackSpeedDuration = 1f;

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

            SkeletonAnim = transform.GetChild(0).GetComponent<SkeletonAnimation>();
            SetAllCharacters();
            if (imGood)
            {
                SkeletonAnim.GetComponent<Renderer>().sortingOrder = 50;
            }
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
            if (imGood && badTeam.GetCharacters().All(x => !x.gameObject.activeSelf) ||
                !imGood && goodTeam.GetCharacters().All(x => !x.gameObject.activeSelf))
            {
                fighting = false;
                SkeletonAnim.AnimationName = "activity/victory-pose-back-flip";
                SkeletonAnim.loop = true;
                return;
            }

            if (HP <= 0)
            {
                Killed = true;
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